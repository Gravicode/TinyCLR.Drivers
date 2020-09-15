using System;
using System.Diagnostics;
using GHIElectronics.TinyCLR.Devices.Gpio;
using Meadow.TinyCLR.Core;
using Meadow.Utilities;

namespace Meadow.TinyCLR.ICs.IOExpanders
{
    public delegate void PinChangedEventHandler(object sender, DigitalInputPortEventArgs e);
    public partial class Mcp23x08
    {
        public class DigitalInputPort 
        {
            Mcp23x08 _mcp;
            public int Pin { get; protected set; }
            private readonly int _pin;
            private DateTime _lastChangeTime;
            private bool _lastState;

            public bool State
            {
                get
                {
                    return _mcp.ReadPort(this.Pin);
                }
            }
            public GpioPinEdge InterruptMode { get; set; }
            public PinChangedEventHandler PinEventChanged { get; set; }

            void OnChanged(DigitalInputPortEventArgs e)
            {
                PinEventChanged?.Invoke(this, e);
            }

            public DigitalInputPort(
                Mcp23x08 mcpController,
                int pin,
                GpioPinEdge interruptMode =  GpioPinEdge.FallingEdge)
                //: base(pin, (IDigitalChannelInfo)pin.SupportedChannels[0], interruptMode)
            {
                Pin = pin;
                _mcp = mcpController;
                _pin = pin;
                InterruptMode = interruptMode;
                //if (interruptMode != InterruptMode.None)
                {
                    _mcp.InputChanged += PinChanged;
                }
            }

            internal void PinChanged(object sender, IOExpanderInputChangedEventArgs e)
            {
                try
                {
                    var now = DateTime.UtcNow;
                    var isInterrupt = BitHelpers.GetBitValue(e.InterruptPins, (byte)_pin);
                    if (!isInterrupt)
                    {
                        return;
                    }

                    var currentState = BitHelpers.GetBitValue(e.InputState, (byte)_pin);
                    if (currentState != _lastState)
                    {
                        switch (InterruptMode)
                        {
                            case  GpioPinEdge.FallingEdge:
                                if (currentState)
                                    OnChanged(
                                        new DigitalInputPortEventArgs(false, now, _lastChangeTime));
                                break;
                            case  GpioPinEdge.RisingEdge:
                                if (currentState)
                                    OnChanged(
                                        new DigitalInputPortEventArgs(true, now, _lastChangeTime));
                                break;
                            /*
                            case InterruptMode.EdgeBoth:
                                RaiseChangedAndNotify(
                                    new DigitalInputPortEventArgs(currentState, now,
                                        _lastChangeTime));
                                break;
                            case InterruptMode.None:
                                break;
                            */
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    _lastState = currentState;
                    _lastChangeTime = now;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }


            
        }
    }
}
