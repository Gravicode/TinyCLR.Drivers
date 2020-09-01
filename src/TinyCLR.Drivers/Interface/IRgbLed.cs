using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace TinyCLR.Drivers.Interface
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
