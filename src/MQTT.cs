#region Usings directives
using BarRaider.SdTools;
using MQTTnet;
using MQTTnet.Client;
#endregion

namespace StreamDock.Plugins.Payload
{
    public static class MQTT_Client
    {
        private static IMqttClient? mqttClient;
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(10);
        private static int retryAttempts = 0;
        private static bool disconnecting = false;

        // Events that external classes can subscribe to
        public static event Action<string, string>? OnMessageReceived;
        public static event Action<bool, string>? OnConnectionStatusChanged;

        // Get the subscriber from outside
        public static IMqttClient? Client
        { get => mqttClient; }

        public static bool ConnectWithDefaults()
        {
            // Check if parameters have changed
            if (lastHost == MQTT_Config.Host &&
                lastPort == MQTT_Config.Port &&
                lastUser == MQTT_Config.User &&
                lastPassword == MQTT_Config.Password &&
                lastUseAuthentication == MQTT_Config.UseAuthentication &&
                lastUseWebSocket == MQTT_Config.UseWebSocket &&
                ClientConnected)
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, "MQTT connection parameters unchanged, skipping reconnect");
                return true;
            }

            return ConnectToBroker(MQTT_Config.Host, MQTT_Config.Port, MQTT_Config.User, MQTT_Config.Password, MQTT_Config.UseAuthentication, MQTT_Config.UseWebSocket);
        }

        public static bool ConnectToBroker(string address, int port, string user, string password, bool authUserPass, bool useWebSocket)
        {
            // Check if parameters have changed
            if (lastHost == address &&
                lastPort == port &&
                lastUser == user &&
                lastPassword == password &&
                lastUseAuthentication == authUserPass &&
                lastUseWebSocket == useWebSocket &&
                ClientConnected)
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, "MQTT connection parameters unchanged, skipping reconnect");
                return true;
            }

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
                .WithTimeout(TimeSpan.FromSeconds(1))
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
                    OnMessageReceived?.Invoke(e.ApplicationMessage.Topic, System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment));
                    return Task.CompletedTask;
                };

                // Handle disconnection
                mqttClient.DisconnectedAsync += e =>
                {
                    Logger.Instance.LogMessage(TracingLevel.WARN, "MQTT Client disconnected, scheduling reconnect");
                    ClientConnected = false;
                    OnConnectionStatusChanged?.Invoke(false, "Disconnected");
                    ScheduleReconnect();
                    return Task.CompletedTask;
                };

                ClientConnected = true;
                disconnecting = false;
                retryAttempts = 0;
                
                // Notify subscribers that we're connected
                OnConnectionStatusChanged?.Invoke(true, "Connected");
            }
            else
            {
                mqttClient?.Dispose();
                ClientConnected = false;
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"Failed to connect to MQTT broker");
                OnConnectionStatusChanged?.Invoke(false, "Connection failed");
                return false;
            }

            // Save last parameters
            lastHost = address;
            lastPort = port;
            lastUser = user;
            lastPassword = password;
            lastUseAuthentication = authUserPass;
            lastUseWebSocket = useWebSocket;

            return true;
        }

        private static void ScheduleReconnect()
        {
            if (disconnecting)
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, "Disconnecting, not scheduling reconnect");
                return;
            }

            Logger.Instance.LogMessage(TracingLevel.INFO, $"Attempting to reconnect in {RetryDelay.TotalSeconds} seconds (Attempt {retryAttempts})");
            Task.Delay(RetryDelay).ContinueWith(_ =>
            {
                retryAttempts++;
                Logger.Instance.LogMessage(TracingLevel.DEBUG, "Connection lost, trying to reconnec...");
                ConnectToBroker(MQTT_Config.Host, MQTT_Config.Port, MQTT_Config.User, MQTT_Config.Password, MQTT_Config.UseAuthentication, MQTT_Config.UseWebSocket);
            });
        }

        public static async Task DisconnectFromBroker()
        {
            if (mqttClient != null)
            {
                disconnecting = true;

                // Unsubscribe and disconnect
                try
                {
                    await mqttClient.DisconnectAsync();
                }
                catch (Exception ex)
                {
                    Logger.Instance.LogMessage(TracingLevel.WARN, $"Cannot disconnect from MQTT: {ex.Message}");
                }

                try
                {
                    mqttClient?.Dispose();
                }
                catch
                {
                    //Logger.Instance.LogMessage(TracingLevel.WARN, $"Cannot dispose MQTT object: {ex.Message}");
                }

                ClientConnected = false;
                OnConnectionStatusChanged?.Invoke(false, "Disconnected");
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

        #region Last used parameters
        private static string lastHost { get; set; } = "127.0.0.1";
        private static int lastPort { get; set; } = 1883;
        private static string lastUser { get; set; } = "olliter";
        private static string lastPassword { get; set; } = "madeinitaly";
        private static bool lastUseAuthentication { get; set; } = true;
        private static bool lastUseWebSocket { get; set; } = true;
        #endregion
    }

    public static class MQTT_Config
    {
        public static string Host { get; set; } = "127.0.0.1";
        public static int Port { get; set; } = 9001;
        public static string User { get; set; } = "olliter";
        public static string Password { get; set; } = "madeinitaly";
        public static bool UseAuthentication { get; set; } = false;
        public static bool UseWebSocket { get; set; } = true;
    }

}
