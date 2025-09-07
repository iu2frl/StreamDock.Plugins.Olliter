#region Using directives
using System.Drawing;
using System.Text.Json.Serialization;
using BarRaider.SdTools;
using BarRaider.SdTools.Payloads;
using Coordinates;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endregion

namespace StreamDock.Plugins.Payload
{
    public class StreamDock
    {
        public static Bitmap? UpdateKeyImage(string value, Color? bgColor = null)
        {
            Bitmap bmp;
            try
            {
                bmp = new Bitmap(ImageHelper.GetImage(bgColor ?? Color.Black));

                bmp = new Bitmap(ImageHelper.SetImageText(bmp, value, new SolidBrush(Color.White), 72, 72));
                return bmp;

            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, ex.Message);
                Logger.Instance.LogMessage(TracingLevel.ERROR, ex.StackTrace);
            }

            return null;
        }
    }

    #region Custom classes

    #region Olliter commands

    public class ReceiverCommand
    {
        [JsonPropertyName("software_id")]
        public string SoftwareId { get; set; } = Environment.MachineName;

        [JsonPropertyName("command")]
        public required string Command { get; set; }

        [JsonPropertyName("action")]
        public required string Action { get; set; }

        [JsonPropertyName("value")]
        public required string Value { get; set; } = "";

        [JsonPropertyName("subreceiver")]
        public required string SubReceiver { get; set; } = "false";
    }

    public class ReceiverStatus
    {
        [JsonPropertyName("software_id")]
        public string SoftwareId { get; set; } = Environment.MachineName;

        [JsonPropertyName("txpower")]
        public string? TxPower { get; set; }

        [JsonPropertyName("monitor_vol")]
        public string? MonitorVolume { get; set; }

        [JsonPropertyName("band")]
        public string? Band { get; set; }

        [JsonPropertyName("swr")]
        public string? SWR { get; set; }

        [JsonPropertyName("master_vol")]
        public string? MasterVolume { get; set; }

        [JsonPropertyName("temperature")]
        public string? Temperature { get; set; }

        [JsonPropertyName("current")]
        public string? Current { get; set; }

        [JsonPropertyName("receiver_a")]
        public ReceiverStatusDetail ReceiverA { get; set; } = new ReceiverStatusDetail();

        [JsonPropertyName("receiver_b")]
        public ReceiverStatusDetail ReceiverB { get; set; } = new ReceiverStatusDetail();
    }

    public class ReceiverStatusDetail
    {
        [JsonPropertyName("active")]
        public string? Enabled { get; set; }

        [JsonPropertyName("frequency")]
        public string? Frequency { get; set; }

        [JsonPropertyName("mode")]
        public string? Mode { get; set; }

        [JsonPropertyName("filterlow")]
        public string? FilterLow { get; set; }

        [JsonPropertyName("filterhigh")]
        public string? FilterHigh { get; set; }

        [JsonPropertyName("volume")]
        public string? Volume { get; set; }

        [JsonPropertyName("squelch")]
        public string? Squelch { get; set; }

        [JsonPropertyName("mox")]
        public string? Mox { get; set; }

        [JsonPropertyName("txvfo")]
        public string? TxVfo { get; set; }

        [JsonPropertyName("signal")]
        public string? Signal { get; set; }
    }

    #endregion

    #region StreamDeck actions

    public class BaseKeypadMqttItem : KeypadBase
    {
        private PluginSettings _settings = new();
        private GlobalPluginSettings _globalSettings = new();
        private string lastReceivedSettings = "";
        private string lastReceivedGlobalSettings = "";
        private DateTime lastMqttUpdate = DateTime.Now;
        private bool timeout = false;

        public PluginSettings Settings
        {
            get => _settings;
        }

        public GlobalPluginSettings GlobalSettings
        {
            get => _globalSettings;
        }

        public bool Timeout
        {
            get => timeout;
        }

        #region StreamDock events
        public BaseKeypadMqttItem(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            Connection.SetImageAsync(StreamDock.UpdateKeyImage($"Loading...")).Wait();

            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType().Name}: No settings found, creating default settings");
                this._settings = PluginSettings.CreateDefaultSettings();
                Connection.SetSettingsAsync(JObject.FromObject(_settings));
            }
            else
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType().Name}: Previous settings found, updating settings.");
                var newSettings = payload.Settings.ToObject<PluginSettings>();
    
                if (newSettings != null)
                {
                    Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: Updating values. RxIndex={newSettings.RxIndex}, SubRx={newSettings.SubRx}, RxBand={newSettings.RxBand}, VolumeIncrement={newSettings.VolumeIncrement}, FrequencyIncrement={newSettings.FrequencyIncrement}");
                    this._settings = newSettings;
                }
                else
                {
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType().Name}: Settings are invalid, creating default settings");
                    this._settings = PluginSettings.CreateDefaultSettings();
                    Connection.SetSettingsAsync(JObject.FromObject(_settings));
                }
            }

            // Ensure global settings are initialized
            _globalSettings = GlobalPluginSettings.CreateDefaultSettings();
            
            // First request current global settings (will call ReceivedGlobalSettings if they exist)
            Connection.GetGlobalSettingsAsync();
            
            // Only send default settings if this is the first time the plugin is run
            // By adding a slight delay, we ensure we don't overwrite existing settings
            System.Threading.Tasks.Task.Run(async () => {
                await System.Threading.Tasks.Task.Delay(500);
                
                // If this is the first run or settings need to be initialized
                if (_globalSettings.MqttAuthenticationList.Count <= 2) {
                    Connection.SetGlobalSettingsAsync(JObject.FromObject(_globalSettings)).Wait();
                }
            });

            if (!MQTT_Client.ClientConnected)
            {
                MQTT_Client.ConnectWithDefaults();
            }

            MQTT_Client.OnMessageReceived += MQTT_Client_OnMessageReceived;
            MQTT_Client.OnConnectionStatusChanged += MQTT_Client_OnConnectionStatusChanged;
        }

        public override void KeyPressed(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: KeyPressed called");
        }

        public override void KeyReleased(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: KeyReleased called");
        }

        public override void OnTick()
        {
            // Watchdog to ensure we have recent MQTT data
            if ((DateTime.Now - lastMqttUpdate).TotalSeconds > 30)
            {
                timeout = true;

                if (MQTT_Client.ClientConnected)
                    Connection.SetImageAsync(StreamDock.UpdateKeyImage($"Broker: OK\nOl-Master: KO")).Wait();
                else
                    Connection.SetImageAsync(StreamDock.UpdateKeyImage($"Broker: KO\nOl-Master: KO")).Wait();
            }
        }

        public override void Dispose()
        {
            MQTT_Client.OnMessageReceived -= MQTT_Client_OnMessageReceived;
            MQTT_Client.OnConnectionStatusChanged -= MQTT_Client_OnConnectionStatusChanged;
            
            if (MQTT_Client.Client != null && MQTT_Client.ClientConnected)
            {
                MQTT_Client.DisconnectFromBroker().Wait();
            }
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: ReceivedGlobalSettings called: {payload.Settings}");

            try
            {
                // Check if settings have changed
                if (payload.Settings.ToString() == lastReceivedGlobalSettings)
                {
                    Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: ReceivedGlobalSettings: No changes detected, ignoring.");
                    return;
                }

                // Update last received settings
                lastReceivedGlobalSettings = payload.Settings.ToString() ?? "";

                var newSettings = payload.Settings.ToObject<GlobalPluginSettings>();
                if (newSettings != null)
                {
                    // Store the settings but DO NOT send them back immediately
                    _globalSettings = newSettings;
                    Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: Global settings updated: {System.Text.Json.JsonSerializer.Serialize(_globalSettings)}");
                    GlobalSettingsUpdated();
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"{GetType().Name}: Error ReceivedGlobalSettings: {ex.Message}");
            }
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: ReceivedSettings called: {payload.Settings.ToString().Replace("\n", " ")}");
            try
            {
                // Check if settings have changed
                if (payload.Settings.ToString() == lastReceivedSettings)
                {
                    Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: ReceivedSettings: No changes detected, ignoring.");
                    return;
                }

                // Update last received settings
                lastReceivedSettings = payload.Settings.ToString() ?? "";

                var newSettings = payload.Settings.ToObject<PluginSettings>();
                if (newSettings != null)
                {
                    _settings = newSettings;
                    Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: Settings updated: {System.Text.Json.JsonSerializer.Serialize(_settings)}");
                    SettingsUpdated();
                    Connection.SetSettingsAsync(JObject.FromObject(_settings)).Wait();
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"{GetType().Name}: Error ReceivedSettings: {ex.Message}");
            }
        }
        #endregion

        #region Custom events
        private void MQTT_Client_OnMessageReceived(string topic, string payload)
        {
            //Logger.Instance.LogMessage(TracingLevel.INFO, "MQTT Message received");
            var command = System.Text.Json.JsonSerializer.Deserialize<ReceiverStatus>(payload);
            int.TryParse(topic.Substring(topic.Length - 1, 1), out var receiverNumber);
            if (command != null && receiverNumber > 0 && receiverNumber <= 4)
            {
                MQTT_StatusReceived(receiverNumber, command);
                lastMqttUpdate = DateTime.Now;
                timeout = false;
            }
        }
        
        private void MQTT_Client_OnConnectionStatusChanged(bool isConnected, string message)
        {
            if (isConnected)
            {
                Connection.SetImageAsync(StreamDock.UpdateKeyImage($"Ready")).Wait();
                Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType().Name}: MQTT Connected");
            }
            else
            {
                Connection.SetImageAsync(StreamDock.UpdateKeyImage($"{message}")).Wait();
                Logger.Instance.LogMessage(TracingLevel.WARN, $"{GetType().Name}: MQTT {message}");
            }
        }

        public virtual void MQTT_StatusReceived(int receiverNumber, ReceiverStatus command)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: MQTT_StatusReceived called");
        }

        public virtual void SettingsUpdated()
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: SettingsUpdated called with: {System.Text.Json.JsonSerializer.Serialize(Settings).Replace("\n", " ")}");
        }

        public virtual void GlobalSettingsUpdated()
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: GlobalSettingsUpdated called with: {System.Text.Json.JsonSerializer.Serialize(Settings).Replace("\n", " ")}");

            MQTT_Config.Host = this.GlobalSettings.MqttHost;
            MQTT_Config.Port = this.GlobalSettings.MqttPort;
            MQTT_Config.User = this.GlobalSettings.MqttUsername;
            MQTT_Config.Password = this.GlobalSettings.MqttPassword;
            MQTT_Config.UseAuthentication = this.GlobalSettings.MqttAuthentication;
            MQTT_Config.UseWebSocket = this.GlobalSettings.MqttWebsocket;

            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: MQTT_Config updated. MqttHost={MQTT_Config.Host}, MqttPort={MQTT_Config.Port}, MqttUser={MQTT_Config.User}, UseAuthentication={MQTT_Config.UseAuthentication}, UseWebSocket={MQTT_Config.UseWebSocket}");

            MQTT_Client.ConnectWithDefaults();
        }
        #endregion
    }

    public class BaseDialMqttItem : EncoderBase
    {
        private PluginSettings _settings;
        private GlobalPluginSettings _globalSettings = new();
        private string lastReceivedSettings = "";
        private string lastReceivedGlobalSettings = "";
        private DateTime lastTickEvent = DateTime.MinValue;

        public PluginSettings Settings
        {
            get => _settings;
        }

        public GlobalPluginSettings GlobalSettings
        {
            get => _globalSettings;
        }

        public BaseDialMqttItem(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            Connection.SetImageAsync(StreamDock.UpdateKeyImage($"Loading...")).Wait();

            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType().Name}: No settings found, creating default settings");
                this._settings = PluginSettings.CreateDefaultSettings();
                Connection.SetSettingsAsync(JObject.FromObject(_settings));
            }
            else
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType().Name}: Previous settings found, updating settings.");
                var newSettings = payload.Settings.ToObject<PluginSettings>();

                if (newSettings != null)
                {
                    Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: Updating values. RxIndex={newSettings.RxIndex}, SubRx={newSettings.SubRx}, RxBand={newSettings.RxBand}, VolumeIncrement={newSettings.VolumeIncrement}, FrequencyIncrement={newSettings.FrequencyIncrement}");
                    this._settings = newSettings;
                }
                else
                {
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType().Name}: Settings are invalid, creating default settings");
                    this._settings = PluginSettings.CreateDefaultSettings();
                    Connection.SetSettingsAsync(JObject.FromObject(_settings));
                }
            }

            // Ensure global settings are initialized
            _globalSettings = GlobalPluginSettings.CreateDefaultSettings();
            
            // First request current global settings (will call ReceivedGlobalSettings if they exist)
            Connection.GetGlobalSettingsAsync();
            
            // Only send default settings if this is the first time the plugin is run
            // By adding a slight delay, we ensure we don't overwrite existing settings
            System.Threading.Tasks.Task.Run(async () => {
                await System.Threading.Tasks.Task.Delay(500);
                
                // If this is the first run or settings need to be initialized
                if (_globalSettings.MqttAuthenticationList.Count <= 2) {
                    Connection.SetGlobalSettingsAsync(JObject.FromObject(_globalSettings)).Wait();
                }
            });

            if (!MQTT_Client.ClientConnected)
            {
                MQTT_Client.ConnectWithDefaults();
            }

            MQTT_Client.OnMessageReceived += MQTT_Client_OnMessageReceived;
            MQTT_Client.OnConnectionStatusChanged += MQTT_Client_OnConnectionStatusChanged;
        }
        
        private void MQTT_Client_OnConnectionStatusChanged(bool isConnected, string message)
        {
            if (isConnected)
            {
                Connection.SetImageAsync(StreamDock.UpdateKeyImage($"Ready")).Wait();
                Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType().Name}: MQTT Connected");
            }
            else
            {
                Connection.SetImageAsync(StreamDock.UpdateKeyImage($"{message}")).Wait();
                Logger.Instance.LogMessage(TracingLevel.WARN, $"{GetType().Name}: MQTT {message}");
            }
        }

        public override void DialDown(DialPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: DialDown called");
        }

        public override void DialRotate(DialRotatePayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: DialRotate called");
        }

        public override void DialUp(DialPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: DialUp called");
        }

        public override void Dispose()
        {
            MQTT_Client.OnConnectionStatusChanged -= MQTT_Client_OnConnectionStatusChanged;
            
            if (MQTT_Client.Client != null && MQTT_Client.ClientConnected)
            {
                MQTT_Client.DisconnectFromBroker().Wait();
            }
        }

        public override void OnTick()
        {
            if ((DateTime.Now - lastTickEvent).TotalSeconds < 5)
            {
                // Limit updates to every 5 seconds
                return;
            }

            if (MQTT_Client.ClientConnected)
            {
                Connection.SetImageAsync(StreamDock.UpdateKeyImage($"Ready")).Wait();
            }
            else
            {
                Connection.SetImageAsync(StreamDock.UpdateKeyImage($"Disconnected")).Wait();
            }
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: ReceivedGlobalSettings called: {payload.Settings}");

            try
            {
                // Check if settings have changed
                if (payload.Settings.ToString() == lastReceivedGlobalSettings)
                {
                    Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: ReceivedGlobalSettings: No changes detected, ignoring.");
                    return;
                }

                // Update last received settings
                lastReceivedGlobalSettings = payload.Settings.ToString() ?? "";

                var newSettings = payload.Settings.ToObject<GlobalPluginSettings>();
                if (newSettings != null)
                {
                    // Store the settings but DO NOT send them back immediately
                    _globalSettings = newSettings;
                    Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: Global settings updated: {System.Text.Json.JsonSerializer.Serialize(_globalSettings)}");
                    GlobalSettingsUpdated();
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"{GetType().Name}: Error ReceivedGlobalSettings: {ex.Message}");
            }
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: ReceivedSettings called");
            try
            {
                // Check if settings have changed
                if (payload.Settings.ToString() == lastReceivedSettings)
                {
                    Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: ReceivedSettings: No changes detected, ignoring.");
                    return;
                }

                // Update last received settings
                lastReceivedSettings = payload.Settings.ToString() ?? "";

                var newSettings = payload.Settings.ToObject<PluginSettings>();
                if (newSettings != null)
                {
                    _settings = newSettings;
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType().Name}: Settings updated");
                    SettingsUpdated();
                    Connection.SetSettingsAsync(JObject.FromObject(_settings)).Wait();
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"{GetType().Name}: Error ReceivedSettings: {ex.Message}");
            }
        }

        public override void TouchPress(TouchpadPressPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: TouchPress called: {payload}");
        }

        public virtual void SettingsUpdated()
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: SettingsUpdated called with: {System.Text.Json.JsonSerializer.Serialize(Settings).Replace("\n", " ")}");

            // TODO: Implement settings update logic
        }

        public virtual void GlobalSettingsUpdated()
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: GlobalSettingsUpdated called with: {System.Text.Json.JsonSerializer.Serialize(GlobalSettings).Replace("\n", " ")}");

            MQTT_Config.Host = this.GlobalSettings.MqttHost;
            MQTT_Config.Port = this.GlobalSettings.MqttPort;
            MQTT_Config.User = this.GlobalSettings.MqttUsername;
            MQTT_Config.Password = this.GlobalSettings.MqttPassword;
            MQTT_Config.UseAuthentication = this.GlobalSettings.MqttAuthentication;
            MQTT_Config.UseWebSocket = this.GlobalSettings.MqttWebsocket;

            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: MQTT_Config updated. MqttHost={MQTT_Config.Host}, MqttPort={MQTT_Config.Port}, MqttUser={MQTT_Config.User}, UseAuthentication={MQTT_Config.UseAuthentication}, UseWebSocket={MQTT_Config.UseWebSocket}");

            MQTT_Client.ConnectWithDefaults();
        }

        private void MQTT_Client_OnMessageReceived(string topic, string payload)
        {
            //Logger.Instance.LogMessage(TracingLevel.INFO, "MQTT Message received");
            var command = System.Text.Json.JsonSerializer.Deserialize<ReceiverStatus>(payload);
            int.TryParse(topic.Substring(topic.Length - 1, 1), out var receiverNumber);
            if (command != null && receiverNumber > 0 && receiverNumber <= 4)
            {
                MQTT_StatusReceived(receiverNumber, command);
            }
        }

        public virtual void MQTT_StatusReceived(int receiverNumber, ReceiverStatus command)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: MQTT_StatusReceived called");
        }
    }

    #endregion

    #region Settings

    public class GlobalPluginSettings
    {
        public static GlobalPluginSettings CreateDefaultSettings()
        {
            GlobalPluginSettings instance = new();

            // Use values from MQTT_Config instead of hardcoded defaults
            // This ensures that we keep the current configuration when adding new controls
            instance.MqttHost = MQTT_Config.Host;
            instance.MqttPort = MQTT_Config.Port;
            instance.MqttUsername = MQTT_Config.User;
            instance.MqttPassword = MQTT_Config.Password;
            instance.MqttAuthentication = MQTT_Config.UseAuthentication;
            instance.MqttWebsocket = MQTT_Config.UseWebSocket;

            return instance;
        }

        #region Json global properties
        [JsonProperty(PropertyName = "MqttHost")]
        public string MqttHost { get; set; } = MQTT_Config.Host;
    
        [JsonProperty(PropertyName = "MqttPort")]
        public int MqttPort { get; set; } = MQTT_Config.Port;
    
        [JsonProperty(PropertyName = "MqttUsername")]
        public string MqttUsername { get; set; } = MQTT_Config.User;
    
        [JsonProperty(PropertyName = "MqttPassword")]
        public string MqttPassword { get; set; } = MQTT_Config.Password;
    
        [JsonProperty(PropertyName = "MqttAuthenticationList")]
        public List<AuthenticationList> MqttAuthenticationList { get; set; } = new List<AuthenticationList>
        {
            new AuthenticationList { AuthenticationName = "No", AuthenticationValue = false },
            new AuthenticationList { AuthenticationName = "Yes", AuthenticationValue = true },
        };
    
        [JsonProperty(PropertyName = "MqttWebsocketList")]
        public List<WebSocketList> MqttWebsocketList { get; set; } = new List<WebSocketList>
        {
            new WebSocketList { WebSocketName = "MQTT", WebSocketValue = false },
            new WebSocketList { WebSocketName = "WebSocket", WebSocketValue = true },
        };
    
        [JsonProperty(PropertyName = "MqttAuthentication")]
        public bool MqttAuthentication { get; set; } = MQTT_Config.UseAuthentication;
    
        [JsonProperty(PropertyName = "MqttWebsocket")]
        public bool MqttWebsocket { get; set; } = MQTT_Config.UseWebSocket;
        #endregion
    }

    public class PluginSettings
    {
        public static PluginSettings CreateDefaultSettings()
        {
            PluginSettings instance = new();

            instance.RxIndex = 1;
            instance.SubRx = 0;
            instance.RxBand = "B20M";
            instance.VolumeIncrement = 10;
            instance.FrequencyIncrement = 0;
            instance.SdrMode = "Last";

            return instance;
        }

        #region Json actions properties
        [JsonProperty(PropertyName = "RxIndex")]
        public int RxIndex { get; set; } = 1;

        [JsonProperty(PropertyName = "RxIndexList")]
        public List<SdrIndexes> RxIndexList { get; set; } = new List<SdrIndexes>
            {
                new SdrIndexes { RcvName = "1", RcvValue = 1 },
                new SdrIndexes { RcvName = "2", RcvValue = 2 },
                new SdrIndexes { RcvName = "3", RcvValue = 3 },
                new SdrIndexes { RcvName = "4", RcvValue = 4 },
            };

        [JsonProperty(PropertyName = "SubRx")]
        public int SubRx { get; set; } = 0;

        [JsonProperty(PropertyName = "SubRxList")]
        public List<SdrSubRx> SubRxList { get; set; } = new List<SdrSubRx>
            {
                new SdrSubRx { SubRxName = "Main", SubRxValue = 0 },
                new SdrSubRx { SubRxName = "Sub", SubRxValue = 1 },
            };

        [JsonProperty(PropertyName = "RxBand")]
        public string? RxBand { get; set; } = "B20M";

        [JsonProperty(PropertyName = "RxBandList")]
        public List<SdrBands> RxBandList { get; set; } = new List<SdrBands>
            {
                new SdrBands { BandName = "160m", BandValue = "B160M" },
                new SdrBands { BandName = "80m", BandValue = "B80M" },
                new SdrBands { BandName = "60m", BandValue = "B60M" },
                new SdrBands { BandName = "40m", BandValue = "B40M" },
                new SdrBands { BandName = "30m", BandValue = "B30M" },
                new SdrBands { BandName = "20m", BandValue = "B20M" },
                new SdrBands { BandName = "17m", BandValue = "B17M" },
                new SdrBands { BandName = "15m", BandValue = "B15M" },
                new SdrBands { BandName = "12m", BandValue = "B12M" },
                new SdrBands { BandName = "10m", BandValue = "B10M" },
                new SdrBands { BandName = "6m", BandValue = "B6M" },
                new SdrBands { BandName = "2m", BandValue = "B2M" },
                new SdrBands { BandName = "GEN", BandValue = "GEN" },
            };

        [JsonProperty(PropertyName = "VolumeIncrement")]
        public int VolumeIncrement { get; set; } = 10; // %

        [JsonProperty(PropertyName = "VolumeIncrementList")]
        public List<VolumeIncrementList> VolumeIncrementList { get; set; } = new List<VolumeIncrementList>
            {
                new VolumeIncrementList { VolumeIncrementName = "5%", VolumeIncrementValue = 5 },
                new VolumeIncrementList { VolumeIncrementName = "10%", VolumeIncrementValue = 10 },
                new VolumeIncrementList { VolumeIncrementName = "15%", VolumeIncrementValue = 15 },
                new VolumeIncrementList { VolumeIncrementName = "20%", VolumeIncrementValue = 20 },
                new VolumeIncrementList { VolumeIncrementName = "25%", VolumeIncrementValue = 25 },
                new VolumeIncrementList { VolumeIncrementName = "30%", VolumeIncrementValue = 30 },
                new VolumeIncrementList { VolumeIncrementName = "35%", VolumeIncrementValue = 35 },
                new VolumeIncrementList { VolumeIncrementName = "40%", VolumeIncrementValue = 40 },
                new VolumeIncrementList { VolumeIncrementName = "50%", VolumeIncrementValue = 50 },
            };

        [JsonProperty(PropertyName = "FrequencyIncrement")]
        public int FrequencyIncrement { get; set; } = 0; // Frequency in Hz

        [JsonProperty(PropertyName = "FrequencyIncrementList")]
        public List<FrequencyIncrementList> FrequencyIncrementList { get; set; } = new List<FrequencyIncrementList>
            {
                new FrequencyIncrementList { FrequencyIncrementName = "Default", FrequencyIncrementValue = 0 },
                new FrequencyIncrementList { FrequencyIncrementName = "1Hz", FrequencyIncrementValue = 1 },
                new FrequencyIncrementList { FrequencyIncrementName = "5Hz", FrequencyIncrementValue = 5 },
                new FrequencyIncrementList { FrequencyIncrementName = "10Hz", FrequencyIncrementValue = 10 },
                new FrequencyIncrementList { FrequencyIncrementName = "50Hz", FrequencyIncrementValue = 50 },
                new FrequencyIncrementList { FrequencyIncrementName = "100Hz", FrequencyIncrementValue = 100 },
                new FrequencyIncrementList { FrequencyIncrementName = "500Hz", FrequencyIncrementValue = 500 },
                new FrequencyIncrementList { FrequencyIncrementName = "1kHz", FrequencyIncrementValue = 1000 },
                new FrequencyIncrementList { FrequencyIncrementName = "5kHz", FrequencyIncrementValue = 5000 },
                new FrequencyIncrementList { FrequencyIncrementName = "10kHz", FrequencyIncrementValue = 10000 },
                new FrequencyIncrementList { FrequencyIncrementName = "50kHz", FrequencyIncrementValue = 50000 },
                new FrequencyIncrementList { FrequencyIncrementName = "100kHz", FrequencyIncrementValue = 100000 },
                new FrequencyIncrementList { FrequencyIncrementName = "500kHz", FrequencyIncrementValue = 500000 },
                new FrequencyIncrementList { FrequencyIncrementName = "1MHz", FrequencyIncrementValue = 1000000 },
                new FrequencyIncrementList { FrequencyIncrementName = "5MHz", FrequencyIncrementValue = 5000000 },
                new FrequencyIncrementList { FrequencyIncrementName = "10MHz", FrequencyIncrementValue = 10000000 },
            };

        [JsonProperty(PropertyName = "SdrMode")]
        public string SdrMode { get; set; } = "Last";

        [JsonProperty(PropertyName = "SdrModeList")]
        public List<SdrModeList> SdrModeList { get; set; } = new List<SdrModeList>
            {
                new SdrModeList { ModeName = "LSB", ModeValue = "LSB" },
                new SdrModeList { ModeName = "USB", ModeValue = "USB" },
                new SdrModeList { ModeName = "DSB", ModeValue = "DSB" },
                new SdrModeList { ModeName = "CWL", ModeValue = "CWL" },
                new SdrModeList { ModeName = "CWU", ModeValue = "CWU" },
                new SdrModeList { ModeName = "AM", ModeValue = "AM" },
                new SdrModeList { ModeName = "SAM", ModeValue = "SAM" },
                new SdrModeList { ModeName = "SAML", ModeValue = "SAML" },
                new SdrModeList { ModeName = "SAMU", ModeValue = "SAMU" },
                new SdrModeList { ModeName = "DIGL", ModeValue = "DIGL" },
                new SdrModeList { ModeName = "DIGU", ModeValue = "DIGU" },
                new SdrModeList { ModeName = "FM5", ModeValue = "FM5" },
                new SdrModeList { ModeName = "FM2", ModeValue = "FM2" },
                new SdrModeList { ModeName = "FT", ModeValue = "FT" },
                new SdrModeList { ModeName = "Last used", ModeValue = "Last" },
            };

        [JsonProperty(PropertyName = "KeyerMsgIndex")]
        public int KeyerMsgIndex { get; set; } = 1; // Index of the message from CW Keyer

        [JsonProperty(PropertyName = "KeyerMsgIndexList")]
        public List<KeyerMsg> KeyerMsgIndexList { get; set; } = new List<KeyerMsg>
            {
                new KeyerMsg { KeyerMsgName = "F1", KeyerMsgValue = 1 },
                new KeyerMsg { KeyerMsgName = "F2", KeyerMsgValue = 2 },
                new KeyerMsg { KeyerMsgName = "F3", KeyerMsgValue = 3 },
                new KeyerMsg { KeyerMsgName = "F4", KeyerMsgValue = 4 },
                new KeyerMsg { KeyerMsgName = "F5", KeyerMsgValue = 5 },
                new KeyerMsg { KeyerMsgName = "F6", KeyerMsgValue = 6 },
                new KeyerMsg { KeyerMsgName = "F7", KeyerMsgValue = 7 },
                new KeyerMsg { KeyerMsgName = "F8", KeyerMsgValue = 8 },
                new KeyerMsg { KeyerMsgName = "F9", KeyerMsgValue = 9 },
                new KeyerMsg { KeyerMsgName = "F10", KeyerMsgValue = 10 },
                new KeyerMsg { KeyerMsgName = "F11", KeyerMsgValue = 11 },
                new KeyerMsg { KeyerMsgName = "F12", KeyerMsgValue = 12 },
                new KeyerMsg { KeyerMsgName = "Shift + F1", KeyerMsgValue = 13 },
                new KeyerMsg { KeyerMsgName = "Shift + F2", KeyerMsgValue = 14 },
                new KeyerMsg { KeyerMsgName = "Shift + F3", KeyerMsgValue = 15 },
                new KeyerMsg { KeyerMsgName = "Shift + F4", KeyerMsgValue = 16 },
                new KeyerMsg { KeyerMsgName = "Shift + F5", KeyerMsgValue = 17 },
                new KeyerMsg { KeyerMsgName = "Shift + F6", KeyerMsgValue = 18 },
                new KeyerMsg { KeyerMsgName = "Shift + F7", KeyerMsgValue = 19 },
                new KeyerMsg { KeyerMsgName = "Shift + F8", KeyerMsgValue = 20 },
                new KeyerMsg { KeyerMsgName = "Shift + F9", KeyerMsgValue = 21 },
                new KeyerMsg { KeyerMsgName = "Shift + F10", KeyerMsgValue = 22 },
                new KeyerMsg { KeyerMsgName = "Shift + F11", KeyerMsgValue = 23 },
                new KeyerMsg { KeyerMsgName = "Shift + F12", KeyerMsgValue = 24 },
            };

        [JsonProperty(PropertyName = "KeyerText")]
        public string KeyerText { get; set; } = "CQ CQ CQ de IU2FRL";
        #endregion
    }

    #endregion

    public class SdrBands
    {
        [JsonProperty(PropertyName = "bandName")]
        public string? BandName { get; set; }

        [JsonProperty(PropertyName = "bandValue")]
        public string? BandValue { get; set; }
    }

    public class SdrIndexes
    {
        [JsonProperty(PropertyName = "rcvIndexName")]
        public string? RcvName { get; set; }

        [JsonProperty(PropertyName = "rcvIndexValue")]
        public int RcvValue { get; set; }
    }

    public class SdrSubRx
    {
        [JsonProperty(PropertyName = "subRxName")]
        public string? SubRxName { get; set; }

        [JsonProperty(PropertyName = "subRxValue")]
        public int SubRxValue { get; set; }
    }

    public class KeyerMsg
    {
        [JsonProperty(PropertyName = "keyerMsgName")]
        public string? KeyerMsgName { get; set; }

        [JsonProperty(PropertyName = "keyerMsgValue")]
        public int KeyerMsgValue { get; set; }
    }

    #region Settings lists
    public class VolumeIncrementList
    {
        [JsonProperty(PropertyName = "volumeIncrementName")]
        public string? VolumeIncrementName { get; set; }

        [JsonProperty(PropertyName = "volumeIncrementValue")]
        public int VolumeIncrementValue { get; set; }
    }

    public class FrequencyIncrementList
    {
        [JsonProperty(PropertyName = "frequencyIncrementName")]
        public string? FrequencyIncrementName { get; set; }
        [JsonProperty(PropertyName = "frequencyIncrementValue")]
        public int FrequencyIncrementValue { get; set; }
    }

    public class SdrModeList
    {
        [JsonProperty(PropertyName = "modeName")]
        public string? ModeName { get; set; }
        [JsonProperty(PropertyName = "modeValue")]
        public string? ModeValue { get; set; }
    }

    public class AuthenticationList
    {
        [JsonProperty(PropertyName = "mqttAuthenticationName")]
        public string? AuthenticationName { get; set; }
        [JsonProperty(PropertyName = "mqttAuthenticationValue")]
        public bool AuthenticationValue { get; set; }
    }

    public class WebSocketList
    {
        [JsonProperty(PropertyName = "mqttWebsocketName")]
        public string? WebSocketName { get; set; }
        [JsonProperty(PropertyName = "mqttWebsocketValue")]
        public bool WebSocketValue { get; set; }
    }

    #endregion

    #endregion
}