using GHIElectronics.TinyCLR.Devices.Gpio;
using Meadow.Peripherals.Sensors.Distance;
using System;
using System.Diagnostics;
using System.Threading;

namespace Meadow.TinyCLR.Sensors.Distance
{
    /// <summary>
    /// HCSR04 Distance Sensor
    /// </summary>
    public class Hcsr04 : IRangeFinder
    {
        #region Properties

        /// <summary>
        /// Returns current distance detected in cm.
        /// </summary>
        public float CurrentDistance { get; private set; } = -1;

        /// <summary>
        /// Minimum valid distance in cm (CurrentDistance returns -1 if below).
        /// </summary>
        public float MinimumDistance => 2;

        /// <summary>
        /// Maximum valid distance in cm (CurrentDistance returns -1 if above).
        /// </summary>
        public float MaximumDistance => 400;

        /// <summary>
        /// Raised when an received a rebound trigger signal
        /// </summary>
        public event DistanceEventHandler DistanceDetected = delegate { };

        #endregion

        #region Member variables / fields

        /// <summary>
        /// Trigger Pin.
        /// </summary>
        protected GpioPin triggerPort;

        /// <summary>
        /// Echo Pin.
        /// </summary>
        protected GpioPin echoPort;

        protected long tickStart;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new HCSR04 object with an IO Device
        /// </summary>
        /// <param name="triggerPin"></param>
        /// <param name="echoPin"></param>
        public Hcsr04(int triggerPin, int echoPin)
        {
            //this (device.CreateDigitalOutputPort(triggerPin, false), 
            //device.CreateDigitalInputPort(echoPin, InterruptMode.EdgeBoth))
            var gpio = GpioController.GetDefault();
            var trigger = gpio.OpenPin(triggerPin);
            trigger.SetDriveMode(GpioPinDriveMode.Output);
            trigger.Write(GpioPinValue.Low);

            var echo = gpio.OpenPin(echoPin);
            echo.SetDriveMode(GpioPinDriveMode.InputPullUp);
            Setup(trigger, echo);
        }

        /// <summary>
        /// Create a new HCSR04 object 
        /// </summary>
        /// <param name="triggerPort"></param>
        /// <param name="echoPort"></param>
        public Hcsr04(GpioPin triggerPort, GpioPin echoPort)
        {
            Setup(triggerPort, echoPort);
        }
        void Setup(GpioPin triggerPort, GpioPin echoPort)
        {
            this.triggerPort = triggerPort;

            this.echoPort = echoPort;
            this.echoPort.ValueChanged += EchoPort_ValueChanged;
        }

        private void EchoPort_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            //OnEchoPortChanged
            if (echoPort.Read() == GpioPinValue.High)
            // if(echoPort.State == true)
            {
                tickStart = DateTime.Now.Ticks;
                return;
            }

            //    Debug.WriteLine("false");

            // Calculate Difference
            var elapsed = DateTime.Now.Ticks - tickStart;

            // Return elapsed ticks
            // x10 for ticks to micro sec
            // divide by 58 for cm (assume speed of sound is 340m/s)
            var curDis = elapsed / 580;

            CurrentDistance = curDis;

            //debug - remove 
            Debug.WriteLine($"{elapsed}, {curDis}, {CurrentDistance}, {DateTime.Now.Ticks}");

            //restore this before publishing to hide false results 
            //    if (CurrentDistance < MinimumDistance || CurrentDistance > MaximumDistance)
            //       CurrentDistance = -1;

            DistanceDetected?.Invoke(this, new DistanceEventArgs(CurrentDistance));
        }
        #endregion

        /// <summary>
        /// Sends a trigger signal
        /// </summary>
        public void MeasureDistance()
        {
            CurrentDistance = -1;

            // Raise trigger port to high for 10+ micro-seconds
            triggerPort.Write(GpioPinValue.High);// true;
            Thread.Sleep(1); //smallest amount of time we can wait

            // Start Clock
            tickStart = DateTime.Now.Ticks;
            // Trigger device to measure distance via sonic pulse
            triggerPort.Write(GpioPinValue.Low);// = false;
        }


    }
}