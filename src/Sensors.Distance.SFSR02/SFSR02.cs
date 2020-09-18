using GHIElectronics.TinyCLR.Devices.Gpio;
using Meadow.Peripherals.Sensors.Distance;
using System;
using System.Threading;

namespace Meadow.Foundation.Sensors.Distance
{
    public class Sfsr02 : IRangeFinder
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
        public float MaximumDistance => 800;

        /// <summary>
        /// Raised when an received a rebound trigger signal
        /// </summary>
        public event DistanceEventHandler DistanceDetected;

        #endregion

        #region Member variables / fields

        /// <summary>
        /// Trigger/Echo Pin
        /// </summary>
        protected GpioPin triggerEchoPort;

        protected long tickStart;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new SFSR02 object with an IO Device
        /// </summary>
        /// <param name="triggerEchoPin"></param>
        /// <param name="device"></param>
        public Sfsr02(int triggerEchoPin) 
            
        {
            //this(device.CreateBiDirectionalPort(triggerEchoPin, false))
            var gpio = GpioController.GetDefault();
            var trigger = gpio.OpenPin(triggerEchoPin);
            trigger.SetDriveMode(GpioPinDriveMode.Output);
            trigger.Write(GpioPinValue.Low);
            Setup(trigger);
        }

        /// <summary>
        /// Create a new SFSR02 object 
        /// </summary>
        /// <param name="triggerEchoPort"></param>
        public Sfsr02(GpioPin triggerEchoPort)
        {
            Setup(triggerEchoPort);
        }

        void Setup(GpioPin triggerEchoPort)
        {
            this.triggerEchoPort = triggerEchoPort;

            this.triggerEchoPort.ValueChanged += TriggerEchoPort_ValueChanged;
        }

        private void TriggerEchoPort_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            if (triggerEchoPort.Read() == GpioPinValue.High)
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
            triggerEchoPort.SetDriveMode(GpioPinDriveMode.Output);
            triggerEchoPort.Write(GpioPinValue.Low);
            Thread.Sleep(1); //smallest amount of time we can wait

            CurrentDistance = -1;

            // Raise trigger port to high for 20 micro-seconds
            triggerEchoPort.Write(GpioPinValue.High);
            Thread.Sleep(1); //smallest amount of time we can wait

            // Start Clock
            tickStart = DateTime.Now.Ticks;
            // Trigger device to measure distance via sonic pulse
            triggerEchoPort.Write(GpioPinValue.Low);// = false;

            triggerEchoPort.SetDriveMode(GpioPinDriveMode.Input);// = PortDirectionType.Input;
        }

      
    }
}
