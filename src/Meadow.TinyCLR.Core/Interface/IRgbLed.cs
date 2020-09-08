using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace Meadow.TinyCLR.Interface
{
    public interface IRgbLed
    {
        CommonType Common { get; }


    }
    public enum CommonType
    {
        CommonCathode = 0,
        CommonAnode = 1
    }
}
