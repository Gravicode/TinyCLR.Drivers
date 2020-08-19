using GHIElectronics.TinyCLR.Devices.Gpio;
using System;
using System.Collections;
using System.Text;
using System.Threading;
using TinyCLR.Drivers.Interface;

namespace TinyCLR.Drivers.Modules
{
    /// <summary>
    /// Electrical switch (usually mechanical) that switches on an isolated circuit.
    /// </summary>
    public class Relay : IRelay
    {
        /// <summary>
        /// Returns digital output port
        /// </summary>
        public GpioPin DigitalOut { get; protected set; }

        /// <summary>
        /// Returns type of relay.
        /// </summary>
        public RelayType Type { get; protected set; }

        /// <summary>
        /// Whether or not the relay is on. Setting this property will turn it on or off.
        /// </summary>
        public bool IsOn
        {
            get => isOn;
            set
            {


                // if turning on,
                isOn = value;
                DigitalOut.Write(isOn ? GpioPinValue.High : GpioPinValue.Low);

            }
        }
        protected bool isOn = false;
        protected bool onValue = true;

        /// <summary>
        /// Creates a new Relay on an IDigitalOutputPort.
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="type"></param>
        public Relay(int pin, RelayType type = RelayType.NormallyOpen) 
        {
            var gpio = GpioController.GetDefault();
            this.DigitalOut = gpio.OpenPin(pin);
            this.DigitalOut.SetDriveMode(GpioPinDriveMode.Output);
            this.DigitalOut.Write( type == RelayType.NormallyClosed ? GpioPinValue.High : GpioPinValue.Low);
        }

        /// <summary>
        /// Creates a new Relay on an IDigitalOutputPort. Allows you 
        /// to use any peripheral that exposes ports that conform to the
        /// IDigitalOutputPort, such as the MCP23008.
        /// </summary>
        /// <param name="port"></param>
        /// <param name="type"></param>
        public Relay(GpioPin port, RelayType type = RelayType.NormallyOpen)
        {
            // if it's normally closed, we have to invert the "on" value
            Type = type;
            onValue = (Type == RelayType.NormallyClosed) ? false : true;

            DigitalOut = port;
        }

        /// <summary>
        /// Toggles the relay on or off.
        /// </summary>
        public void Toggle()
        {
            IsOn = !IsOn;
        }
    }
}
