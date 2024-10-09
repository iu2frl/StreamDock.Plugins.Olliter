#region Using directives
using System.Drawing;
using BarRaider.SdTools;
using BarRaider.SdTools.Events;
using BarRaider.SdTools.Wrappers;
using MQTTnet;
using System.Text.Json.Serialization;
using System.Text.Json;
using MQTTnet.Client;
using Coordinates;
using System.Text;
using Common;
using BarRaider.SdTools.Payloads;
#endregion

#region Streamdeck Commands
namespace ToggleRx1
{
    // Name: Toggle Receiver 1
    // Tooltip: Toggle main power to RX 1
    [PluginActionId("it.iu2frl.streamdock.olliter.togglerx1")]
    public class ToggleRx1(ISDConnection connection, InitialPayload payload) : Common.BaseKeypadMqttItem(connection, payload)
    {
        public override void KeyPressed(KeyPayload payload)
        {
            var command = new Common.ReceiverCommand
            {
                Action = "toggle",
                Command = "enable",
                SubReceiver = "false"
            };
            Common.MQTT_Client.PublishMessageAsync("receivers/command/1", JsonSerializer.Serialize(command)).Wait();
            Logger.Instance.LogMessage(TracingLevel.INFO, "KeyPressed called");
        }

        private async void Connection_OnTitleParametersDidChange(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.TitleParametersDidChange> e)
        {
            await Connection.SetImageAsync(Common.StreamDock.UpdateKeyImage($"RX 1"));
        }

        public override void MQTT_StatusReceived(int receiverNumber, ReceiverStatus command)
        {
            try
            {
                if (receiverNumber == 1)
                {
                    if (command.ReceiverA.Enabled == "True")
                    {
                        Connection.SetImageAsync(Common.StreamDock.UpdateKeyImage($"RX1\nEnabled")).Wait();
                    }
                    else
                    {
                        Connection.SetImageAsync(Common.StreamDock.UpdateKeyImage($"RX1\nDisabled")).Wait();
                    }
                }
            }
            catch (Exception retExc)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"Cannot parse payload: {retExc.Message}");
            }
        }
    }
}

namespace ToggleMox1
{
    // Name: MOX Receiver 1
    // Tooltip: Toggle receive/transmit RX 1
    [PluginActionId("it.iu2frl.streamdock.olliter.togglemox1")]
    public class ToggleRx1(ISDConnection connection, InitialPayload payload) : Common.BaseKeypadMqttItem(connection, payload)
    {
        public override void KeyPressed(KeyPayload payload)
        {
            var command = new Common.ReceiverCommand
            {
                Action = "toggle",
                Command = "mox",
                SubReceiver = "false"
            };
            Common.MQTT_Client.PublishMessageAsync("receivers/command/1", JsonSerializer.Serialize(command)).Wait();
            Logger.Instance.LogMessage(TracingLevel.INFO, "KeyPressed called");
        }

        private async void Connection_OnTitleParametersDidChange(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.TitleParametersDidChange> e)
        {
            await Connection.SetImageAsync(Common.StreamDock.UpdateKeyImage($"MOX 1"));
        }

        public override void MQTT_StatusReceived(int receiverNumber, ReceiverStatus command)
        {
            try
            {
                if (receiverNumber == 1)
                {
                    if (command.ReceiverA.TxVfo == "True" && command.ReceiverA.Mox == "True")
                    {
                        Connection.SetImageAsync(Common.StreamDock.UpdateKeyImage($"RX1\nTransmit")).Wait();
                    }
                    else
                    {
                        Connection.SetImageAsync(Common.StreamDock.UpdateKeyImage($"RX1\nReceive")).Wait();
                    }
                }
            }
            catch (Exception retExc)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"Cannot parse payload: {retExc.Message}");
            }
        }
    }
}

namespace TuneRx1
{
    // Name: Tune Receiver 1
    // Tooltip: Frequency knob for RX 1
    [PluginActionId("it.iu2frl.streamdock.olliter.tunerx1")]
    public class TuneRx1(ISDConnection connection, InitialPayload payload) : Common.BaseDialMqttItem(connection, payload)
    {
        public override void DialUp(DialPayload payload)
        {
            var command = new Common.ReceiverCommand
            {
                Action = "+",
                Command = "frequency",
                Value = "",
                SubReceiver = "false"
            };
            Common.MQTT_Client.PublishMessageAsync("receivers/command/1", JsonSerializer.Serialize(command)).Wait();
        }
    }
}
#endregion

