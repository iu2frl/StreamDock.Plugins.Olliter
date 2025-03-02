#region Using directives
using System.Drawing;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BarRaider.SdTools;
using BarRaider.SdTools.Payloads;
using BarRaider.SdTools.Wrappers;
using Common;
using Coordinates;
using MQTTnet;
using MQTTnet.Client;
#endregion

#region Streamdeck Commands
namespace ToggleReceivers
{
    #region Main receivers
    // Name: Toggle Receiver 1
    // Tooltip: Toggle main power to RX 1
    // Controllers: Keypad
    [PluginActionId("it.iu2frl.streamdock.olliter.togglerx1")]

    public class ToggleRx1 : OlliterBaseCommands.ToggleRx
    {
        public ToggleRx1(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 1;
            Logger.Instance.LogMessage(TracingLevel.INFO, "ToggleRx1 created");
        }
    }

    // Name: Toggle Receiver 2
    // Tooltip: Toggle main power to RX 2
    // Controllers: Keypad
    [PluginActionId("it.iu2frl.streamdock.olliter.togglerx2")]
    public class ToggleRx2 : OlliterBaseCommands.ToggleRx
    {
        public ToggleRx2(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 2;
            Logger.Instance.LogMessage(TracingLevel.INFO, "ToggleRx2 created");
        }
    }

    // Name: Toggle Receiver 3
    // Tooltip: Toggle main power to RX 3
    // Controllers: Keypad
    [PluginActionId("it.iu2frl.streamdock.olliter.togglerx3")]
    public class ToggleRx3 : OlliterBaseCommands.ToggleRx
    {
        public ToggleRx3(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 3;
            Logger.Instance.LogMessage(TracingLevel.INFO, "ToggleRx3 created");
        }
    }

    // Name: Toggle Receiver 4
    // Tooltip: Toggle main power to RX 4
    // Controllers: Keypad
    [PluginActionId("it.iu2frl.streamdock.olliter.togglerx4")]
    public class ToggleRx4 : OlliterBaseCommands.ToggleRx
    {
        public ToggleRx4(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 4;
            Logger.Instance.LogMessage(TracingLevel.INFO, "ToggleRx4 created");
        }
    }
    #endregion

    #region Sub receivers
    // Name: Toggle Receiver 1 Sub
    // Tooltip: Toggle power to RX 1 Sub
    // Controllers: Keypad
    [PluginActionId("it.iu2frl.streamdock.olliter.togglerx1sub")]

    public class ToggleRx1sub : OlliterBaseCommands.ToggleRx
    {
        public ToggleRx1sub(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 1;
            base.SubReceiver = true;
            Logger.Instance.LogMessage(TracingLevel.INFO, "ToggleRx1sub created");
        }
    }

    // Name: Toggle Receiver 2 Sub
    // Tooltip: Toggle power to RX 2 Sub
    // Controllers: Keypad
    [PluginActionId("it.iu2frl.streamdock.olliter.togglerx2sub")]
    public class ToggleRx2sub : OlliterBaseCommands.ToggleRx
    {
        public ToggleRx2sub(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 2;
            base.SubReceiver = true;
            Logger.Instance.LogMessage(TracingLevel.INFO, "ToggleRx2sub created");
        }
    }

    // Name: Toggle Receiver 3 Sub
    // Tooltip: Toggle power to RX 3 Sub
    // Controllers: Keypad
    [PluginActionId("it.iu2frl.streamdock.olliter.togglerx3sub")]
    public class ToggleRx3sub : OlliterBaseCommands.ToggleRx
    {
        public ToggleRx3sub(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 3;
            base.SubReceiver = true;
            Logger.Instance.LogMessage(TracingLevel.INFO, "ToggleRx3sub created");
        }
    }

