using System;

namespace Meadow.TinyCLR.Displays.TextDisplayMenu.InputTypes
{
    public class Temperature : NumericBase
    {
        public Temperature() : base(-10, 100, 2)
        {
        }
    }
}
