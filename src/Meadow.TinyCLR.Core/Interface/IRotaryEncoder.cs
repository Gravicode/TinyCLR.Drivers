using GHIElectronics.TinyCLR.Devices.Gpio;
using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace Meadow.TinyCLR.Interface
{
    public enum RotationDirection
    {
        Clockwise = 0,
        CounterClockwise = 1
    }
    public class RotaryTurnedEventArgs
    {
        public RotaryTurnedEventArgs(RotationDirection direction)
        {
            Direction = direction;
        }

        public RotationDirection Direction { get; set; }
    }
    public delegate void RotaryTurnedEventHandler(object sender, RotaryTurnedEventArgs e);
    public interface IRotaryEncoder
    {
        GpioPin APhasePort { get; }
        GpioPin BPhasePort { get; }

        event RotaryTurnedEventHandler Rotated;
    }
}