#region Common objects
namespace Common
{
    public class StreamDock
    {
        #region Private Methods
        public static Bitmap? UpdateKeyImage(string value)
        {
            Bitmap bmp;
            try
            {
                bmp = new Bitmap(ImageHelper.GetImage(Color.Black));

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
        #endregion
    }

    #region Custom classes
    public static class MQTT_Client
    {
        private static IMqttClient mqttClient;

        // Event that external classes can subscribe to
        public static event Action<string, string> OnMessageReceived;

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

                // Update public properties
                ServerAddress = address;
                ClientConnected = true;
                // Update private properties
                mqttHost = address;
                mqttUser = user;
                mqttPassword = password;
                mqttPort = port;
                mqttUseAuthentication = authUserPass;
                mqttUseWebSocket = useWebSocket;
            }
            else
            {
                mqttClient?.Dispose();
                ClientConnected = false;
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"Failed to connect to MQTT broker");
                return false;
            }

            return true;
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
                Logger.Instance.LogMessage(TracingLevel.INFO, "Disconnected from broker succesfully");
            }
        }

        public static async Task PublishMessageAsync(string topic, string payload)
        {
            if (!ClientConnected)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, "MQTT is not connected, trying to reconnect");
                
                if (!ConnectToBroker(mqttHost, mqttPort, mqttUser, mqttPassword, mqttUseAuthentication, mqttUseWebSocket))
                    Logger.Instance.LogMessage(TracingLevel.INFO, "Cannot connect to MQTT broker");

                return;
            }

            try
            {
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(payload)
                    .WithQualityOfServiceLevel(0)
                    .WithRetainFlag(false)
                    .Build();

                await mqttClient.PublishAsync(message);

                Logger.Instance.LogMessage(TracingLevel.INFO, $"Published message: Topic={topic}, Payload={payload}");
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"Cannot send message: {ex.Message}");
            }
        }

        #region Public properties
        public static bool ClientConnected { get; private set; }

        public static string ServerAddress { get; private set; }
        #endregion

        #region Private properties
        private static string mqttUser = "olliter";
        private static string mqttPassword = "madeinitaly";
        private static string mqttHost = "127.0.0.1";
        private static int mqttPort = 1883;
        private static bool mqttUseAuthentication = true;
        private static bool mqttUseWebSocket = true;
        #endregion
    }

    public class ReceiverCommand
    {
        [JsonPropertyName("software_id")]
        public string SoftwareId = Environment.MachineName;

        [JsonPropertyName("subreceiver")]
        public string SubReceiver { get; set; }

        [JsonPropertyName("command")]
        public string Command { get; set; }

        [JsonPropertyName("action")]
        public string Action { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class ReceiverStatus
    {
        [JsonPropertyName("software_id")]
        public string SoftwareId { get; set; }

        [JsonPropertyName("txpower")]
        public string TxPower { get; set; }

        [JsonPropertyName("monitor_vol")]
        public string MonitorVolume { get; set; }

        [JsonPropertyName("band")]
        public string Band { get; set; }

        [JsonPropertyName("swr")]
        public string SWR { get; set; }

        [JsonPropertyName("master_vol")]
        public string MasterVolume { get; set; }

        [JsonPropertyName("temperature")]
        public string Temperature { get; set; }

        [JsonPropertyName("current")]
        public string Current { get; set; }

        [JsonPropertyName("receiver_a")]
        public ReceiverStatusDetail ReceiverA { get; set; }

        [JsonPropertyName("receiver_b")]
        public ReceiverStatusDetail ReceiverB { get; set; }
    }

    public class ReceiverStatusDetail
    {
        [JsonPropertyName("active")]
        public string Enabled { get; set; }

        [JsonPropertyName("frequency")]
        public string Frequency { get; set; }

        [JsonPropertyName("mode")]
        public string Mode { get; set; }

        [JsonPropertyName("filterlow")]
        public string FilterLow { get; set; }

        [JsonPropertyName("filterhigh")]
        public string FilterHigh { get; set; }

        [JsonPropertyName("volume")]
        public string Volume { get; set; }

        [JsonPropertyName("squelch")]
        public string Squelch { get; set; }

        [JsonPropertyName("mox")]
        public string Mox { get; set; }

        [JsonPropertyName("txvfo")]
        public string TxVfo { get; set; }

        [JsonPropertyName("signal")]
        public string Signal { get; set; }
    }

    public class BaseKeypadMqttItem : KeypadBase
    {
        #region StreamDock events
        public BaseKeypadMqttItem(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            MQTT_Client.ConnectToBroker("127.0.0.1", 1883, "olliter", "madeinitaly", true, true);
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
        }

        public override void Dispose()
        {
            MQTT_Client.DisconnectFromBroker().Wait();
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType().Name}: ReceivedSettings called");
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType().Name}: ReceivedGlobalSettings called");
        }
        #endregion

        #region Custom events
        private void MQTT_Client_OnMessageReceived(string topic, string payload)
        {
            //Logger.Instance.LogMessage(TracingLevel.INFO, "MQTT Message received");
            var command = JsonSerializer.Deserialize<ReceiverStatus>(payload);
            int.TryParse(topic.Substring(topic.Length - 1, 1), out var receiverNumber);
            if (command != null && receiverNumber > 0 && receiverNumber <= 4)
            {
                MQTT_StatusReceived(receiverNumber, command);
            }
        }

        public virtual void MQTT_StatusReceived(int receiverNumber, ReceiverStatus command)
        {
            
        }
        #endregion
    }

    public class BaseDialMqttItem : EncoderBase
    {
        public BaseDialMqttItem(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            MQTT_Client.ConnectToBroker("127.0.0.1", 1883, "olliter", "madeinitaly", true, true);
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

        public override void OnTick() => throw new NotImplementedException();
        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType().Name}: ReceivedGlobalSettings called");
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType().Name}: ReceivedSettings called");
        }

        public override void TouchPress(TouchpadPressPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{GetType().Name}: TouchPress called");
        }
    }
    #endregion
}
#endregion

