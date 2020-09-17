using GHIElectronics.TinyCLR.Devices.Spi;
using System;

namespace Meadow.TinyCLR.Sensors.Atmospheric
{
    internal class Ms5611Spi : Ms5611Base
    {
        private SpiDevice _spi;
        private int _chipSelect;

        internal Ms5611Spi(SpiDevice spi, int chipSelect, Ms5611.Resolution resolution)
            : base(resolution)
        {
            _spi = spi;
            _chipSelect = chipSelect;

            throw new NotImplementedException();
        }

        public override void BeginPressureConversion()
        {
            throw new NotImplementedException();
        }

        public override void BeginTempConversion()
        {
            throw new NotImplementedException();
        }

        public override byte[] ReadData()
        {
            throw new NotImplementedException();
        }

        public override void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
