using System;

namespace Meadow.TinyCLR.Displays
{
    public class Ssd1309 : Ssd1306
    {
        /// <summary>
        ///     Create a new SSD1306 object using the default parameters for
        /// </summary>
        /// <remarks>
        ///     Note that by default, any pixels out of bounds will throw and exception.
        ///     This can be changed by setting the <seealso cref="IgnoreOutOfBoundsPixels" />
        ///     property to true.
        /// </remarks>
        /// <param name="displayType">Type of SSD1306 display (default = 128x64 pixel display).</param>
        ///
        public Ssd1309(string spiBus, int chipSelectPin, int dcPin, int resetPin) :
            base(spiBus, chipSelectPin, dcPin, resetPin, DisplayType.OLED128x64)
        {
        }

        /// <summary>
        ///     Create a new SSD1309 object using the default parameters for
        /// </summary>
        /// <remarks>
        ///     Note that by default, any pixels out of bounds will throw and exception.
        ///     This can be changed by setting the <seealso cref="IgnoreOutOfBoundsPixels" />
        ///     property to true.
        /// </remarks>
        /// <param name="address">Address of the bus on the I2C display.</param>
        public Ssd1309(string i2cBus, byte address = 0x3c) : base(i2cBus, address, DisplayType.OLED128x64)
        {
        }
    }
}