#region Debug stuff
namespace StreamDock.Plugins.Payload
{
    // Name: Debug
    // Tooltip: This function only prints to the log
    [PluginActionId("it.iu2frl.streamdock.debug")]
    public class PluginButtonAction : KeypadBase
    {
        public PluginButtonAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            Connection.OnApplicationDidLaunch += Connection_OnApplicationDidLaunch;
            Connection.OnApplicationDidTerminate += Connection_OnApplicationDidTerminate;
            Connection.OnDeviceDidConnect += Connection_OnDeviceDidConnect;
            Connection.OnDeviceDidDisconnect += Connection_OnDeviceDidDisconnect;
            Connection.OnPropertyInspectorDidAppear += Connection_OnPropertyInspectorDidAppear;
            Connection.OnPropertyInspectorDidDisappear += Connection_OnPropertyInspectorDidDisappear;
            Connection.OnSendToPlugin += Connection_OnSendToPlugin;
            Connection.OnTitleParametersDidChange += Connection_OnTitleParametersDidChange;
        }

        private async void Connection_OnTitleParametersDidChange(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.TitleParametersDidChange> e)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "OnTitleParametersDidChange Event Handled");

            await Connection.SetImageAsync(Common.StreamDock.UpdateKeyImage($"[{e.Event.Payload.Coordinates.Row}, {e.Event.Payload.Coordinates.Column}]")); // 초기 이미지 출력
        }
        private void Connection_OnSendToPlugin(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.SendToPlugin> e)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "OnSendToPlugin Event Handled");
        }
        private void Connection_OnPropertyInspectorDidAppear(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.PropertyInspectorDidAppear> e)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "OnPropertyInspectorDidAppear Event Handled");

        }
        private void Connection_OnPropertyInspectorDidDisappear(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.PropertyInspectorDidDisappear> e)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "OnPropertyInspectorDidDisappear Event Handled");
        }
        private void Connection_OnDeviceDidDisconnect(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.DeviceDidDisconnect> e)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "OnDeviceDidDisconnect Event Handled");
        }
        private void Connection_OnDeviceDidConnect(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.DeviceDidConnect> e)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "OnDeviceDidConnect Event Handled");
        }
        private void Connection_OnApplicationDidTerminate(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.ApplicationDidTerminate> e)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "OnApplicationDidTerminate Event Handled");
        }
        private void Connection_OnApplicationDidLaunch(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.ApplicationDidLaunch> e)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "OnApplicationDidLaunch Event Handled");
        }
        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Destructor called");
        }
        public override void KeyPressed(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "KeyPressed called");
        }
        public override void KeyReleased(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "KeyReleased called");
        }
        public override void OnTick()
        {
        }
        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "ReceivedSettings called");
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "ReceivedGlobalSettings called");
        }
    }
}
#endregion
