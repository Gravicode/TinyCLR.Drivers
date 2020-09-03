using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace TinyCLR.Drivers.Interface
{
    public interface IToneGenerator
    {
        void PlayTone(float frequency, int duration = 0);
        void StopTone();
    }
}
