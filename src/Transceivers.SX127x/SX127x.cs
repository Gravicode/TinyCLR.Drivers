using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
using System;
using System.Diagnostics;

namespace Meadow.Foundation.Transceivers
{
    public class SX127x
    {
        private const byte REG_VERSION = 0x42;
        SpiDevice device;
        public SX127x(string bus, int chipSelect)

        {
            var gpio = GpioController.GetDefault();
           

            var chipSelectPort = gpio.OpenPin(chipSelect);
            chipSelectPort.SetDriveMode(GpioPinDriveMode.Output);
            chipSelectPort.Write(GpioPinValue.Low);

            var settings = new SpiConnectionSettings()
            {
                ChipSelectType = SpiChipSelectType.Gpio,
                ChipSelectLine = chipSelectPort,
                Mode = SpiMode.Mode1,
                ClockFrequency = 4_000_000,
            };

            var controller = SpiController.FromName(bus);
            device = controller.GetDevice(settings);
            // : base(bus, chipSelect)
        }

        public byte GetVersion()
        {
            try
            {
                var read = new byte[1];
                device.TransferFullDuplex(new byte[] { REG_VERSION }, read);
                return read[0];
                //return device.ReadRegister(REG_VERSION);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ReadDeviceID " + ex.Message);
                return 0xff;
            }
        }
    }
}