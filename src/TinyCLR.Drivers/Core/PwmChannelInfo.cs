using System;
using System.Collections;
using System.Text;
using System.Threading;
using TinyCLR.Drivers.Interface;

namespace TinyCLR.Drivers.Core
{
    public class PwmChannelInfo:IPwmChannelInfo
    {
       
        public PwmChannelInfo(string name, uint timer, uint timerChannel, float minimumFrequency = 0, float maximumFrequency = 100000, bool pullDownCapable = false, bool pullUpCapable = false)
        {
            this.MaximumFrequency = maximumFrequency;
            this.MinimumFrequency = minimumFrequency;
            this.Timer = timer;
            this.TimerChannel = timerChannel;
        }

        public float MinimumFrequency { get; protected set; }
        public float MaximumFrequency { get; protected set; }
        public uint Timer { get; protected set; }
        public uint TimerChannel { get; protected set; }
    }
}