    // Name: Toggle Receiver 4 Sub
    // Tooltip: Toggle power to RX 4 Sub
    // Controllers: Keypad
    [PluginActionId("it.iu2frl.streamdock.olliter.togglerx4sub")]
    public class ToggleRx4sub : OlliterBaseCommands.ToggleRx
    {
        public ToggleRx4sub(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 4;
            base.SubReceiver = true;
            Logger.Instance.LogMessage(TracingLevel.INFO, "ToggleRx4sub created");
        }
    }
    #endregion
}

namespace ToggleMox
{
    #region Main receivers
    // Name: MOX Receiver 1
    // Tooltip: Toggle receive/transmit RX 1
    // Controllers: Keypad
    [PluginActionId("it.iu2frl.streamdock.olliter.togglemox1")]
    public class ToggleMox1 : OlliterBaseCommands.ToggleMox
    {
        public ToggleMox1(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 1;
            Logger.Instance.LogMessage(TracingLevel.INFO, "ToggleMox1 created");
        }
    }

    // Name: MOX Receiver 2
    // Tooltip: Toggle receive/transmit RX 2
    // Controllers: Keypad
    [PluginActionId("it.iu2frl.streamdock.olliter.togglemox2")]
    public class ToggleMox2 : OlliterBaseCommands.ToggleMox
    {
        public ToggleMox2(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 2;
            Logger.Instance.LogMessage(TracingLevel.INFO, "ToggleMox2 created");
        }
    }

    // Name: MOX Receiver 3
    // Tooltip: Toggle receive/transmit RX 3
    // Controllers: Keypad
    [PluginActionId("it.iu2frl.streamdock.olliter.togglemox3")]
    public class ToggleMox3 : OlliterBaseCommands.ToggleMox
    {
        public ToggleMox3(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 3;
            Logger.Instance.LogMessage(TracingLevel.INFO, "ToggleMox3 created");
        }
    }

    // Name: MOX Receiver 4
    // Tooltip: Toggle receive/transmit RX 4
    // Controllers: Keypad
    [PluginActionId("it.iu2frl.streamdock.olliter.togglemox4")]
    public class ToggleMox4 : OlliterBaseCommands.ToggleMox
    {
        public ToggleMox4(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 4;
            Logger.Instance.LogMessage(TracingLevel.INFO, "ToggleMox4 created");
        }
    }
    #endregion

    #region Sub receivers
    // Name: MOX Receiver 1 Sub
    // Tooltip: Toggle receive/transmit RX 1 Sub
    // Controllers: Keypad
    [PluginActionId("it.iu2frl.streamdock.olliter.togglemox1sub")]
    public class ToggleMox1sub : OlliterBaseCommands.ToggleMox
    {
        public ToggleMox1sub(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 1;
            base.SubReceiver = true;
            Logger.Instance.LogMessage(TracingLevel.INFO, "ToggleMox1sub created");
        }
    }

    // Name: MOX Receiver 2 Sub
    // Tooltip: Toggle receive/transmit RX 2 Sub
    // Controllers: Keypad
    [PluginActionId("it.iu2frl.streamdock.olliter.togglemox2sub")]
    public class ToggleMox2sub : OlliterBaseCommands.ToggleMox
    {
        public ToggleMox2sub(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 2;
            base.SubReceiver = true;
            Logger.Instance.LogMessage(TracingLevel.INFO, "ToggleMox2sub created");
        }
    }

    // Name: MOX Receiver 3 Sub
    // Tooltip: Toggle receive/transmit RX 3 Sub
    // Controllers: Keypad
    [PluginActionId("it.iu2frl.streamdock.olliter.togglemox3sub")]
    public class ToggleMox3sub : OlliterBaseCommands.ToggleMox
    {
        public ToggleMox3sub(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 3;
            base.SubReceiver = true;
            Logger.Instance.LogMessage(TracingLevel.INFO, "ToggleMox3sub created");
        }
    }

