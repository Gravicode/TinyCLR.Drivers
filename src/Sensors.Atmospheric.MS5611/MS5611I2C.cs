using GHIElectronics.TinyCLR.Devices.I2c;
using System;
using System.Diagnostics;

namespace Meadow.TinyCLR.Sensors.Atmospheric
{
    internal class Ms5611I2c : Ms5611Base
    {
        private I2cDevice _i2c;
        private byte _address;

        internal Ms5611I2c(I2cDevice i2c, byte address, Ms5611.Resolution resolution)
            : base(resolution)
        {
            _i2c = i2c;
            _address = address;
        }

        public override void Reset()
        {
            var cmd = (byte)Commands.Reset;
            Debug.WriteLine($"Sending {cmd:X2} to {_address:X2}");
            _i2c.Write(new byte[] { _address, cmd });
        }

        public override void BeginTempConversion()
        {
            var cmd = (byte)((byte)Commands.ConvertD2 + 2 * (byte)Resolution);
            Debug.WriteLine($"Sending {cmd:X2} to {_address:X2}");
            _i2c.Write(new byte[] { _address, cmd });
        }

        public override void BeginPressureConversion()
        {
            var cmd = (byte)((byte)Commands.ConvertD1 + 2 * (byte)Resolution);
            Debug.WriteLine($"Sending {cmd:X2} to {_address:X2}");
            _i2c.Write(new byte[] { _address, cmd });
        }

        public override byte[] ReadData()
        {
            // write a
            _i2c.Write(new byte[] { _address, (byte)Commands.ReadADC });
            var data = new byte[3];
            _i2c.WriteRead(new byte[] { _address },data);
            return data;
        }
    }
}
