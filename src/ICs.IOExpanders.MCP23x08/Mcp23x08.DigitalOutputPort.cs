﻿using Meadow.TinyCLR.Core;
using System;

namespace Meadow.TinyCLR.ICs.IOExpanders
{
    public partial class Mcp23x08
    {
        public class DigitalOutputPort //: DigitalOutputPortBase
        {
            public bool disposed { get; set; }
            Mcp23x08 _mcp;
            public int Pin { get; set; }
            public bool State {
                get => this.state;
                set {
                    _mcp.WriteToPort(this.Pin, value);
                }
            } protected bool state;

            public DigitalOutputPort(
                Mcp23x08 mcpController,
                int pin,
                bool initialState = false,
                OutputType outputType = OutputType.OpenDrain)
                
            {
                //: base(pin, (IDigitalChannelInfo)pin.SupportedChannels[0], initialState, outputType)
                this.Pin = pin;
                
                _mcp = mcpController;
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                // TODO: we should consider moving this logic to the finalizer
                // but the problem with that is that we don't know when it'll be called
                // but if we do it in here, we may need to check the _disposed field
                // elsewhere

                if (!disposed) {
                    if (disposing) {
                        this.state = false;
                        _mcp.ResetPin(this.Pin);
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
