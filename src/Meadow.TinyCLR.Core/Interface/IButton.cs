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

        event EventHandler PressStarted;
        event EventHandler PressEnded;
        event EventHandler Clicked;
    }
}
