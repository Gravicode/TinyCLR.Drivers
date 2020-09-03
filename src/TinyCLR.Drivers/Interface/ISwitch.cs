using System;
using System.Collections;
using System.Text;
using System.Threading;
using TinyCLR.Drivers.System;

namespace TinyCLR.Drivers.Interface
{
    public interface ISwitch 
    {
        bool IsOn { get; }

        event EventHandler Changed;
    }
}
