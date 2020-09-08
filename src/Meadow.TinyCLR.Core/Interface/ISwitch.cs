using System;
using System.Collections;
using System.Text;
using System.Threading;
using Meadow.TinyCLR.System;

namespace Meadow.TinyCLR.Interface
{
    public interface ISwitch 
    {
        bool IsOn { get; }

        event EventHandler Changed;
    }
}