    // Name: MOX Receiver 4 Sub
    // Tooltip: Toggle receive/transmit RX 4 Sub
    // Controllers: Keypad
    [PluginActionId("it.iu2frl.streamdock.olliter.togglemox4sub")]
    public class ToggleMox4sub : OlliterBaseCommands.ToggleMox
    {
        public ToggleMox4sub(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 4;
            base.SubReceiver = true;
            Logger.Instance.LogMessage(TracingLevel.INFO, "ToggleMox4sub created");
        }
    }
    #endregion
}

namespace TuneRx
{
    #region Main receivers
    // Name: Tune Receiver 1
    // Tooltip: Frequency knob for RX 1
    // Controllers: Knob
    [PluginActionId("it.iu2frl.streamdock.olliter.tunerx1")]
    public class TuneRx1 : OlliterBaseCommands.TuneRx
    {
        public TuneRx1(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 1;
            Logger.Instance.LogMessage(TracingLevel.INFO, "TuneRx1 created");
        }
    }

    // Name: Tune Receiver 2
    // Tooltip: Frequency knob for RX 2
    // Controllers: Knob
    [PluginActionId("it.iu2frl.streamdock.olliter.tunerx2")]
    public class TuneRx2 : OlliterBaseCommands.TuneRx
    {
        public TuneRx2(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 2;
            Logger.Instance.LogMessage(TracingLevel.INFO, "TuneRx2 created");
        }
    }

    // Name: Tune Receiver 3
    // Tooltip: Frequency knob for RX 3
    // Controllers: Knob
    [PluginActionId("it.iu2frl.streamdock.olliter.tunerx3")]
    public class TuneRx3 : OlliterBaseCommands.TuneRx
    {
        public TuneRx3(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 3;
            Logger.Instance.LogMessage(TracingLevel.INFO, "TuneRx3 created");
        }
    }

    // Name: Tune Receiver 4
    // Tooltip: Frequency knob for RX 4
    // Controllers: Knob
    [PluginActionId("it.iu2frl.streamdock.olliter.tunerx4")]
    public class TuneRx4 : OlliterBaseCommands.TuneRx
    {
        public TuneRx4(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 4;
            Logger.Instance.LogMessage(TracingLevel.INFO, "TuneRx4 created");
        }
    }
    #endregion

    #region Sub receivers
    // Name: Tune Receiver 1 Sub
    // Tooltip: Frequency knob for RX 1 Sub
    // Controllers: Knob
    [PluginActionId("it.iu2frl.streamdock.olliter.tunerx1sub")]
    public class TuneRx1sub : OlliterBaseCommands.TuneRx
    {
        public TuneRx1sub(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 1;
            base.SubReceiver = true;
            Logger.Instance.LogMessage(TracingLevel.INFO, "TuneRx1sub created");
        }
    }

    // Name: Tune Receiver 2 Sub
    // Tooltip: Frequency knob for RX 2 Sub
    // Controllers: Knob
    [PluginActionId("it.iu2frl.streamdock.olliter.tunerx2sub")]
    public class TuneRx2sub : OlliterBaseCommands.TuneRx
    {
        public TuneRx2sub(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 2;
            base.SubReceiver = true;
            Logger.Instance.LogMessage(TracingLevel.INFO, "TuneRx2sub created");
        }
    }

    // Name: Tune Receiver 3 Sub
    // Tooltip: Frequency knob for RX 3 Sub
    // Controllers: Knob
    [PluginActionId("it.iu2frl.streamdock.olliter.tunerx3sub")]
    public class TuneRx3sub : OlliterBaseCommands.TuneRx
    {
        public TuneRx3sub(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 3;
            base.SubReceiver = true;
            Logger.Instance.LogMessage(TracingLevel.INFO, "TuneRx3sub created");
        }
    }

    // Name: Tune Receiver 4 Sub
    // Tooltip: Frequency knob for RX 4 Sub
    // Controllers: Knob
    [PluginActionId("it.iu2frl.streamdock.olliter.tunerx4sub")]
    public class TuneRx4sub : OlliterBaseCommands.TuneRx
    {
        public TuneRx4sub(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 4;
            base.SubReceiver = true;
            Logger.Instance.LogMessage(TracingLevel.INFO, "TuneRx4sub created");
        }
    }
    #endregion
}

