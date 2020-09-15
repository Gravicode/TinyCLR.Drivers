using System;
using GHIElectronics.TinyCLR.Devices.I2c;

namespace Meadow.TinyCLR.Sensors.Atmospheric
{
    internal class Bme280I2C : Bme280Comms
    {
        private I2cDevice _i2c;
        private byte _address;

        internal Bme280I2C(I2cDevice i2c, byte busAddress)
        {
            if ((busAddress != 0x76) && (busAddress != 0x77))
            {
                throw new ArgumentOutOfRangeException(nameof(busAddress), "Address should be 0x76 or 0x77");
            }

            _i2c = i2c;
            _address = busAddress;
        }

        public override byte[] ReadRegisters(byte startRegister, int readCount)
        {
            var readByte = new byte[readCount];
            _i2c.WriteRead(new byte[] { _address,  (byte)startRegister },readByte);
            return readByte;
        }

        public override void WriteRegister(Register register, byte value)
        {
            _i2c.Write( new byte[] { _address, (byte)register, value });
        }
    }
}