using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
using System;

namespace Meadow.TinyCLR.Sensors.Atmospheric
{
    internal class Bme280Spi : Bme280Comms
    {
        private SpiDevice _spi;
        private GpioPin _chipSelect;

        internal Bme280Spi(SpiDevice spi, GpioPin chipSelect = null)
        {
            _spi = spi;
            _chipSelect = chipSelect;
        }

        public override byte[] ReadRegisters(byte startRegister, int readCount)
        {
            // the buffer needs to be big enough for the output and response
            var buffer = new byte[readCount + 1];
            var bufferTx = new byte[readCount + 1];
            buffer[0] = startRegister;

            //  var rx = _spi.ExchangeData(_chipSelect, buffer);
            var rx = new byte[readCount + 1];
            _spi.Read(rx);

            // skip past the byte where we clocked out the register address
            var reg = new byte[readCount];
            for(int i = 1; i < readCount; i++)
            {
                reg[i - 1] = rx[i];
            }
            var registerData = reg;//rx.Skip(1).Take(readCount).ToArray();

            return registerData;
        }

        public override void WriteRegister(Register register, byte value)
        {
            _spi.Write(new byte[] { (byte)register, value });
        }
    }
}