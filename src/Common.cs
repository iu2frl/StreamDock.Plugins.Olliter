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
            if (MQTT_Client.Client != null && MQTT_Client.ClientConnected)
            {
                MQTT_Client.DisconnectFromBroker().Wait();
            }
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
        public PluginSettings()
        {
            RxBands = new List<SdrBands>
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
            RxIndexes = new List<SdrIndexes>
            {
                new SdrIndexes { RcvName = "1", RcvValue = 1 },
                new SdrIndexes { RcvName = "2", RcvValue = 2 },
                new SdrIndexes { RcvName = "3", RcvValue = 3 },
                new SdrIndexes { RcvName = "4", RcvValue = 4 },
            };
            SubRxs = new List<SdrSubRx>
            {
                new SdrSubRx { SubRxName = "Main", SubRxValue = 0 },
                new SdrSubRx { SubRxName = "Sub", SubRxValue = 1 },
            };
            VolumeIncrements = new List<VolumeIncrements>
            {
                new VolumeIncrements { VolumeIncrementName = "5%", VolumeIncrementValue = 5 },
                new VolumeIncrements { VolumeIncrementName = "10%", VolumeIncrementValue = 10 },
                new VolumeIncrements { VolumeIncrementName = "15%", VolumeIncrementValue = 15 },
                new VolumeIncrements { VolumeIncrementName = "20%", VolumeIncrementValue = 20 },
                new VolumeIncrements { VolumeIncrementName = "25%", VolumeIncrementValue = 25 },
                new VolumeIncrements { VolumeIncrementName = "30%", VolumeIncrementValue = 30 },
                new VolumeIncrements { VolumeIncrementName = "35%", VolumeIncrementValue = 35 },
                new VolumeIncrements { VolumeIncrementName = "40%", VolumeIncrementValue = 40 },
                new VolumeIncrements { VolumeIncrementName = "50%", VolumeIncrementValue = 50 },
            };
            FrequencyIncrements = new List<FrequencyIncrements>
            {
                new FrequencyIncrements { FrequencyIncrementName = "Default", FrequencyIncrementValue = 0 },
                new FrequencyIncrements { FrequencyIncrementName = "1Hz", FrequencyIncrementValue = 1 },
                new FrequencyIncrements { FrequencyIncrementName = "5Hz", FrequencyIncrementValue = 5 },
                new FrequencyIncrements { FrequencyIncrementName = "10Hz", FrequencyIncrementValue = 10 },
                new FrequencyIncrements { FrequencyIncrementName = "50Hz", FrequencyIncrementValue = 50 },
                new FrequencyIncrements { FrequencyIncrementName = "100Hz", FrequencyIncrementValue = 100 },
                new FrequencyIncrements { FrequencyIncrementName = "500Hz", FrequencyIncrementValue = 500 },
                new FrequencyIncrements { FrequencyIncrementName = "1kHz", FrequencyIncrementValue = 1000 },
                new FrequencyIncrements { FrequencyIncrementName = "5kHz", FrequencyIncrementValue = 5000 },
                new FrequencyIncrements { FrequencyIncrementName = "10kHz", FrequencyIncrementValue = 10000 },
                new FrequencyIncrements { FrequencyIncrementName = "50kHz", FrequencyIncrementValue = 50000 },
                new FrequencyIncrements { FrequencyIncrementName = "100kHz", FrequencyIncrementValue = 100000 },
                new FrequencyIncrements { FrequencyIncrementName = "500kHz", FrequencyIncrementValue = 500000 },
                new FrequencyIncrements { FrequencyIncrementName = "1MHz", FrequencyIncrementValue = 1000000 },
                new FrequencyIncrements { FrequencyIncrementName = "5MHz", FrequencyIncrementValue = 5000000 },
                new FrequencyIncrements { FrequencyIncrementName = "10MHz", FrequencyIncrementValue = 10000000 },
            };
        }

        public static PluginSettings CreateDefaultSettings()
        {
            PluginSettings instance = new();
            instance.RxIndex = 1;
            instance.SubRx = 0;
            instance.RxBand = "B20M";
            instance.VolumeIncrement = 10;
            instance.FrequencyIncrement = 0;

            return instance;
        }

        #region Json properties
        [JsonProperty(PropertyName = "RxIndex")]
        public int RxIndex { get; set; }

        [JsonProperty(PropertyName = "RxIndexes")]
        public List<SdrIndexes>? RxIndexes { get; set; }

        [JsonProperty(PropertyName = "SubRx")]
        public int SubRx { get; set; }

        [JsonProperty(PropertyName = "SubRxs")]
        public List<SdrSubRx>? SubRxs { get; set; }

        [JsonProperty(PropertyName = "RxBand")]
        public string? RxBand { get; set; }

        [JsonProperty(PropertyName = "RxBands")]
        public List<SdrBands>? RxBands { get; set; }

        [JsonProperty(PropertyName = "VolumeIncrement")]
        public int VolumeIncrement { get; set; } // %

        [JsonProperty(PropertyName = "VolumeIncrements")]
        public List<VolumeIncrements>? VolumeIncrements { get; set; }

        [JsonProperty(PropertyName = "FrequencyIncrement")]
        public double FrequencyIncrement { get; set; } // Frequency in Hz

        [JsonProperty(PropertyName = "FrequencyIncrements")]
        public List<FrequencyIncrements>? FrequencyIncrements { get; set; }
        #endregion
    }

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

    public class VolumeIncrements
    {
        [JsonProperty(PropertyName = "volumeIncrementName")]
        public string? VolumeIncrementName { get; set; }

        [JsonProperty(PropertyName = "volumeIncrementValue")]
        public int VolumeIncrementValue { get; set; }
    }

    public class FrequencyIncrements
    {
        [JsonProperty(PropertyName = "frequencyIncrementName")]
        public string? FrequencyIncrementName { get; set; }
        [JsonProperty(PropertyName = "frequencyIncrementValue")]
        public double FrequencyIncrementValue { get; set; }
    }
    #endregion
}