using GHIElectronics.TinyCLR.Devices.I2c;
using System;

namespace Meadow.TinyCLR.Sensors.Light
{
    /// <summary>
    ///     Driver for the Max44009 light-to-digital converter.
    /// </summary>
    public class Max44009
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public static byte DefaultI2cAddress = 0x4A; //alt is 0x4B

        private static I2cDevice i2cPeripheral;

        public Max44009(string i2cBus, byte address = 0x4a)
        {
            var settings = new I2cConnectionSettings(address, 100_000); //The slave's address and the bus speed.
            var controller = I2cController.FromName(i2cBus);
            i2cPeripheral = controller.GetDevice(settings);
            //i2cPeripheral = new I2cPeripheral(i2cBus, address);

            i2cPeripheral.Write(new byte[] { 0x02, 0x00 });
        }

        public double GetIlluminance()
        {
            var data = new byte[2];
             i2cPeripheral.WriteRead(new byte[] { 0x03 }, data);

            int exponent = ((data[0] & 0xF0) >> 4);
            int mantissa = ((data[0] & 0x0F) >> 4) | (data[1] & 0x0F);

            var luminance = Math.Pow(2, exponent) * mantissa * 0.045;

            return luminance;
        }
    }
}