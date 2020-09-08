using GHIElectronics.TinyCLR.Devices.Gpio;
using System;
using System.Collections;
using System.Text;
using System.Threading;
using Meadow.TinyCLR.Interface;
using Meadow.TinyCLR.System;

namespace Meadow.TinyCLR.Modules.Switches
{
    /// <summary>
    /// Represents a simple, two position, Single-Pole-Dual-Throw (SPDT) switch that closes a circuit 
    /// to either ground/common or high depending on position.
    /// 
    /// Note: This class is not yet implemented.
    /// </summary>
    public class SpdtSwitch : ISwitch
    {
        #region Properties

        /// <summary>
        /// Describes whether or not the switch circuit is closed/connected (IsOn = true), or open (IsOn = false).
        /// </summary>
        public bool IsOn
        {
            get => DigitalIn.Read()== GpioPinValue.High;
            protected set => Changed(this, new EventArgs());
        }

        /// <summary>
        /// Returns the DigitalInputPort.
        /// </summary>
        public GpioPin DigitalIn { get; protected set; }

        /// <summary>
        /// Raised when the switch circuit is opened or closed.
        /// </summary>
        public event EventHandler Changed = delegate { };

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates a new SpdtSwitch object with the center pin connected to the specified digital pin, one pin connected to common/ground and one pin connected to high/3.3V.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="pin"></param>
        /// <param name="interruptMode"></param>
        /// <param name="resistorMode"></param>
        /// <param name="debounceDuration"></param>
        /// <param name="glitchFilterCycleCount"></param>
        public SpdtSwitch(int pin, GpioPinEdge interruptMode, GpioPinDriveMode resistorMode, int debounceDuration = 20, int glitchFilterCycleCount = 0) 
            
        {
            var gpio = GpioController.GetDefault();
            this.DigitalIn = gpio.OpenPin(pin);
            this.DigitalIn.SetDriveMode(resistorMode);
            this.DigitalIn.ValueChangedEdge = interruptMode;
            this.DigitalIn.DebounceTimeout = new TimeSpan(0, 0, 0, debounceDuration);
        }

        /// <summary>
        /// Creates a SpdtSwitch on a especified interrupt port
        /// </summary>
        /// <param name="interruptPort"></param>
        public SpdtSwitch(GpioPin interruptPort)
        {
            DigitalIn = interruptPort;
            DigitalIn.ValueChanged += DigitalIn_ValueChanged;
        }



        #endregion

        #region Methods

        /// <summary>
        /// Event handler when switch value has been changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DigitalIn_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            IsOn = DigitalIn.Read() == GpioPinValue.High;
        }

        #endregion
    }
}
