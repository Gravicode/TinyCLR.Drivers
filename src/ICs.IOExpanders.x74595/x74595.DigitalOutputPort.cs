using System;
//using Meadow.Hardware;
using Meadow.TinyCLR.Core;

namespace Meadow.TinyCLR.ICs.IOExpanders
{
    public partial class x74595
    {
        public class DigitalOutputPort //: DigitalOutputPortBase
        {
            public bool disposed { get; set; }
            readonly x74595 _x74595;
            public int Pin { get; set; }
            public bool State
            {
                get => state;
                set
                {
                    _x74595.WriteToPin(Pin, value);
                }
            }
            protected bool state;

            public DigitalOutputPort(
                x74595 x74595,
                int pin,
                bool initialState, 
                OutputType outputType)
                //: base(pin, (IDigitalChannelInfo)pin.SupportedChannels[0], initialState, outputType)
            {
                this.Pin = pin;

                _x74595 = x74595;
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        state = false;
                    }
                    disposed = true;
                }
            }

            // Finalizer
            ~DigitalOutputPort()
            {
                Dispose(false);
            }
        }
    }
}