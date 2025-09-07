#region Using directives
using System.Diagnostics;
using System.Globalization;
using BarRaider.SdTools;
using BarRaider.SdTools.Payloads;
using BarRaider.SdTools.Wrappers;
#endregion

namespace StreamDock.Plugins.Payload
{
    #region Keypad controls

    // Name: Toggle Receiver
    // Tooltip: Toggle main power to a receiver
    // Controllers: Keypad
    // PropertyInspector: ./property_inspector/pi-rx-sub.html
    [PluginActionId("it.iu2frl.streamdock.olliter.togglerx")]
    public class ToggleRx(ISDConnection connection, InitialPayload payload) : BaseKeypadMqttItem(connection, payload)
    {
        public override void KeyPressed(KeyPayload payload)
        {
            var receiverCommand = new ReceiverCommand
            {
                Command = "enable",
                Action = "toggle",
                SubReceiver = base.Settings.SubRx > 0 ? "true" : "false",
                Value = ""
            };
            string command = System.Text.Json.JsonSerializer.Serialize(receiverCommand);
            string topic = $"receivers/command/{base.Settings.RxIndex}";
            MQTT_Client.PublishMessageAsync(topic, command).Wait();
            Logger.Instance.LogMessage(TracingLevel.INFO, "KeyPressed called with: ");
        }