namespace ChangeVolume
{
    #region Main receivers
    // Name: Change Volume RX 1
    // Tooltip: Volume knob for RX 1
    // Controllers: Knob
    [PluginActionId("it.iu2frl.streamdock.olliter.changevolume1")]
    public class ChangeVolume1 : OlliterBaseCommands.ChangeVolume
    {
        public ChangeVolume1(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 1;
            Logger.Instance.LogMessage(TracingLevel.INFO, "ChangeVolume1 created");
        }
    }

    // Name: Change Volume RX 2
    // Tooltip: Volume knob for RX 2
    // Controllers: Knob
    [PluginActionId("it.iu2frl.streamdock.olliter.changevolume2")]
    public class ChangeVolume2 : OlliterBaseCommands.ChangeVolume
    {
        public ChangeVolume2(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 2;
            Logger.Instance.LogMessage(TracingLevel.INFO, "ChangeVolume2 created");
        }
    }

    // Name: Change Volume RX 3
    // Tooltip: Volume knob for RX 3
    // Controllers: Knob
    [PluginActionId("it.iu2frl.streamdock.olliter.changevolume3")]
    public class ChangeVolume3 : OlliterBaseCommands.ChangeVolume
    {
        public ChangeVolume3(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 3;
            Logger.Instance.LogMessage(TracingLevel.INFO, "ChangeVolume3 created");
        }
    }

    // Name: Change Volume RX 4
    // Tooltip: Volume knob for RX 4
    // Controllers: Knob
    [PluginActionId("it.iu2frl.streamdock.olliter.changevolume4")]
    public class ChangeVolume4 : OlliterBaseCommands.ChangeVolume
    {
        public ChangeVolume4(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 4;
            Logger.Instance.LogMessage(TracingLevel.INFO, "ChangeVolume4 created");
        }
    }
    #endregion

    #region Sub receivers
    // Name: Change Volume RX 1 Sub
    // Tooltip: Volume knob for RX 1 Sub
    // Controllers: Knob
    [PluginActionId("it.iu2frl.streamdock.olliter.changevolume1sub")]
    public class ChangeVolume1sub : OlliterBaseCommands.ChangeVolume
    {
        public ChangeVolume1sub(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 1;
            base.SubReceiver = true;
            Logger.Instance.LogMessage(TracingLevel.INFO, "ChangeVolume1sub created");
        }
    }

    // Name: Change Volume RX 2 Sub
    // Tooltip: Volume knob for RX 2 Sub
    // Controllers: Knob
    [PluginActionId("it.iu2frl.streamdock.olliter.changevolume2sub")]
    public class ChangeVolume2sub : OlliterBaseCommands.ChangeVolume
    {
        public ChangeVolume2sub(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 2;
            base.SubReceiver = true;
            Logger.Instance.LogMessage(TracingLevel.INFO, "ChangeVolume2sub created");
        }
    }

    // Name: Change Volume RX 3 Sub
    // Tooltip: Volume knob for RX 3 Sub
    // Controllers: Knob
    [PluginActionId("it.iu2frl.streamdock.olliter.changevolume3sub")]
    public class ChangeVolume3sub : OlliterBaseCommands.ChangeVolume
    {
        public ChangeVolume3sub(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 3;
            base.SubReceiver = true;
            Logger.Instance.LogMessage(TracingLevel.INFO, "ChangeVolume3sub created");
        }
    }

