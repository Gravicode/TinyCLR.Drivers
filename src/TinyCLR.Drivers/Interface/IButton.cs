using System;
using System.Collections;
using System.Text;
using System.Threading;
using TinyCLR.Drivers.System;

namespace TinyCLR.Drivers.Interface
{
    public interface IButton 
    {
        bool State { get; }

        event EventHandler PressStarted;
        event EventHandler PressEnded;
        event EventHandler Clicked;
    }
}
