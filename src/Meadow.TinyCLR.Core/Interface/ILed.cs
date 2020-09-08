using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace Meadow.TinyCLR.Interface
{
    public interface ILed
    {
        bool IsOn { get; set; }

        void StartBlink(uint onDuration = 200, uint offDuration = 200);
        void Stop();
    }
}