    // Name: Change Volume RX 4 Sub
    // Tooltip: Volume knob for RX 4 Sub
    // Controllers: Knob
    [PluginActionId("it.iu2frl.streamdock.olliter.changevolume4sub")]
    public class ChangeVolume4sub : OlliterBaseCommands.ChangeVolume
    {
        public ChangeVolume4sub(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            base.ReceiverNumber = 4;
            base.SubReceiver = true;
            Logger.Instance.LogMessage(TracingLevel.INFO, "ChangeVolume4sub created");
        }
    }
    #endregion
}
#endregion

#region Base Olliter commands
namespace OlliterBaseCommands
{
    public class ToggleRx(ISDConnection connection, InitialPayload payload) : Common.BaseKeypadMqttItem(connection, payload)
    {
        private int _receiverNumber;
        public bool SubReceiver { get; set; }

        public int ReceiverNumber
        {
            get => _receiverNumber;
            set
            {
                if (_receiverNumber != value)
                {
                    _receiverNumber = value;
                    OnReceiverNumberChanged();
                }
            }
        }

        public override void KeyPressed(KeyPayload payload)
        {
            var receiverCommand = new Common.ReceiverCommand
            {
                Command = "enable",
                Action = "toggle",
                SubReceiver = SubReceiver ? "true" : "false",
                Value = ""
            };
            string command = JsonSerializer.Serialize(receiverCommand);
            string topic = $"receivers/command/{_receiverNumber}";
            Common.MQTT_Client.PublishMessageAsync(topic, command).Wait();
            Logger.Instance.LogMessage(TracingLevel.INFO, "KeyPressed called with: ");
        }

        private async void Connection_OnTitleParametersDidChange(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.TitleParametersDidChange> e)
        {
            await OnReceiverNumberChanged();
        }

