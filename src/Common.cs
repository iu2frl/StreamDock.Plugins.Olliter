﻿#region Using directives
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

            if (!MQTT_Client.ClientConnected)
            {
                MQTT_Client.ConnectToBroker(MQTT_Config.Host, MQTT_Config.Port, MQTT_Config.User, MQTT_Config.Password, MQTT_Config.UseAuthentication, MQTT_Config.UseWebSocket);
            }
            MQTT_Client.OnMessageReceived += MQTT_Client_OnMessageReceived;
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
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: ReceivedGlobalSettings called: {payload}");
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: ReceivedSettings called: {payload.Settings}");
            try
            {
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
            }
        }

        public virtual void MQTT_StatusReceived(int receiverNumber, ReceiverStatus command)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: MQTT_StatusReceived called");
        }

        public virtual void SettingsUpdated()
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: SettingsUpdated called");
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

            if (!MQTT_Client.ClientConnected)
            {
                MQTT_Client.ConnectToBroker(MQTT_Config.Host, MQTT_Config.Port, MQTT_Config.User, MQTT_Config.Password, MQTT_Config.UseAuthentication, MQTT_Config.UseWebSocket);
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
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: ReceivedSettings called");
            try
            {
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
        }
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

            return instance;
        }

        #region Json properties
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
    #endregion
}