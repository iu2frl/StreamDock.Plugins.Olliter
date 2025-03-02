#region Using directives
using System.Globalization;
using BarRaider.SdTools;
using BarRaider.SdTools.Payloads;
using BarRaider.SdTools.Wrappers;
#endregion

namespace StreamDock.Plugins.Payload
{
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
                SubReceiver = base.Settings.SubRx ? "true" : "false",
                Value = ""
            };
            string command = System.Text.Json.JsonSerializer.Serialize(receiverCommand);
            string topic = $"receivers/command/{base.Settings.RxIndex}";
            MQTT_Client.PublishMessageAsync(topic, command).Wait();
            Logger.Instance.LogMessage(TracingLevel.INFO, "KeyPressed called with: ");
        }

        private void Connection_OnTitleParametersDidChange(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.TitleParametersDidChange> e)
        {
            SettingsUpdated();
        }

        public override void MQTT_StatusReceived(int receiverNumber, ReceiverStatus command)
        {
            try
            {
                if (receiverNumber == base.Settings.RxIndex)
                {
                    if (base.Settings.SubRx)
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
                SubReceiver = base.Settings.SubRx ? "true" : "false",
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
                    if (base.Settings.SubRx)
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
            Connection.SetImageAsync(StreamDock.UpdateKeyImage($"RX {base.Settings.RxIndex}\nMOX")).Wait();
        }

    }

    // Name: Change Frequency
    // Tooltip: Change frequency on a receiver using a knob
    // Controllers: Knob
    // PropertyInspector: ./property_inspector/pi-rx-sub-frequency.html
    [PluginActionId("it.iu2frl.streamdock.olliter.changefrequency")]
    public class TuneRx(ISDConnection connection, InitialPayload payload) : BaseDialMqttItem(connection, payload)
    {
        public override void DialRotate(DialRotatePayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: DialRotate called with ticks {payload.Ticks}");
            var increment = "";

            if (base.Settings.FrequencyIncrement > 0)
            {
                increment = (base.Settings.FrequencyIncrement / 1000000).ToString();
            }

            var receiverCommand = new ReceiverCommand
            {
                Command = "frequency",
                Action = payload.Ticks > 0 ? "+" : "-",
                SubReceiver = base.Settings.SubRx ? "true" : "false",
                Value = increment
            };
            string command = System.Text.Json.JsonSerializer.Serialize(receiverCommand);
            string topic = $"receivers/command/{base.Settings.RxIndex}";
            MQTT_Client.PublishMessageAsync(topic, command).Wait();
        }

        public override void SettingsUpdated()
        {
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
        public override void DialRotate(DialRotatePayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: DialRotate called with ticks {payload.Ticks}");
            
            var increment = "15";

            if (base.Settings.VolumeIncrement > 0)
            {
                increment = base.Settings.VolumeIncrement.ToString();
            }

            var receiverCommand = new ReceiverCommand
            {
                Command = "volume",
                Action = payload.Ticks > 0 ? "+" : "-",
                SubReceiver = base.Settings.SubRx ? "true" : "false",
                Value = increment
            };
            string command = System.Text.Json.JsonSerializer.Serialize(receiverCommand);
            string topic = $"receivers/command/{base.Settings.RxIndex}";
            MQTT_Client.PublishMessageAsync(topic, command).Wait();
        }
        public override void SettingsUpdated()
        {
            Connection.SetImageAsync(StreamDock.UpdateKeyImage($"RX {base.Settings.RxIndex}\nVolume")).Wait();
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
                        rxLine = $"SET ";
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
                SubReceiver = base.Settings.SubRx ? "true" : "false",
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
                    var receiverFrequency = base.Settings.SubRx ? command.ReceiverA.Frequency : command.ReceiverA.Frequency;
                    var receiverFrequencyValue = Convert.ToDouble(receiverFrequency, CultureInfo.InvariantCulture) * 1000;

                    var rxLine = $"RX{base.Settings.RxIndex} " + (base.Settings.SubRx ? "Sub" : "Main");
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
            Connection.SetImageAsync(StreamDock.UpdateKeyImage($"RX {base.Settings.RxIndex}\nFrequency")).Wait();
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
                SubReceiver = base.Settings.SubRx ? "true" : "false",
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
                        var receiverFrequency = base.Settings.SubRx ? command.ReceiverA.Frequency : command.ReceiverA.Frequency;
                        var receiverFrequencyValue = Convert.ToDouble(receiverFrequency, CultureInfo.InvariantCulture) * 1000;

                        var rxLine = $"RX{base.Settings.RxIndex} " + (base.Settings.SubRx ? "Sub" : "Main");
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
            Connection.SetImageAsync(StreamDock.UpdateKeyImage($"RX {base.Settings.RxIndex}\nFrequency")).Wait();
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
                SubReceiver = base.Settings.SubRx ? "true" : "false",
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
                    var receiverVolume = base.Settings.SubRx ? command.ReceiverA.Volume : command.ReceiverA.Volume;
                    var rxLine = $"RX{base.Settings.RxIndex} " + (base.Settings.SubRx ? "Sub" : "Main");
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
                SubReceiver = base.Settings.SubRx ? "true" : "false",
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
                    var receiverVolume = base.Settings.SubRx ? command.ReceiverA.Volume : command.ReceiverA.Volume;
                    var rxLine = $"RX{base.Settings.RxIndex} " + (base.Settings.SubRx ? "Sub" : "Main");
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
            Connection.SetImageAsync(StreamDock.UpdateKeyImage($"RX {base.Settings.RxIndex}\nVolume")).Wait();
        }
    }
}

namespace StreamDock.Plugins.PluginAction
{
    // Placeholder
}
