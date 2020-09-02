using GHIElectronics.TinyCLR.Devices.Gpio;
using System;
using System.Collections;
using System.Text;
using System.Threading;
using TinyCLR.Drivers.Core;
using TinyCLR.Drivers.Interface;
using TinyCLR.Drivers.System;

namespace TinyCLR.Drivers.Modules.Buttons
{
    /// <summary>
    /// A simple push button. 
    /// </summary>
    public class PushButton : IButton
    {
        #region Properties
        /// <summary>
        /// This duration controls the debounce filter. It also has the effect
        /// of rate limiting clicks. Decrease this time to allow users to click
        /// more quickly.
        /// </summary>
        public TimeSpan DebounceDuration
        {
            get => (DigitalIn != null) ? DigitalIn.DebounceTimeout : TimeSpan.MinValue;
            set
            {
                DigitalIn.DebounceTimeout = value;
            }
        }

        /// <summary>
        /// Returns the sanitized state of the switch. If the switch 
        /// is pressed, returns true, otherwise false.
        /// </summary>
        public bool State
        {
            get
            {
                bool currentState = DigitalIn.GetDriveMode() == GpioPinDriveMode.InputPullDown ? true : false;

                return (state == currentState) ? true : false;
            }
        }

        /// <summary>
        /// The minimum duration for a long press.
        /// </summary>
        public TimeSpan LongPressThreshold { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Returns digital input port.
        /// </summary>
        public GpioPin DigitalIn { get; private set; }

        /// <summary>
        /// Raised when a press starts (the button is pushed down; circuit is closed).
        /// </summary>
        public event EventHandler PressStarted = delegate { };

        /// <summary>
        /// Raised when a press ends (the button is released; circuit is opened).
        /// </summary>
        public event EventHandler PressEnded = delegate { };

        /// <summary>
        /// Raised when the button circuit is re-opened after it has been closed (at the end of a �press�.
        /// </summary>
        public event EventHandler Clicked = delegate { };

        /// <summary>
        /// Raised when the button circuit is pressed for at least 500ms.
        /// </summary>
        public event EventHandler LongPressClicked = delegate { };
        #endregion

        #region Member variables / fields
        /// <summary>
        /// Returns the current raw state of the switch.
        /// </summary>
        protected bool state => (DigitalIn != null) ? !(DigitalIn.Read() == GpioPinValue.High)  : false;

        /// <summary>
        /// Minimum DateTime value when the button was pushed
        /// </summary>
        protected DateTime _lastClicked = DateTime.MinValue;

        /// <summary>
        /// Maximum DateTime value when the button was just pushed
        /// </summary>
        protected DateTime buttonPressStart = DateTime.MaxValue;

        /// <summary>
        /// Circuit Termination Type (CommonGround, High or Floating)
        /// </summary>
        protected CircuitTerminationType _circuitType;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates PushButto a digital input port connected on a IIOdevice, especifying Interrupt Mode, Circuit Type and optionally Debounce filter duration.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="inputPin"></param>
        /// <param name="resistor"></param>
        /// <param name="debounceDuration"></param>
        public PushButton(int inputPin, GpioPinDriveMode resistor =  GpioPinDriveMode.Input, int debounceDuration = 20)
        {
            var gpio = GpioController.GetDefault();
            this.DigitalIn = gpio.OpenPin(inputPin);
            this.DigitalIn.SetDriveMode(resistor);
            this.DigitalIn.DebounceTimeout = new TimeSpan(0,0,0, debounceDuration);

            this.DigitalIn.Write(GpioPinValue.Low);
            this.DigitalIn.ValueChanged += DigitalIn_ValueChanged;
            // if we terminate in ground, we need to pull the port high to test for circuit completion, otherwise down.
            //DigitalIn = device.CreateDigitalInputPort(inputPin, InterruptMode.EdgeBoth, resistor, debounceDuration);
            //DigitalIn.Changed += DigitalInChanged;
        }

        private void DigitalIn_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            //if (e.Edge == GpioPinEdge.FallingEdge)
            //{
            //    // Pin went low
            //}
            //else
            //{
            //    // Pin went high
            //}

            bool STATE_PRESSED = DigitalIn.GetDriveMode() == GpioPinDriveMode.InputPullDown ? true : false;
            bool STATE_RELEASED = DigitalIn.GetDriveMode() == GpioPinDriveMode.InputPullDown ? false : true;
            //bool STATE_PRESSED = _circuitType == CircuitTerminationType.High ? true : false;
            //bool STATE_RELEASED = _circuitType == CircuitTerminationType.High ? false : true;

            if (State)
            {
                // save our press start time (for long press event)
                buttonPressStart = DateTime.Now;
                // raise our event in an inheritance friendly way
                this.RaisePressStarted();
            }
            else if (State == false)
            {
                // calculate the press duration
                TimeSpan pressDuration = DateTime.Now - buttonPressStart;

                // reset press start time
                buttonPressStart = DateTime.MaxValue;

                // if it's a long press, raise our long press event
                if (LongPressThreshold > TimeSpan.Zero && pressDuration > LongPressThreshold)
                {
                    this.RaiseLongPress();
                }
                else
                {
                    this.RaiseClicked();
                }

                // raise the other events
                this.RaisePressEnded();
            }
        }

        /// <summary>
        /// Creates a PushButton on a digital input portespecifying Interrupt Mode, Circuit Type and optionally Debounce filter duration.
        /// </summary>
        /// <param name="interruptPort"></param>
        /// <param name="resistor"></param>
        /// <param name="debounceDuration"></param>
        public PushButton(GpioPin interruptPort, GpioPinDriveMode resistor =  GpioPinDriveMode.Input, int debounceDuration = 20)
        {
            DigitalIn = interruptPort;
            DigitalIn.SetDriveMode( resistor);
            DebounceDuration = new TimeSpan(0, 0, 0, 0, debounceDuration);
            this.DigitalIn.ValueChanged += DigitalIn_ValueChanged;
        }

        #endregion

        #region Methods

        //private void DigitalInChanged(object sender, DigitalInputPortEventArgs e)
        //{
           
        //}

        /// <summary>
        /// Raised when the button circuit is re-opened after it has been closed (at the end of a �press�).
        /// </summary>
        protected virtual void RaiseClicked()
        {
            this.Clicked(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raised when a press starts (the button is pushed down; circuit is closed).
        /// </summary>
        protected virtual void RaisePressStarted()
        {
            // raise the press started event
            this.PressStarted(this, new EventArgs());
        }

        /// <summary>
        /// Raised when a press ends (the button is released; circuit is opened).
        /// </summary>
        protected virtual void RaisePressEnded()
        {
            this.PressEnded(this, new EventArgs());
        }

        /// <summary>
        /// Raised when the button circuit is pressed for at least 500ms.
        /// </summary>
        protected virtual void RaiseLongPress()
        {
            this.LongPressClicked(this, new EventArgs());
        }

        #endregion
    }
}
