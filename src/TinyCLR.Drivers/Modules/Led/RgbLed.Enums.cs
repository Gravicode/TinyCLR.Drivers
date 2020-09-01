using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace TinyCLR.Drivers.Modules.Led
{
    /// <summary>
    /// Possible colors on RgbLed
    /// </summary>
    public partial class RgbLed
    {
        public enum Colors
        {
            Red,
            Green,
            Blue,
            Yellow,
            Magenta,
            Cyan,
            White,
            Black,
            count
        }
    }
}