        public override void MQTT_StatusReceived(int receiverNumber, ReceiverStatus command)
        {
            try
            {
                if (receiverNumber == base.Settings.RxIndex)
                {
                    if (base.Settings.SubRx > 0)
                    {
                        var rxLine = $"RX{base.Settings.RxIndex} Sub";
                        var rxStatus = command.ReceiverB.Enabled == "True" ? "Enabled" : "Disabled";
                        Connection.SetImageAsync(StreamDock.UpdateKeyImage($"{rxLine}\n{rxStatus}")).Wait();
                    }
                    else
                    {
                        var rxLine = $"RX{base.Settings.RxIndex} Main";
                        var rxStatus = command.ReceiverA.Enabled == "True" ? "Enabled" : "Disabled";
                        Connection.SetImageAsync(StreamDock.UpdateKeyImage($"{rxLine}\n{rxStatus}")).Wait();
                    }
                }
            }
            catch (Exception retExc)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"Cannot parse payload: {retExc.Message}");
            }
        }

        public override void SettingsUpdated()
        {
            base.SettingsUpdated();
            Connection.SetImageAsync(StreamDock.UpdateKeyImage($"RX {base.Settings.RxIndex}\nStatus")).Wait();
        }
    }

    // Name: Toggle MOX
    // Tooltip: Toggle MOX on a receiver
    // Controllers: Keypad
    // PropertyInspector: ./property_inspector/pi-rx-sub.html
    [PluginActionId("it.iu2frl.streamdock.olliter.togglemox")]
    public class ToggleMox(ISDConnection connection, InitialPayload payload) : BaseKeypadMqttItem(connection, payload)
    {
        public override void KeyPressed(KeyPayload payload)
        {
            var receiverCommand = new ReceiverCommand
            {
                Command = "mox",
                Action = "toggle",
                SubReceiver = base.Settings.SubRx > 0 ? "true" : "false",
                Value = ""
            };
            string command = System.Text.Json.JsonSerializer.Serialize(receiverCommand);
            string topic = $"receivers/command/{base.Settings.RxIndex}";
            MQTT_Client.PublishMessageAsync(topic, command).Wait();
            Logger.Instance.LogMessage(TracingLevel.INFO, "KeyPressed called with: ");
        }
        public override void MQTT_StatusReceived(int receiverNumber, ReceiverStatus command)
        {
            try
            {
                if (receiverNumber == base.Settings.RxIndex)
                {
                    if (base.Settings.SubRx > 0)
                    {
                        var rxLine = $"RX{base.Settings.RxIndex} Sub";
                        var rxStatus = command.ReceiverB.Mox == "True" ? "TX" : "RX";
                        Connection.SetImageAsync(StreamDock.UpdateKeyImage($"{rxLine}\n{rxStatus}")).Wait();
                    }
                    else
                    {
                        var rxLine = $"RX{base.Settings.RxIndex} Main";
                        var rxStatus = command.ReceiverA.Mox == "True" ? "TX" : "RX";
                        Connection.SetImageAsync(StreamDock.UpdateKeyImage($"{rxLine}\n{rxStatus}")).Wait();
                    }
                }
            }
            catch (Exception retExc)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"Cannot parse payload: {retExc.Message}");
            }
        }
        public override void SettingsUpdated()
        {
            base.SettingsUpdated();
            Connection.SetImageAsync(StreamDock.UpdateKeyImage($"RX {base.Settings.RxIndex}\nMOX")).Wait();
        }

    }

    // Name: Change Band
    // Tooltip: Change band on a receiver
    // Controllers: Keypad
    // PropertyInspector: ./property_inspector/pi-rx-band.html
    [PluginActionId("it.iu2frl.streamdock.olliter.changeband")]
    public class ChangeBand(ISDConnection connection, InitialPayload payload) : BaseKeypadMqttItem(connection, payload)
    {
        public override void KeyPressed(KeyPayload payload)
        {
            var receiverCommand = new ReceiverStatus
            {
                Band = base.Settings.RxBand
            };

            string command = System.Text.Json.JsonSerializer.Serialize(receiverCommand);
            string topic = $"receivers/set/{base.Settings.RxIndex}";
            MQTT_Client.PublishMessageAsync(topic, command).Wait();
        }
        public override void MQTT_StatusReceived(int receiverNumber, ReceiverStatus command)
        {
            try
            {
                if (receiverNumber == base.Settings.RxIndex && !string.IsNullOrEmpty(command.Band))
                {
                    var rxLine = "";
                    if (base.Settings.RxBand != command.Band)
                    {
                        rxLine = $"SET\n";
                    }

                    rxLine += $"RX{base.Settings.RxIndex}";

                    var rxBand = base.Settings.RxBand.Replace("B", "").ToUpper();

                    Connection.SetImageAsync(StreamDock.UpdateKeyImage($"{rxLine}\n{rxBand}")).Wait();
                }
            }
            catch (Exception retExc)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"Cannot parse payload: {retExc.Message}");
            }
        }
        public override void SettingsUpdated()
        {
            base.SettingsUpdated();
            Connection.SetImageAsync(StreamDock.UpdateKeyImage($"RX {base.Settings.RxIndex}\nBand")).Wait();
        }
    }

    // Name: Increase Frequency
    // Tooltip: Increase frequency on a receiver using buttons
    // Controllers: Keypad
    // PropertyInspector: ./property_inspector/pi-rx-sub-frequency.html
    [PluginActionId("it.iu2frl.streamdock.olliter.increasefrequencybuttons")]
    public class ChangeFrequencyButtons(ISDConnection connection, InitialPayload payload) : BaseKeypadMqttItem(connection, payload)
    {
        public override void KeyPressed(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: KeyPressed called");
            var increment = "";

            if (base.Settings.FrequencyIncrement > 0)
            {
                increment = (base.Settings.FrequencyIncrement / 1000000).ToString();
            }

            var receiverCommand = new ReceiverCommand
            {
                Command = "frequency",
                Action = "+",
                SubReceiver = base.Settings.SubRx > 0 ? "true" : "false",
                Value = increment
            };
            string command = System.Text.Json.JsonSerializer.Serialize(receiverCommand);
            string topic = $"receivers/command/{base.Settings.RxIndex}";
            MQTT_Client.PublishMessageAsync(topic, command).Wait();
        }

        public override void MQTT_StatusReceived(int receiverNumber, ReceiverStatus command)
        {
            try
            {
                if (receiverNumber == base.Settings.RxIndex)
                {
                    var receiverFrequency = base.Settings.SubRx > 0 ? command.ReceiverA.Frequency : command.ReceiverA.Frequency;
                    var receiverFrequencyValue = Convert.ToDouble(receiverFrequency, CultureInfo.InvariantCulture) * 1000;

                    var rxLine = $"RX{base.Settings.RxIndex} " + (base.Settings.SubRx > 0 ? "Sub" : "Main");
                    var rxStatus = receiverFrequencyValue.ToString("F3");
                    Connection.SetImageAsync(StreamDock.UpdateKeyImage($"{rxLine}\n{rxStatus}\n+{base.Settings.FrequencyIncrement}Hz")).Wait();
                }
            }
            catch (Exception retExc)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"Cannot parse payload: {retExc.Message}");
            }
        }

        public override void SettingsUpdated()
        {
            base.SettingsUpdated();
            Connection.SetImageAsync(StreamDock.UpdateKeyImage($"RX{base.Settings.RxIndex}\nFrequency")).Wait();
        }
    }

    // Name: Decrease Frequency
    // Tooltip: Decrease frequency on a receiver using buttons
    // Controllers: Keypad
    // PropertyInspector: ./property_inspector/pi-rx-sub-frequency.html
    [PluginActionId("it.iu2frl.streamdock.olliter.decreasefrequencybuttons")]
    public class DecreaseFrequencyButtons(ISDConnection connection, InitialPayload payload) : BaseKeypadMqttItem(connection, payload)
    {
        public override void KeyPressed(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: KeyPressed called");
            var increment = "";
            if (base.Settings.FrequencyIncrement > 0)
            {
                increment = (base.Settings.FrequencyIncrement / 1000000).ToString();
            }
            var receiverCommand = new ReceiverCommand
            {
                Command = "frequency",
                Action = "-",
                SubReceiver = base.Settings.SubRx > 0 ? "true" : "false",
                Value = increment
            };
            string command = System.Text.Json.JsonSerializer.Serialize(receiverCommand);
            string topic = $"receivers/command/{base.Settings.RxIndex}";
            MQTT_Client.PublishMessageAsync(topic, command).Wait();
        }

        public override void MQTT_StatusReceived(int receiverNumber, ReceiverStatus command)
        {
            try
            {
                if (receiverNumber == base.Settings.RxIndex)
                {
                    if (receiverNumber == base.Settings.RxIndex)
                    {
                        var receiverFrequency = base.Settings.SubRx > 0 ? command.ReceiverA.Frequency : command.ReceiverA.Frequency;
                        var receiverFrequencyValue = Convert.ToDouble(receiverFrequency, CultureInfo.InvariantCulture) * 1000;

                        var rxLine = $"RX{base.Settings.RxIndex} " + (base.Settings.SubRx > 0 ? "Sub" : "Main");
                        var rxStatus = receiverFrequencyValue.ToString("F3");
                        Connection.SetImageAsync(StreamDock.UpdateKeyImage($"{rxLine}\n{rxStatus}\n-{base.Settings.FrequencyIncrement}Hz")).Wait();
                    }
                }
            }
            catch (Exception retExc)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"Cannot parse payload: {retExc.Message}");
            }
        }

        public override void SettingsUpdated()
        {
            base.SettingsUpdated();
            Connection.SetImageAsync(StreamDock.UpdateKeyImage($"RX{base.Settings.RxIndex}\nFrequency")).Wait();
        }
    }

    // Name: Increase Volume
    // Tooltip: Increase volume on a receiver using buttons
    // Controllers: Keypad
    // PropertyInspector: ./property_inspector/pi-rx-sub-volume.html
    [PluginActionId("it.iu2frl.streamdock.olliter.increasevolumebuttons")]
    public class IncreaseVolumeButtons(ISDConnection connection, InitialPayload payload) : BaseKeypadMqttItem(connection, payload)
    {
        public override void KeyPressed(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: KeyPressed called");
            var increment = "15";
            if (base.Settings.VolumeIncrement > 0)
            {
                increment = base.Settings.VolumeIncrement.ToString();
            }
            var receiverCommand = new ReceiverCommand
            {
                Command = "volume",
                Action = "+",
                SubReceiver = base.Settings.SubRx > 0 ? "true" : "false",
                Value = increment
            };
            string command = System.Text.Json.JsonSerializer.Serialize(receiverCommand);
            string topic = $"receivers/command/{base.Settings.RxIndex}";
            MQTT_Client.PublishMessageAsync(topic, command).Wait();
        }

        public override void MQTT_StatusReceived(int receiverNumber, ReceiverStatus command)
        {
            try
            {
                if (receiverNumber == base.Settings.RxIndex)
                {
                    var receiverVolume = base.Settings.SubRx > 0 ? command.ReceiverA.Volume : command.ReceiverA.Volume;
                    var rxLine = $"RX{base.Settings.RxIndex} " + (base.Settings.SubRx > 0 ? "Sub" : "Main");
                    var rxStatus = $"{receiverVolume}%";
                    Connection.SetImageAsync(StreamDock.UpdateKeyImage($"{rxLine}\n{rxStatus}\n+{base.Settings.VolumeIncrement}%")).Wait();
                }
            }
            catch (Exception retExc)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"Cannot parse payload: {retExc.Message}");
            }
        }
        public override void SettingsUpdated()
        {
            base.SettingsUpdated();
            Connection.SetImageAsync(StreamDock.UpdateKeyImage($"RX {base.Settings.RxIndex}\nVolume")).Wait();
        }
    }

    // Name: Decrease Volume
    // Tooltip: Decrease volume on a receiver using buttons
    // Controllers: Keypad
    // PropertyInspector: ./property_inspector/pi-rx-sub-volume.html
    [PluginActionId("it.iu2frl.streamdock.olliter.decreasevolumebuttons")]
    public class DecreaseVolumeButtons(ISDConnection connection, InitialPayload payload) : BaseKeypadMqttItem(connection, payload)
    {
        public override void KeyPressed(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: KeyPressed called");
            var increment = "15";
            if (base.Settings.VolumeIncrement > 0)
            {
                increment = base.Settings.VolumeIncrement.ToString();
            }
            var receiverCommand = new ReceiverCommand
            {
                Command = "volume",
                Action = "-",
                SubReceiver = base.Settings.SubRx > 0 ? "true" : "false",
                Value = increment
            };
            string command = System.Text.Json.JsonSerializer.Serialize(receiverCommand);
            string topic = $"receivers/command/{base.Settings.RxIndex}";
            MQTT_Client.PublishMessageAsync(topic, command).Wait();
        }
        public override void MQTT_StatusReceived(int receiverNumber, ReceiverStatus command)
        {
            try
            {
                if (receiverNumber == base.Settings.RxIndex)
                {
                    var receiverVolume = base.Settings.SubRx > 0 ? command.ReceiverA.Volume : command.ReceiverA.Volume;
                    var rxLine = $"RX{base.Settings.RxIndex} " + (base.Settings.SubRx > 0 ? "Sub" : "Main");
                    var rxStatus = $"{receiverVolume}%";
                    Connection.SetImageAsync(StreamDock.UpdateKeyImage($"{rxLine}\n{rxStatus}\n-{base.Settings.VolumeIncrement}%")).Wait();
                }
            }
            catch (Exception retExc)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"Cannot parse payload: {retExc.Message}");
            }
        }
        public override void SettingsUpdated()
        {
            base.SettingsUpdated();
            Connection.SetImageAsync(StreamDock.UpdateKeyImage($"RX {base.Settings.RxIndex}\nVolume")).Wait();
        }
    }

    // Name: Change receiver mode
    // Tooltip: Change receiver mode on a receiver
    // Controllers: Keypad
    // PropertyInspector: ./property_inspector/pi-rx-mode.html
    [PluginActionId("it.iu2frl.streamdock.olliter.changemode")]
    public class ChangeMode(ISDConnection connection, InitialPayload payload) : BaseKeypadMqttItem(connection, payload)
    {
        public override void KeyPressed(KeyPayload payload)
        {
            var receiverCommand = new ReceiverCommand
            {
                Command = "mode",
                Action = "",
                SubReceiver = "false",
                Value = Settings.SdrMode
            };
            string command = System.Text.Json.JsonSerializer.Serialize(receiverCommand);
            string topic = $"receivers/command/{base.Settings.RxIndex}";
            MQTT_Client.PublishMessageAsync(topic, command).Wait();
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"Changing mode to {Settings.SdrMode}");
        }
        public override void MQTT_StatusReceived(int receiverNumber, ReceiverStatus command)
        {
            try
            {
                if (receiverNumber == base.Settings.RxIndex)
                {
                    var rxLine = "";
                    if (base.Settings.SdrMode != command.ReceiverA.Mode)
                    {
                        rxLine = $"SET\n";
                    }

                    rxLine += $"RX{base.Settings.RxIndex}";
                    var rxStatus = Settings.SdrMode;
                    Connection.SetImageAsync(StreamDock.UpdateKeyImage($"{rxLine}\n{rxStatus}")).Wait();
                }
            }
            catch (Exception retExc)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"Cannot parse payload: {retExc.Message}");
            }
        }
        public override void SettingsUpdated()
        {
            base.SettingsUpdated();
            Connection.SetImageAsync(StreamDock.UpdateKeyImage($"RX {base.Settings.RxIndex}\nMode")).Wait();
        }
    }

    // Name: Send keyer message
    // Tooltip: Sends one of the keyer messages configured in OL-Master
    // Controllers: Keypad
    // PropertyInspector: ./property_inspector/pi-keyer-msg.html
    [PluginActionId("it.iu2frl.streamdock.olliter.keyersendmsg")]
    public class SendKeyerMessage(ISDConnection connection, InitialPayload payload) : BaseKeypadMqttItem(connection, payload)
    {
        public override void KeyPressed(KeyPayload payload)
        {
            var receiverCommand = new ReceiverCommand
            {
                Command = "keyer",
                Action = "sendmem",
                SubReceiver = "false",
                Value = Settings.KeyerMsgIndex.ToString()
            };
            string command = System.Text.Json.JsonSerializer.Serialize(receiverCommand);
            string topic = $"receivers/command/{base.Settings.RxIndex}";
            MQTT_Client.PublishMessageAsync(topic, command).Wait();
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"Changing mode to {Settings.SdrMode}");
        }
        public override void OnTick()
        {
            base.OnTick();

            if (!base.Timeout)
                UpdateText();
        }

        public override void SettingsUpdated()
        {
            base.SettingsUpdated();
            UpdateText();
        }

        private void UpdateText()
        {
            var btnMessage = $"Send Keyer\nMSG: #{base.Settings.KeyerMsgIndex}";
            Connection.SetImageAsync(StreamDock.UpdateKeyImage($"{btnMessage}")).Wait();
        }
    }

    // Name: Send text using keyer
    // Tooltip: Sends arbitrary text using the keyer of OL-Master
    // Controllers: Keypad
    // PropertyInspector: ./property_inspector/pi-keyer-text.html
    [PluginActionId("it.iu2frl.streamdock.olliter.keyersendtext")]
    public class SendKeyerText(ISDConnection connection, InitialPayload payload) : BaseKeypadMqttItem(connection, payload)
    {
        public override void KeyPressed(KeyPayload payload)
        {
            var receiverCommand = new ReceiverCommand
            {
                Command = "keyer",
                Action = "sendtext",
                SubReceiver = "false",
                Value = Settings.KeyerText
            };
            string command = System.Text.Json.JsonSerializer.Serialize(receiverCommand);
            string topic = $"receivers/command/{base.Settings.RxIndex}";
            MQTT_Client.PublishMessageAsync(topic, command).Wait();
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"Changing mode to {Settings.SdrMode}");
        }

        public override void SettingsUpdated()
        {
            base.SettingsUpdated();
            UpdateText();
        }

        public override void OnTick()
        {
            base.OnTick();

            if (!base.Timeout)
                UpdateText();
        }

        private void UpdateText()
        {
            var btnMessage = "Send text:\n";
            if (base.Settings.KeyerText.Length > 12)
            {
                btnMessage += base.Settings.KeyerText.Substring(0, 10) + "..";
            }
            else
            {
                btnMessage += base.Settings.KeyerText;
            }
            Connection.SetImageAsync(StreamDock.UpdateKeyImage($"{btnMessage}")).Wait();
        }
    }

    #endregion

    #region Knob controls

    // Name: Change Frequency
    // Tooltip: Change frequency on a receiver using a knob
    // Controllers: Knob
    // PropertyInspector: ./property_inspector/pi-rx-sub-frequency.html
    [PluginActionId("it.iu2frl.streamdock.olliter.changefrequency")]
    public class TuneRx(ISDConnection connection, InitialPayload payload) : BaseDialMqttItem(connection, payload)
    {
        private string lastBand = "";

        public override void MQTT_StatusReceived(int receiverNumber, ReceiverStatus command)
        {
            try
            {
                if (receiverNumber == base.Settings.RxIndex)
                {
                    string band = command.Band ?? "";

                    if (!string.IsNullOrEmpty(band))
                    {
                        lastBand = band;
                    }
                }
            }
            catch (Exception retExc)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"Cannot parse payload: {retExc.Message}");
            }
        }

        public override void DialRotate(DialRotatePayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: DialRotate called with ticks {payload.Ticks}");
            var increment = "";

            if (base.Settings.FrequencyIncrement > 0)
            {
                increment = (((double)base.Settings.FrequencyIncrement) / 1000000.0).ToString();
            }

            var receiverCommand = new ReceiverCommand
            {
                Command = "frequency",
                Action = payload.Ticks > 0 ? "+" : "-",
                SubReceiver = base.Settings.SubRx > 0 ? "true" : "false",
                Value = increment
            };
            string command = System.Text.Json.JsonSerializer.Serialize(receiverCommand);
            string topic = $"receivers/command/{base.Settings.RxIndex}";
            MQTT_Client.PublishMessageAsync(topic, command).Wait();
        }

        public override void DialUp(DialPayload payload)
        {
            if (!string.IsNullOrEmpty(lastBand))
            {
                var receiverCommand = new ReceiverStatus
                {
                    Band = lastBand
                };
                string command = System.Text.Json.JsonSerializer.Serialize(receiverCommand);
                string topic = $"receivers/set/{base.Settings.RxIndex}";
                MQTT_Client.PublishMessageAsync(topic, command).Wait();
            }
        }

        public override void SettingsUpdated()
        {
            base.SettingsUpdated();
            Connection.SetImageAsync(StreamDock.UpdateKeyImage($"RX {base.Settings.RxIndex}\nFrequency")).Wait();
        }
    }

    // Name: Change Volume
    // Tooltip: Change volume on a receiver
    // Controllers: Knob
    // PropertyInspector: ./property_inspector/pi-rx-sub-volume.html
    [PluginActionId("it.iu2frl.streamdock.olliter.changevolume")]
    public class ChangeVolume(ISDConnection connection, InitialPayload payload) : BaseDialMqttItem(connection, payload)
    {
        private int lastVolume = -1;
        private bool muted = false;

        public override void MQTT_StatusReceived(int receiverNumber, ReceiverStatus command)
        {
            try
            {
                if (receiverNumber == base.Settings.RxIndex)
                {

                    int volume = 0;

                    if (base.Settings.SubRx == 0)
                    {
                        volume = Convert.ToInt32(command.ReceiverA.Volume);
                    }
                    else
                    {
                        volume = Convert.ToInt32(command.ReceiverB.Volume);
                    }

                    if (volume > 0)
                    {
                        lastVolume = volume;
                        muted = false;
                    }

                }
            }
            catch (Exception retExc)
            {
                //Logger.Instance.LogMessage(TracingLevel.WARN, $"Cannot parse payload: {retExc.Message}");
            }
        }

        public override void DialRotate(DialRotatePayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: DialRotate called with ticks {payload.Ticks}");

            var increment = "15";
            muted = false;

            if (base.Settings.VolumeIncrement > 0)
            {
                increment = base.Settings.VolumeIncrement.ToString();
            }

            var receiverCommand = new ReceiverCommand
            {
                Command = "volume",
                Action = payload.Ticks > 0 ? "+" : "-",
                SubReceiver = base.Settings.SubRx > 0 ? "true" : "false",
                Value = increment
            };
            string command = System.Text.Json.JsonSerializer.Serialize(receiverCommand);
            string topic = $"receivers/command/{base.Settings.RxIndex}";
            MQTT_Client.PublishMessageAsync(topic, command).Wait();
        }

        public override void DialUp(DialPayload payload)
        {
            if (muted && lastVolume > 0)
            {
                var receiverCommand = new ReceiverCommand
                {
                    Command = "volume",
                    Action = "",
                    SubReceiver = base.Settings.SubRx > 0 ? "true" : "false",
                    Value = lastVolume.ToString()
                };
                string command = System.Text.Json.JsonSerializer.Serialize(receiverCommand);
                string topic = $"receivers/command/{base.Settings.RxIndex}";
                MQTT_Client.PublishMessageAsync(topic, command).Wait();
                muted = false;
            }
            else
            {
                var receiverCommand = new ReceiverCommand
                {
                    Command = "volume",
                    Action = "",
                    SubReceiver = base.Settings.SubRx > 0 ? "true" : "false",
                    Value = "0"
                };
                string command = System.Text.Json.JsonSerializer.Serialize(receiverCommand);
                string topic = $"receivers/command/{base.Settings.RxIndex}";
                MQTT_Client.PublishMessageAsync(topic, command).Wait();
                muted = true;
            }
        }

        public override void SettingsUpdated()
        {
            base.SettingsUpdated();
            Connection.SetImageAsync(StreamDock.UpdateKeyImage($"RX {base.Settings.RxIndex}\nVolume")).Wait();
        }
    }

    #endregion

    // Name: Launch OL-SDR Console
    // Tooltip: Launch OL-SDR Console software if not already running
    // Controllers: Keypad
    // Icon: ./images/Olliter
    [PluginActionId("it.iu2frl.streamdock.olliter.launcholsdr")]
    public class LaunchOLSDR : KeypadBase
    {
        public LaunchOLSDR(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            UpdateKey();
        }

        public override void KeyPressed(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, "Launching OL-Master software");

            try
            {
                // Check if the application is already running
                if (!isProcessRunning("OL-Master"))
                {
                    // If the application is not running, start a new instance
                    Process.Start("\"C:\\Program Files\\OL-Master\\OL-Master.exe\"");
                    Logger.Instance.LogMessage(TracingLevel.INFO, "OL-Master started.");
                }
                else
                {
                    Logger.Instance.LogMessage(TracingLevel.INFO, "OL-Master is already running.");
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"Error launching OL-Master: {ex.Message}");
            }
        }

        public override void KeyReleased(KeyPayload payload) { }

        public override void Dispose() { }

        public override void OnTick()
        {
            UpdateKey();
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload) { }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        #region Custom events
        private void UpdateKey()
        {
            if (isProcessRunning("OL-Master"))
            {
                Connection.SetImageAsync(StreamDock.UpdateKeyImage($"OL-Master\nRunning")).Wait();
            }
            else
            {
                Connection.SetImageAsync(StreamDock.UpdateKeyImage($"Start\nOL-Master")).Wait();
            }
        }

        private bool isProcessRunning(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            return processes.Length > 0;
        }
        #endregion
    }
}

namespace StreamDock.Plugins.PluginAction
{
    // Placeholder
}
