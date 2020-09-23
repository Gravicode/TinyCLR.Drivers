using System;
using System.Collections;
using System.Text;
using System.Threading;
using Meadow.TinyCLR.System;

namespace Meadow.TinyCLR.Interface
{
    public interface IButton 
    {
        bool State { get; }

        event Meadow.TinyCLR.System.EventHandler PressStarted;
        event Meadow.TinyCLR.System.EventHandler PressEnded;
        event Meadow.TinyCLR.System.EventHandler Clicked;
    }
}
