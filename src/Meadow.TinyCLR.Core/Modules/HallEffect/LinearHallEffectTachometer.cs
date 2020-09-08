using GHIElectronics.TinyCLR.Devices.Gpio;
using System;
using System.Collections;
using System.Text;
using System.Threading;
using Meadow.TinyCLR.Core;
using Meadow.TinyCLR.System;

namespace Meadow.TinyCLR.Modules.HallEffect
{
    /// <summary>
    /// Represents a Lineal Hall Effect tachometer.
    /// 
    /// Note: This class is not yet implemented.
    /// </summary>
    public class LinearHallEffectTachometer
    {
        public delegate void RpmChangeHandler(object sender, FloatChangeResult e);
        /// <summary>
        /// Event raised when the RPM change is greater than the 
        /// RPMChangeNotificationThreshold value.
        /// </summary>
        public event RpmChangeHandler RPMsChanged = delegate { };
        //FloatChangeResult

        /// <summary>
        /// Any changes to the RPMs that are greater than the RPM change
        /// threshold will cause an event to be raised when the instance is
        /// set to update automatically.
        /// </summary>
        public float RPMChangeNotificationThreshold { get; set; } = 0.001F;

        /// <summary>
        /// Input port for the tachometer
        /// </summary>
        public GpioPin InputPort { get; private set; }

        /// <summary>
        /// Returns number of magnets of the sensor.
        /// </summary>
        public ushort NumberOfMagnets { get; set; }

        /// <summary>
        /// Returns number of revolutions per minute.
        /// </summary>
        public int RPMs => (int)_RPMs;

        protected float _RPMs = 0.0F;
        protected float _lastNotifiedRPMs = 0.0F;
        protected DateTime _revolutionTimeStart = DateTime.MinValue;
        protected ushort _numberOfReads = 0; //

        /// <summary>
        /// LinearHallEffectTachometer driver
        /// </summary>
        /// <param name="inputPin"></param>
        /// <param name="type"></param>
        /// <param name="numberOfMagnets"></param>
        /// <param name="rpmChangeNotificationThreshold"></param>
        public LinearHallEffectTachometer(int inputPin, CircuitTerminationType type = CircuitTerminationType.CommonGround,
            ushort numberOfMagnets = 2, float rpmChangeNotificationThreshold = 1.0F) 
            
        {
            //this(device.CreateDigitalInputPort(inputPin), type, numberOfMagnets, rpmChangeNotificationThreshold)
          
            var gpio = GpioController.GetDefault();
            var DigitalInput = gpio.OpenPin(inputPin);
            DigitalInput.SetDriveMode(GpioPinDriveMode.Input);
            setTachoMeter(DigitalInput, type, numberOfMagnets, rpmChangeNotificationThreshold);          
            
        }

        void setTachoMeter(GpioPin inputPort, CircuitTerminationType type = CircuitTerminationType.CommonGround,
            ushort numberOfMagnets = 2, float rpmChangeNotificationThreshold = 1.0F)
        {
            NumberOfMagnets = numberOfMagnets;
            RPMChangeNotificationThreshold = rpmChangeNotificationThreshold;

            // if we terminate in ground, we need to pull the port high to test for circuit completion, otherwise down.
            //var resistorMode = (type == CircuitTerminationType.CommonGround) ? H.Port.ResistorMode.PullUp : H.Port.ResistorMode.PullDown;

            // create the interrupt port from the pin and resistor type
            InputPort = inputPort;

            InputPort.ValueChanged += InputPort_ValueChanged;
        }

        private void InputPort_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            var time = DateTime.Now;

            // if it's the very first read, set the time and bail out
            if (_numberOfReads == 0 && _revolutionTimeStart == DateTime.MinValue)
            {
                //S.Console.WriteLine("First reading.");
                _revolutionTimeStart = time;
                _numberOfReads++;
                return;
            }

            // increment our count of magnet detections
            _numberOfReads++;

            // if we've made a full revolution
            if (_numberOfReads == NumberOfMagnets)
            {
                //S.Console.WriteLine("Viva La Revolucion!");
                // calculate how much time has elapsed since the start of the revolution 
                var revolutionTime = time - _revolutionTimeStart;

                //S.Console.WriteLine("RevTime Milliseconds: " + revolutionTime.Milliseconds.ToString());

                if (revolutionTime.Milliseconds < 3)
                {
                    //S.Console.WriteLine("rev time < 3. Garbage, bailing.");
                    _numberOfReads = 0;
                    _revolutionTimeStart = time;
                    return;
                }

                // calculate our rpms
                // RPSecond = 1000 / revTime.millis
                // PPMinute = RPSecond * 60
                _RPMs = ((float)1000 / (float)revolutionTime.Milliseconds) * (float)60;

                //if (revolutionTime.Milliseconds < 5) {
                //    S.Console.WriteLine("revolution time was < 5. garbage results.");
                //} else {
                //    S.Console.WriteLine("RPMs: " + _RPMs);
                //}


                // reset our number of reads and store our revolution time start
                _numberOfReads = 0;
                _revolutionTimeStart = time;

                // if the change is enough, raise the event.
                if (Math.Abs(_lastNotifiedRPMs - _RPMs) > RPMChangeNotificationThreshold)
                {
                    OnRaiseRPMChanged();
                }
            }
        }

        public LinearHallEffectTachometer(GpioPin inputPort, CircuitTerminationType type = CircuitTerminationType.CommonGround,
            ushort numberOfMagnets = 2, float rpmChangeNotificationThreshold = 1.0F)
        {
            setTachoMeter(inputPort, type,
              numberOfMagnets, rpmChangeNotificationThreshold);
        }

     
        protected void OnRaiseRPMChanged()
        {
            
            RPMsChanged(this, new FloatChangeResult(_lastNotifiedRPMs, _RPMs));
            _lastNotifiedRPMs = _RPMs;
        }
    }
    public class FloatChangeResult : EventArgs
    {
        public FloatChangeResult(float old_rpm, float new_rpm)
        {
            this.New = new_rpm;
            this.Old = old_rpm;
        }
        public float New { get; set; }
        public float Old { get; set; }
        public float Delta { get; }
        public float DeltaPercent { get; }
    }
}
