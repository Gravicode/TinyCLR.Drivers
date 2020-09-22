using GHIElectronics.TinyCLR.Devices.Gpio;
using System;

namespace Meadow.TinyCLR.Sensors.Motion
{
    /// <summary>
    ///     Create a new Hscens0040 object.
    /// </summary>
    public class Hcsens0040
    {
        #region Member variables and fields

        /// <summary>
        ///     Digital input port
        /// </summary>
        private GpioPin _digitalInputPort;

        #endregion Member variables and fields

        #region Delegates and events

        /// <summary>
        ///     Delgate for the motion start and end events.
        /// </summary>
        public delegate void MotionChange(object sender);

        /// <summary>
        ///     Event raised when motion is detected.
        /// </summary>
        public event MotionChange OnMotionDetected;

        #endregion Delegates and events

        #region Constructors

        /// <summary>
        /// Create a new Parallax PIR object connected to an input pin and IO Device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="inputPin"></param>        
        public Hcsens0040( int pin) 
        {
            //    this (device.CreateDigitalInputPort(pin, InterruptMode.EdgeRising, ResistorMode.PullDown)) {
            var gpio = GpioController.GetDefault();

            var Dpin = gpio.OpenPin(pin);
            Dpin.SetDriveMode(GpioPinDriveMode.InputPullDown);
            Setup(Dpin);
        }

        /// <summary>
        /// Create a new Parallax PIR object connected to a interrupt port.
        /// </summary>
        /// <param name="digitalInputPort"></param>        
        public Hcsens0040(GpioPin digitalInputPort)
        {
            Setup(digitalInputPort);
        }
        void Setup(GpioPin digitalInputPort)
        {
            if (digitalInputPort != null)
            {
                _digitalInputPort = digitalInputPort;
                _digitalInputPort.ValueChanged += _digitalInputPort_ValueChanged; 
            }
            else
            {
                throw new Exception("Invalid pin for the PIR interrupts.");
            }
        }

       
        #endregion Constructors

        #region Interrupt handlers

        /// <summary>
        ///     Catch the PIR motion change interrupts and work out which interrupt should be raised.
        /// </summary>
       
        private void _digitalInputPort_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            if (_digitalInputPort.Read() == GpioPinValue.High)
            {
                OnMotionDetected?.Invoke(this);
            }
        }
        #endregion Interrupt handlers
    }
}