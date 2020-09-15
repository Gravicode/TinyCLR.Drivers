using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
using Meadow.TinyCLR.Core;
using Meadow.Utilities;
using System;


namespace Meadow.TinyCLR.ICs.IOExpanders
{
    /// <summary>
    /// Provide an interface to connect to a 74595 shift register.
    /// </summary>
    /// <remarks>
    /// Control the outputs from a 74595 shift register (or a chain of shift registers)
    /// using a SPI interface.
    /// </remarks>
    public partial class x74595 //: IIODevice
    {
        #region Properties

        public PinDefinitions Pins { get; } = new PinDefinitions();

        #endregion

        #region Constants

        #endregion

        #region Member variables / fields

        /// <summary>
        ///     Number of chips required to implement this ShiftRegister.
        /// </summary>
        private readonly int _numberOfChips;

        private byte[] _latchData;

        /// <summary>
        ///     SPI interface used to communicate with the shift registers.
        /// </summary>
        private readonly SpiDevice _spi;

        //public DeviceCapabilities Capabilities => throw new NotImplementedException();

        #endregion Member variables / fields

        #region Constructor(s)

        /// <summary>
        ///     Default constructor.
        /// </summary>
        /// <remarks>
        ///     This is private to prevent the programmer from calling it explicitly.
        /// </remarks>
        private x74595()
        {
        }

        /// <summary>
        ///     Constructor a ShiftRegister 74595 object.
        /// </summary>
        /// <param name="pins">Number of pins in the shift register (should be a multiple of 8 pins).</param>
        /// <param name="spiBus">SpiBus object</param>
        public x74595(string spiBus, int pinChipSelect, int pins = 8)
        {
           // if ((pins > 0) && ((pins % 8) == 0))
            if(pins == 8)
            {
                _numberOfChips = pins / 8;

                _latchData = new byte[_numberOfChips];

                var gpio = GpioController.GetDefault();
              
                var chipSelectPort = gpio.OpenPin(pinChipSelect);
                chipSelectPort.SetDriveMode(GpioPinDriveMode.Output);
                chipSelectPort.Write(GpioPinValue.Low);

                var settings = new SpiConnectionSettings()
                {
                    ChipSelectType = SpiChipSelectType.Gpio,
                    ChipSelectLine = chipSelectPort,
                    Mode = SpiMode.Mode1,
                    ClockFrequency = 4_000_000,
                };

                var controller = SpiController.FromName(spiBus);
                _spi = controller.GetDevice(settings);
                //_spi = new SpiPeripheral(spiBus, device.CreateDigitalOutputPort(pinChipSelect));
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    "x74595: Size must be greater than zero and a multiple of 8 pins, driver is currently limited to one chip (8 pins)");
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new DigitalOutputPort using the specified pin and initial state.
        /// </summary>
        /// <param name="pin">The pin number to create the port on.</param>
        /// <param name="initialState">Whether the pin is initially high or low.</param>
        /// <returns></returns>
        public DigitalOutputPort CreateDigitalOutputPort(int pin, bool initialState, OutputType outputType)
        {
            if (IsValidPin(pin))
            {
                // create the convenience class
                return new DigitalOutputPort(this, pin, initialState, outputType);
            }

            throw new Exception("Pin is out of range");
        }

        public void Clear(bool update = true)
        {
            _latchData = new byte[_numberOfChips];

            if(update)
            {
                _spi.Write(_latchData);
            }
        }

        /// <summary>
        /// Sets a particular pin's value. 
        /// </summary>
        /// <param name="pin">The pin to write to.</param>
        /// <param name="value">The value to write. True for high, false for low.</param>
        public void WriteToPin(int pin, bool value)
        {
            if (IsValidPin(pin))
            {
                //ToDo multichip logic
                _latchData[0] =  BitHelpers.SetBit(_latchData[0], (byte)pin, value);
            }
            else
            {
                throw new Exception("Pin is out of range");
            }
            _spi.Write(_latchData);
        }

        /// <summary>
        /// Checks whether or not the pin passed in exists on the chip.
        /// </summary>
        protected bool IsValidPin(int pin)
        {
            return Pins.AllPins.Contains(pin);
        }

        public GpioPin CreateDigitalInputPort(int pin, 
            GpioPinEdge interruptMode =  GpioPinEdge.FallingEdge, 
            ResistorMode resistorMode = ResistorMode.Disabled, 
            double debounceDuration = 0,
            double glitchFilterCycleCount = 0)
        {
            throw new NotImplementedException();
        }

        


        //TODO: we know adding all these sucks. when we convert to .NET Core
        // we'll be able to add these to the IIODevice interface implementation
        // and they won't be necessary to put in like this.


        #endregion
    }
}