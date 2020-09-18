using System;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using Meadow.Peripherals.Sensors.Distance;

namespace Meadow.TinyCLR.Sensors.Distance
{
    /// <summary>
    /// HYSRF05 Distance Sensor
    /// </summary>
    public class Hysrf05 : IRangeFinder
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
        public float MaximumDistance => 450;

        /// <summary>
        /// Raised when an received a rebound trigger signal
        /// </summary>
        public event DistanceEventHandler DistanceDetected;

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
        /// Create a new HYSRF05 object with a IO Device
        /// HSSRF05 must be running the default 4/5 pin mode
        /// 3 pin mode is not supported on Meadow
        /// </summary>
        /// <param name="triggerPin"></param>
        /// <param name="echoPin"></param>
        public Hysrf05(int triggerPin, int echoPin)
        {
            //this(device.CreateDigitalOutputPort(triggerPin, false),
            //       device.CreateDigitalInputPort(echoPin, InterruptMode.EdgeBoth))
            var gpio = GpioController.GetDefault();
            var trigger = gpio.OpenPin(triggerPin);
            trigger.SetDriveMode(GpioPinDriveMode.Output);
            trigger.Write(GpioPinValue.Low);

            var echo = gpio.OpenPin(echoPin);
            echo.SetDriveMode(GpioPinDriveMode.InputPullUp);
            Setup(trigger, echo);
        }

        /// <summary>
        /// Create a new HYSRF05 object and hook up the interrupt handler
        /// HSSRF05 must be running the default 4/5 pin mode
        /// 3 pin mode is not supported on Meadow
        /// </summary>
        /// <param name="triggerPort"></param>
        /// <param name="echoPort"></param>
        public Hysrf05(GpioPin triggerPort, GpioPin echoPort)
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
            if (echoPort.Read() == GpioPinValue.High) //echo is high
            {
                tickStart = DateTime.Now.Ticks;
                return;
            }

            // Calculate Difference
            float elapsed = DateTime.Now.Ticks - tickStart;

            // Return elapsed ticks
            // x10 for ticks to micro sec
            // divide by 58 for cm (assume speed of sound is 340m/s)
            CurrentDistance = elapsed / 580f;

            if (CurrentDistance < MinimumDistance || CurrentDistance > MaximumDistance)
                CurrentDistance = -1;

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
            triggerPort.Write(GpioPinValue.High);// = true;
            Thread.Sleep(1); //smallest amount of time we can wait

            // Start Clock
            tickStart = DateTime.Now.Ticks;
            // Trigger device to measure distance via sonic pulse
            triggerPort.Write(GpioPinValue.Low);// = false;
        }


    }
}