using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace Meadow.TinyCLR.Interface
{
    public interface IDCMotor
    {
        float Speed { get; set; }
        bool IsNeutral { get; set; }
    }
}
