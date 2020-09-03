using GHIElectronics.TinyCLR.Devices.Gpio;
using System;
using System.Collections;
using System.Text;
using System.Threading;
using TinyCLR.Drivers.Interface;
using TinyCLR.Drivers.System;

namespace TinyCLR.Drivers.Modules.Switches
{
    /// <summary>
    /// Represents a simple, on/off, Single-Pole-Single-Throw (SPST) switch that closes a circuit 
    /// to either ground/common or high. 
    /// 
    /// Use the SwitchCircuitTerminationType to specify whether the other side of the switch
    /// terminates to ground or high.
    /// 
    /// Note: This class is not yet implemented.
    /// </summary>
    public class SpstSwitch : ISwitch
    {
        #region Properties

        /// <summary>
        /// Describes whether or not the switch circuit is closed/connected (IsOn = true), or open (IsOn = false).
        /// </summary>
        public bool IsOn
        {
            get => DigitalIn.Read()==GpioPinValue.High;
            protected set => Changed(this, new EventArgs());
        }

        /// <summary>
        /// Raised when the switch circuit is opened or closed.
        /// </summary>
        public event EventHandler Changed = delegate { };

        /// <summary>
        /// Returns the DigitalInputPort.
        /// </summary>
        public GpioPin DigitalIn { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates a new SpstSwitch object connected to the specified digital pin, and with the specified CircuitTerminationType in the type parameter.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="pin"></param>
        /// <param name="interruptMode"></param>
        /// <param name="resistorMode"></param>
        /// <param name="debounceDuration"></param>
        /// <param name="glitchFilterCycleCount"></param>
        public SpstSwitch(int pin, GpioPinEdge interruptMode, GpioPinDriveMode resistorMode, int debounceDuration = 20, int glitchFilterCycleCount = 0) 
        {
            //this(device.CreateDigitalInputPort(pin, interruptMode, resistorMode, debounceDuration, glitchFilterCycleCount))
            var gpio = GpioController.GetDefault();
            this.DigitalIn = gpio.OpenPin(pin);
            this.DigitalIn.SetDriveMode(resistorMode);
            this.DigitalIn.ValueChangedEdge = interruptMode;
            this.DigitalIn.DebounceTimeout = new TimeSpan(0, 0, 0, debounceDuration);
            
        }

        /// <summary>
        /// Creates a SpstSwitch on a especified interrupt port
        /// </summary>
        /// <param name="interruptPort"></param>
        public SpstSwitch(GpioPin interruptPort)
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
