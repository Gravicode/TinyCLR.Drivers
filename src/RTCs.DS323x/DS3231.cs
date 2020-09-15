//using Meadow.Hardware;

using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;

namespace Meadow.TinyCLR.RTCs
{
    /// <summary>
    /// Create a new DS3231 Real Time Clock object.
    /// </summary>
    public class Ds3231 : Ds323x
    {
        #region Constructors

        /// <summary>
        /// Create a new MAG3110 object using the default parameters for the component.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="interruptPin">Digital pin connected to the alarm interrupt pin on the RTC.</param>
        public Ds3231( string i2cBus, int interruptPin, byte address = 0x68, ushort speed = 100)
        {
            var settings = new I2cConnectionSettings(address, 100_000); //The slave's address and the bus speed.
            var controller = I2cController.FromName(i2cBus);
            _ds323x = controller.GetDevice(settings);

            //_ds323x = new I2cPeripheral(i2cBus, address);

            // samples will need to pass null
            if (interruptPin != -1)
            {
                var gpio = GpioController.GetDefault();

                _interruptPort = gpio.OpenPin(interruptPin);
                _interruptPort.SetDriveMode(GpioPinDriveMode.InputPullUp);
                _interruptPort.ValueChanged += _interruptPort_ValueChanged; 

                //_interruptPort = device.CreateDigitalInputPort(interruptPin);

                //_interruptPort.Changed += InterruptPort_Changed;
            }
        }

        

        /// <summary>
        /// Create a new Ds3231 object using the default parameters for the component.
        /// </summary>
        /// <param name="address">Address of the DS3231 (default = 0x68).</param>
        /// <param name="speed">Speed of the I2C bus (default = 100 KHz).</param>
        /// <param name="interruptPort">Digital port connected to the alarm interrupt pin on the RTC.</param>
        public Ds3231(string i2cBus, GpioPin interruptPort, byte address = 0x68, ushort speed = 100)
        {
            var settings = new I2cConnectionSettings(address, 100_000); //The slave's address and the bus speed.
            var controller = I2cController.FromName(i2cBus);
            _ds323x = controller.GetDevice(settings);
            //_ds323x = new I2cPeripheral(i2cBus, address);

            // samples will need to pass null
            if (interruptPort != null)
            {
                _interruptPort = interruptPort;
                _interruptPort.ValueChanged += _interruptPort_ValueChanged;

                //_interruptPort = interruptPort;

                // _interruptPort.Changed += InterruptPort_Changed;
            }
        }

        #endregion Constructors
    }
}