using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace Meadow.TinyCLR.Core
{
    public class DigitalInputPortEventArgs : EventArgs
    {
      
        public DigitalInputPortEventArgs(bool value, DateTime time, DateTime previous)
        {
            this.Value = value;
            this.New = time;
            this.Old = previous;
            Delta = this.New - Old;
        }

        public bool Value { get; set; }
        public DateTime New { get; set; }
        public DateTime Old { get; set; }
        public TimeSpan Delta { get; }
    }
}
