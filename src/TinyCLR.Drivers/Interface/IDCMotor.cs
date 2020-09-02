using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace TinyCLR.Drivers.Interface
{
    public interface IDCMotor
    {
        float Speed { get; set; }
        bool IsNeutral { get; set; }
    }
}
