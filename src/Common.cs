#region Using directives
using System.Drawing;
using System.Text.Json.Serialization;
using BarRaider.SdTools;
using BarRaider.SdTools.Payloads;
using Coordinates;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
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

    public class BaseKeypadMqttItem : KeypadBase
    {
        private PluginSettings _settings = new();

        public PluginSettings Settings
        {
            get => _settings;
        }

        #region StreamDock events
        public BaseKeypadMqttItem(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            Connection.SetImageAsync(StreamDock.UpdateKeyImage($"Loading...")).Wait();

            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                this._settings = PluginSettings.CreateDefaultSettings();
                _ = Task.Run(() => Connection.SetSettingsAsync(JObject.FromObject(_settings)).Wait());
            }
            else
            {
                var newSettings = payload.Settings.ToObject<PluginSettings>();
                if (newSettings != null)
                {
                    this._settings = newSettings;
                }
                else
                {
                    this._settings = PluginSettings.CreateDefaultSettings();
                    _ = Task.Run(() => Connection.SetSettingsAsync(JObject.FromObject(_settings)));
                }
            }

            if (!MQTT_Client.ClientConnected)
            {
                MQTT_Client.ConnectToBroker(MQTT_Config.Host, MQTT_Config.Port, MQTT_Config.User, MQTT_Config.Password, MQTT_Config.UseAuthentication, MQTT_Config.UseWebSocket);
            }
            MQTT_Client.OnMessageReceived += MQTT_Client_OnMessageReceived;
        }

        public override void KeyPressed(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType().Name}: KeyPressed called");
        }

        public override void KeyReleased(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType().Name}: KeyReleased called");
        }

        public override void OnTick()
        {
            if (!MQTT_Client.Client.IsConnected)
            {
                Connection.SetImageAsync(StreamDock.UpdateKeyImage($"Connection\nError")).Wait();
            }
        }

        public override void Dispose()
        {
            MQTT_Client.DisconnectFromBroker().Wait();
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType().Name}: ReceivedGlobalSettings called: {payload}");
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: ReceivedSettings called: {payload.Settings}");
            try
            {
                var updates = Tools.AutoPopulateSettings(_settings, payload.Settings);
                Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: {updates} settings were updated. New values: {System.Text.Json.JsonSerializer.Serialize(_settings)}");
                SettingsUpdated();
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
            }
        }

        public virtual void MQTT_StatusReceived(int receiverNumber, ReceiverStatus command)
        {

        }

        public virtual void SettingsUpdated()
        {
        }
        #endregion
    }

    public class BaseDialMqttItem : EncoderBase
    {
        private PluginSettings _settings;

        public PluginSettings Settings
        {
            get => _settings;
        }

        public BaseDialMqttItem(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            Connection.SetImageAsync(StreamDock.UpdateKeyImage($"Loading...")).Wait();

            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                this._settings = PluginSettings.CreateDefaultSettings();
                _ = Task.Run(() => Connection.SetSettingsAsync(JObject.FromObject(_settings)));
            }
            else
            {
                var newSettings = payload.Settings.ToObject<PluginSettings>();
                if (newSettings != null)
                {
                    this._settings = newSettings;
                }
                else
                {
                    this._settings = PluginSettings.CreateDefaultSettings();
                    _ = Task.Run(() => Connection.SetSettingsAsync(JObject.FromObject(_settings)));
                }
            }

            if (!MQTT_Client.ClientConnected)
            {
                MQTT_Client.ConnectToBroker(MQTT_Config.Host, MQTT_Config.Port, MQTT_Config.User, MQTT_Config.Password, MQTT_Config.UseAuthentication, MQTT_Config.UseWebSocket);
            }
        }

        public override void DialDown(DialPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType().Name}: DialDown called");
        }

        public override void DialRotate(DialRotatePayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType().Name}: DialRotate called");
        }

        public override void DialUp(DialPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType().Name}: DialUp called");
        }

        public override void Dispose()
        {
            MQTT_Client.DisconnectFromBroker().Wait();
        }

        public override void OnTick()
        {
            if (!MQTT_Client.Client.IsConnected)
            {
                Connection.SetImageAsync(StreamDock.UpdateKeyImage($"Connection\nError")).Wait();
            }
            else
            {
                Connection.SetImageAsync(StreamDock.UpdateKeyImage("Connected")).Wait();
            }
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType().Name}: ReceivedGlobalSettings called");
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: ReceivedSettings called: {payload.Settings}");
            try
            {
                var updates = Tools.AutoPopulateSettings(_settings, payload.Settings);
                Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: {updates} settings were updated. New values: {System.Text.Json.JsonSerializer.Serialize(_settings)}");
                SettingsUpdated();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"{GetType().Name}: Error ReceivedSettings: {ex.Message}");
            }
        }

        public override void TouchPress(TouchpadPressPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType().Name}: TouchPress called: {payload}");
        }

        public virtual void SettingsUpdated()
        {
        }
    }

    public class PluginSettings
    {
        public static PluginSettings CreateDefaultSettings()
        {
            PluginSettings instance = new();
            instance.RxIndex = 1;
            instance.SubRx = false;
            instance.RxBand = "B20M";
            instance.VolumeIncrement = 10;
            instance.FrequencyIncrement = 0;

            return instance;
        }

        [JsonProperty(PropertyName = "RxIndex")]
        public int RxIndex { get; set; }

        [JsonProperty(PropertyName = "SubRx")]
        public bool SubRx { get; set; }

        [JsonProperty(PropertyName = "RxBand")]
        public string RxBand { get; set; }

        [JsonProperty(PropertyName = "VolumeIncrement")]
        public int VolumeIncrement { get; set; } // %

        [JsonProperty(PropertyName = "FrequencyIncrement")]
        public double FrequencyIncrement { get; set; } // Frequency in Hz
    }

    #endregion
}