using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace TinyCLR.Drivers.Interface
{
    public interface IPwmChannelInfo 
    {
        uint Timer { get; }
        uint TimerChannel { get; }
        float MinimumFrequency { get; }
        float MaximumFrequency { get; }
    }
    public enum TimeScale
    {
        Seconds = 1,
        Milliseconds = 1000,
        Microseconds = 1000000
    }
    public interface IPwmPort : IDisposable
    {
      
        IPwmChannelInfo Channel { get; }
        float Duration { get; set; }
        float Period { get; set; }
        float DutyCycle { get; set; }
        float Frequency { get; set; }
        bool Inverted { get; set; }
        bool State { get; }
        TimeScale TimeScale { get; set; }

        void Start();
        void Stop();
    }
}
