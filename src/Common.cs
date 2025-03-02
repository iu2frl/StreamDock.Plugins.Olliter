using BarRaider.SdTools.Payloads;
using BarRaider.SdTools;
using Coordinates;
using MQTTnet.Client;
using MQTTnet;
using Newtonsoft.Json;
using System.Drawing;
using System.Text.Json.Serialization;
using System.Text;
using System.Text.Json;

namespace Common
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
    public static class MQTT_Client
    {
        private static IMqttClient? mqttClient;
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(10);
        private static int retryAttempts = 0;

        // Event that external classes can subscribe to
        public static event Action<string, string>? OnMessageReceived;

        // Get the subscriber from outside
        public static IMqttClient Client
        { get => mqttClient; }

        public static bool ConnectToBroker(string address, int port, string user, string password, bool authUserPass, bool useWebSocket)
        {
            // Ignore if already connected
            if (mqttClient != null || ClientConnected)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"MQTT Client is already connected, forcing a reconnection");
                DisconnectFromBroker().Wait();
            }

            // Cleanup parameters
            address = address.ToLower().Trim();
            user = user.Trim();
            password = password.Trim();

            // Connection details
            string receiversSetTopic = "receivers/get/#";

            // Create a MQTT client factory
            var factory = new MqttFactory();

            // Create a MQTT client instance
            mqttClient = factory.CreateMqttClient();

            // Create MQTT client options
            MqttClientOptionsBuilder options = new MqttClientOptionsBuilder()
                .WithClientId(Guid.NewGuid().ToString())
                .WithCleanSession(true)
                .WithCleanStart(true)
                .WithKeepAlivePeriod(new TimeSpan(0, 1, 0));

            if (authUserPass)
                options = options.WithCredentials(user, password);

            // TCP Server or Websocket
            if (useWebSocket)
                options = options.WithWebSocketServer(webSocketOptions => webSocketOptions.WithUri($"ws://{address}:{port}/mqtt"));
            else
                options = options.WithTcpServer(address, port);

            try
            {
                // Connect to MQTT broker
                mqttClient.ConnectAsync(options.Build()).Wait();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"Failed to connect to MQTT broker: {ex.Message}");
                ClientConnected = false;
                ScheduleReconnect();
                return false;
            }

            if (mqttClient.IsConnected)
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, "Connected to MQTT broker successfully");

                try
                {
                    // Subscribe to a topic
                    mqttClient.SubscribeAsync(receiversSetTopic).Wait();
                }
                catch (Exception ex)
                {
                    Logger.Instance.LogMessage(TracingLevel.ERROR, $"Cannot subscribe: {ex.Message}");
                }

                // Callback function when a message is received
                mqttClient.ApplicationMessageReceivedAsync += e =>
                {
                    OnMessageReceived?.Invoke(e.ApplicationMessage.Topic, Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment));
                    return Task.CompletedTask;
                };

                // Handle disconnection
                mqttClient.DisconnectedAsync += e =>
                {
                    Logger.Instance.LogMessage(TracingLevel.WARN, "MQTT Client disconnected, scheduling reconnect");
                    ScheduleReconnect();
                    return Task.CompletedTask;
                };

                ClientConnected = true;
            }
            else
            {
                mqttClient?.Dispose();
                ClientConnected = false;
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"Failed to connect to MQTT broker");
                ScheduleReconnect();
                return false;
            }

            return true;
        }

        private static void ScheduleReconnect()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Attempting to reconnect in {RetryDelay.TotalSeconds} seconds (Attempt {retryAttempts})");
            Task.Delay(RetryDelay).ContinueWith(_ =>
            {
                retryAttempts++;
                ConnectToBroker(MQTT_Config.Host, MQTT_Config.Port, MQTT_Config.User, MQTT_Config.Password, MQTT_Config.UseAuthentication, MQTT_Config.UseWebSocket);
            });
        }

        public static async Task DisconnectFromBroker()
        {
            if (mqttClient != null)
            {
                // Unsubscribe and disconnect
                try
                {
                    await mqttClient.DisconnectAsync();
                    mqttClient?.Dispose();
                }
                catch (Exception ex)
                {
                    Logger.Instance.LogMessage(TracingLevel.WARN, $"Cannot dispose MQTT object: {ex.Message}");
                }
                ClientConnected = false;
                Logger.Instance.LogMessage(TracingLevel.INFO, "Disconnected from broker successfully");
            }
        }

        public static async Task PublishMessageAsync(string topic, string payload)
        {
            if (!ClientConnected)
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, "MQTT is not connected, trying to reconnect");

                if (!ConnectToBroker(MQTT_Config.Host, MQTT_Config.Port, MQTT_Config.User, MQTT_Config.Password, MQTT_Config.UseAuthentication, MQTT_Config.UseWebSocket))
                    Logger.Instance.LogMessage(TracingLevel.WARN, "Cannot connect to MQTT broker");

                return;
            }

            try
            {
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(payload)
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithRetainFlag(false)
                    .Build();

                if (mqttClient != null)
                {
                    await mqttClient.PublishAsync(message);
                    Logger.Instance.LogMessage(TracingLevel.DEBUG, $"Published message: Topic={topic}, Payload={payload}");
                }
                else
                {
                    Logger.Instance.LogMessage(TracingLevel.WARN, "MQTT Client is null, cannot send message");
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"Cannot send message: {ex.Message}");
            }
        }

        #region Public properties
        public static bool ClientConnected { get; private set; } = false;
        #endregion
    }

    public static class MQTT_Config
    {
        public static string Host { get; set; } = "127.0.0.1";
        public static int Port { get; set; } = 1883;
        public static string User { get; set; } = "olliter";
        public static string Password { get; set; } = "madeinitaly";
        public static bool UseAuthentication { get; set; } = true;
        public static bool UseWebSocket { get; set; } = true;
    }

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
        private Common.PluginSettings _settings = new();

        public PluginSettings Settings
        {
            get => _settings;
        }

        #region StreamDock events
        public BaseKeypadMqttItem(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
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
                Connection.SetImageAsync(Common.StreamDock.UpdateKeyImage($"Connection\nError")).Wait();
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
        private Common.PluginSettings _settings = new();

        public PluginSettings Settings
        {
            get => _settings;
        }

        public BaseDialMqttItem(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
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
                Connection.SetImageAsync(Common.StreamDock.UpdateKeyImage($"Connection\nError")).Wait();
            }
            else
            {
                Connection.SetImageAsync(Common.StreamDock.UpdateKeyImage("Connected")).Wait();
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

        [JsonProperty(PropertyName = "RxIndex")]
        public int RxIndex { get; set; } = 1;

        [JsonProperty(PropertyName = "SubRx")]
        public bool SubRx { get; set; } = false;

        [JsonProperty(PropertyName = "RxBand")]
        public string RxBand { get; set; } = "B20M";

        [JsonProperty(PropertyName = "VolumeIncrement")]
        public int VolumeIncrement { get; set; } = 10; // %

        [JsonProperty(PropertyName = "FrequencyIncrement")]
        public double FrequencyIncrement { get; set; } = 0; // Frequency in Hz
    }

    #endregion
}