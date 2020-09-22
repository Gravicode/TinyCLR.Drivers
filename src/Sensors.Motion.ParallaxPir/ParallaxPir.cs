using GHIElectronics.TinyCLR.Devices.Gpio;
using Meadow.TinyCLR.Core;
using System;

namespace Meadow.TinyCLR.Sensors.Motion
{
    /// <summary>
    ///     Create a new Parallax PIR object.
    /// </summary>
    public class ParallaxPir
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
        public event MotionChange OnMotionStart;

        /// <summary>
        ///     Event raised when the PIR indicates that there is not longer any motion.
        /// </summary>
        public event MotionChange OnMotionEnd;

        #endregion Delegates and events

        #region Constructors

        /// <summary>
        /// Create a new Parallax PIR object connected to an input pin and IO Device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="inputPin"></param>        
        public ParallaxPir(int pin, GpioPinEdge interruptMode, ResistorMode resistorMode, int debounceDuration = 20, int glitchFilterCycleCount = 0) 
        {
            // this (device.CreateDigitalInputPort(pin, interruptMode, resistorMode, debounceDuration, glitchFilterCycleCount))
            var gpio = GpioController.GetDefault();

            var Dpin = gpio.OpenPin(pin);
            Dpin.SetDriveMode(resistorMode == ResistorMode.PullUp ? GpioPinDriveMode.InputPullUp : GpioPinDriveMode.InputPullDown);
            Dpin.DebounceTimeout = new TimeSpan (0,0,0, debounceDuration);
            Dpin.ValueChangedEdge = interruptMode;
            Setup(Dpin);
            
        }

        /// <summary>
        /// Create a new Parallax PIR object connected to a interrupt port.
        /// </summary>
        /// <param name="digitalInputPort"></param>        
        public ParallaxPir(GpioPin digitalInputPort)
        {
            Setup(digitalInputPort);
        }

        void Setup(GpioPin digitalInputPort)
        {
            //TODO: I changed this from Pins.GPIO_NONE to null
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
        //private void DigitalInputPortChanged(object sender, DigitalInputPortEventArgs e)
        //{
            
        //}
        private void _digitalInputPort_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            if (_digitalInputPort.Read()==GpioPinValue.High)
            {
                OnMotionStart?.Invoke(this);
            }
            else
            {
                OnMotionEnd?.Invoke(this);
            }
        }
        #endregion Interrupt handlers
    }
}