        public override void MQTT_StatusReceived(int receiverNumber, ReceiverStatus command)
        {
            try
            {
                if (receiverNumber == _receiverNumber)
                {
                    if (command.ReceiverA.Enabled == "True")
                    {
                        Connection.SetImageAsync(Common.StreamDock.UpdateKeyImage($"RX{_receiverNumber}\nEnabled")).Wait();
                    }
                    else
                    {
                        Connection.SetImageAsync(Common.StreamDock.UpdateKeyImage($"RX{_receiverNumber}\nDisabled")).Wait();
                    }
                }
            }
            catch (Exception retExc)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"Cannot parse payload: {retExc.Message}");
            }
        }

        private async Task OnReceiverNumberChanged()
        {
            await Connection.SetImageAsync(Common.StreamDock.UpdateKeyImage($"RX {_receiverNumber}"));
        }
    }

    public class ToggleMox(ISDConnection connection, InitialPayload payload) : Common.BaseKeypadMqttItem(connection, payload)
    {
        private int _receiverNumber;
        public bool SubReceiver { get; set; }
        public int ReceiverNumber
        {
            get => _receiverNumber;
            set
            {
                if (_receiverNumber != value)
                {
                    _receiverNumber = value;
                    OnReceiverNumberChanged();
                }
            }
        }
        public override void KeyPressed(KeyPayload payload)
        {
            var receiverCommand = new Common.ReceiverCommand
            {
                Command = "mox",
                Action = "toggle",
                SubReceiver = SubReceiver ? "true" : "false",
                Value = ""
            };
            string command = JsonSerializer.Serialize(receiverCommand);
            string topic = $"receivers/command/{_receiverNumber}";
            Common.MQTT_Client.PublishMessageAsync(topic, command).Wait();
            Logger.Instance.LogMessage(TracingLevel.INFO, "KeyPressed called with: ");
        }
        private async void Connection_OnTitleParametersDidChange(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.TitleParametersDidChange> e)
        {
            await OnReceiverNumberChanged();
        }
        public override void MQTT_StatusReceived(int receiverNumber, ReceiverStatus command)
        {
            try
            {
                if (receiverNumber == _receiverNumber)
                {
                    if (command.ReceiverA.TxVfo == "True" && command.ReceiverA.Mox == "True")
                    {
                        Connection.SetImageAsync(Common.StreamDock.UpdateKeyImage($"RX{_receiverNumber}\nTransmit")).Wait();
                    }
                    else
                    {
                        Connection.SetImageAsync(Common.StreamDock.UpdateKeyImage($"RX{_receiverNumber}\nReceive")).Wait();
                    }
                }
            }
            catch (Exception retExc)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"Cannot parse payload: {retExc.Message}");
            }
        }
        private async Task OnReceiverNumberChanged()
        {
            await Connection.SetImageAsync(Common.StreamDock.UpdateKeyImage($"MOX {_receiverNumber}"));
        }
    }

    public class TuneRx(ISDConnection connection, InitialPayload payload) : Common.BaseDialMqttItem(connection, payload)
    {
        private int _receiverNumber;
        public bool SubReceiver { get; set; }
        public int ReceiverNumber
        {
            get => _receiverNumber;
            set
            {
                if (_receiverNumber != value)
                {
                    _receiverNumber = value;
                    //OnReceiverNumberChanged();
                }
            }
        }

        public override void DialRotate(DialRotatePayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: DialRotate called with ticks {payload.Ticks}");
            if (payload.Ticks > 0)
            {
                var receiverCommand = new Common.ReceiverCommand
                {
                    Command = "frequency",
                    Action = "+",
                    SubReceiver = SubReceiver ? "true" : "false",
                    Value = ""                    
                };
                string command = JsonSerializer.Serialize(receiverCommand);
                string topic = $"receivers/command/{_receiverNumber}";
                Common.MQTT_Client.PublishMessageAsync(topic, command).Wait();
            }
            else
            {
                var receiverCommand = new Common.ReceiverCommand
                {
                    Command = "frequency",
                    Action = "-",
                    SubReceiver = SubReceiver ? "true" : "false",
                    Value = ""
                };
                string command = JsonSerializer.Serialize(receiverCommand);
                string topic = $"receivers/command/{_receiverNumber}";
                Common.MQTT_Client.PublishMessageAsync(topic, command).Wait();
            }
        }
    }

    public class ChangeVolume(ISDConnection connection, InitialPayload payload) : Common.BaseDialMqttItem(connection, payload)
    {
        private int _receiverNumber;
        public bool SubReceiver { get; set; }
        public int ReceiverNumber
        {
            get => _receiverNumber;
            set
            {
                if (_receiverNumber != value)
                {
                    _receiverNumber = value;
                    //OnReceiverNumberChanged();
                }
            }
        }
        public override void DialRotate(DialRotatePayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.DEBUG, $"{GetType().Name}: DialRotate called with ticks {payload.Ticks}");

            var receiverCommand = new Common.ReceiverCommand
            {
                Command = "volume",
                Action = payload.Ticks > 0 ? "+" : "-",
                SubReceiver = SubReceiver ? "true" : "false",
                Value = "10"
            };
            string command = JsonSerializer.Serialize(receiverCommand);
            string topic = $"receivers/command/{_receiverNumber}";
            Common.MQTT_Client.PublishMessageAsync(topic, command).Wait();
        }
    }

    public class ChangeBand(ISDConnection connection, InitialPayload payload) : Common.BaseKeypadMqttItem(connection, payload)
    {
        private int _receiverNumber;
        private string _band = "B20M";
        public bool SubReceiver { get; set; }
        public int ReceiverNumber
        {
            get => _receiverNumber;
            set
            {
                if (_receiverNumber != value)
                {
                    _receiverNumber = value;
                    OnParametersChanged();
                }
            }
        }
        public string Band
        {
            get => _band;
            set
            {
                if (_band != value)
                {
                    _band = value;
                    OnParametersChanged();
                }
            }
        }
        public override void KeyPressed(KeyPayload payload)
        {
            var receiverCommand = new Common.ReceiverStatus
            {
                Band = _band
            };
            string command = JsonSerializer.Serialize(receiverCommand);
            string topic = $"receivers/set/{_receiverNumber}";
            Common.MQTT_Client.PublishMessageAsync(topic, command).Wait();
            Logger.Instance.LogMessage(TracingLevel.DEBUG, "KeyPressed called with: ");
        }

        private async void Connection_OnTitleParametersDidChange(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.TitleParametersDidChange> e)
        {
            await OnParametersChanged();
        }

        public override void MQTT_StatusReceived(int receiverNumber, ReceiverStatus command)
        {
            try
            {
                if (receiverNumber == _receiverNumber)
                {
                    if (command.ReceiverA.Band == "True")
                    {
                        Connection.SetImageAsync(Common.StreamDock.UpdateKeyImage($"RX{_receiverNumber}\nBand A")).Wait();
                    }
                    else
                    {
                        Connection.SetImageAsync(Common.StreamDock.UpdateKeyImage($"RX{_receiverNumber}\nBand B")).Wait();
                    }
                }
            }
            catch (Exception retExc)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"Cannot parse payload: {retExc.Message}");
            }
        }
        
        private async Task OnParametersChanged()
        {
            await Connection.SetImageAsync(Common.StreamDock.UpdateKeyImage($"RX {_receiverNumber}\nBand {GetBandValue()}"));
        }

        private string GetBandValue()
        {
            return _band.Replace("B", "").ToLower();
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
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(10);
        private static int retryAttempts = 0;

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

                await mqttClient.PublishAsync(message);

                Logger.Instance.LogMessage(TracingLevel.DEBUG, $"Published message: Topic={topic}, Payload={payload}");
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
        public ReceiverStatusDetail ReceiverA { get; set; } = new ReceiverStatusDetail();

        [JsonPropertyName("receiver_b")]
        public ReceiverStatusDetail ReceiverB { get; set; } = new ReceiverStatusDetail();
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
            MQTT_Client.ConnectToBroker(MQTT_Config.Host, MQTT_Config.Port, MQTT_Config.User, MQTT_Config.Password, MQTT_Config.UseAuthentication, MQTT_Config.UseWebSocket);
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
            //else
            //{
            //    Connection.SetImageAsync(Common.StreamDock.UpdateKeyImage("Connected")).Wait();
            //}
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
            MQTT_Client.ConnectToBroker(MQTT_Config.Host, MQTT_Config.Port, MQTT_Config.User, MQTT_Config.Password, MQTT_Config.UseAuthentication, MQTT_Config.UseWebSocket);
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
namespace Debug.Keypad
{
    // Name: Keypad Debug
    // Tooltip: This function only prints to the log
    // Controllers: Keypad
    [PluginActionId("it.iu2frl.streamdock.keypaddebug")]
    public class KeypadDebug : KeypadBase
    {
        public KeypadDebug(ISDConnection connection, InitialPayload payload) : base(connection, payload)
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
            await Connection.SetImageAsync(Common.StreamDock.UpdateKeyImage($"[{e.Event.Payload.Coordinates.Row}, {e.Event.Payload.Coordinates.Column}]"));
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

namespace Debug.Dial
{
    // Name: Dial Debug
    // Tooltip: This function only prints to the log
    // Controllers: Knob
    [PluginActionId("it.iu2frl.streamdock.dialdebug")]
    public class DialDebug : EncoderBase
    {
        public DialDebug(ISDConnection connection, InitialPayload payload) : base(connection, payload)
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
            Connection.SetImageAsync(Common.StreamDock.UpdateKeyImage($"[{e.Event.Payload.Coordinates.Row}, {e.Event.Payload.Coordinates.Column}]")).Wait();
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

        public override void DialRotate(DialRotatePayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "DialRotate called");
        }

        public override void DialDown(DialPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "DialDown called");
        }

        public override void DialUp(DialPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "DialUp called");
        }

        public override void TouchPress(TouchpadPressPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "TouchPress called");
        }
    }
}
#endregion

namespace StreamDock.Plugins.PluginAction
{
    // Placeholder
}
