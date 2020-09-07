using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace TinyCLR.Drivers.Modules.Hid
{
    public class JoystickPosition
    {
        public JoystickPosition()
        {

        }
        public JoystickPosition(float horizontalValue, float verticalValue)
        {
            this.HorizontalValue = horizontalValue;
            this.VerticalValue = verticalValue;
        }

        public float HorizontalValue { get; set; }
        public float VerticalValue { get; set; }

      
    
    }
}
