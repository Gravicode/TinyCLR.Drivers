using GHIElectronics.TinyCLR.Devices.Gpio;
using System;


namespace Meadow.TinyCLR.Motors.Stepper
{
    /// <summary>
    /// This class is for the A4988 Stepper Motor Driver
    /// </summary>
    public class A4988
    {
        private GpioPin stepPort;
        private GpioPin directionPort;
        private GpioPin enable;
        private GpioPin ms1;
        private GpioPin ms2;
        private GpioPin ms3;
        private StepDivisor _divisor;
        private object _syncRoot = new object();
        private float _stepAngle;
        private int _stepDivisor;

        /// <summary>
        /// Sets or gets the direction of rotation used for Step or Rotate methods.
        /// </summary>
        public RotationDirection Direction { get; set; }

        /// <summary>
        /// Creates an instance of the A4988 Stepper Motor Driver
        /// </summary>
        /// <param name="device">The IIoDevice instance that can create Digital Output Ports</param>
        /// <param name="step">The Meadow pin connected to the STEP pin of the A4988</param>
        /// <param name="direction">The Meadow pin connected to the DIR pin of the A4988</param>
        /// <remarks>You must provide either all of the micro-step (MS) lines or none of them</remarks>
        public A4988(int step, int direction)
            : this( step, direction, 0, -1, -1, -1)
        {
        }

        /// <summary>
        /// Creates an instance of the A4988 Stepper Motor Driver
        /// </summary>
        /// <param name="device">The IIoDevice instance that can create Digital Output Ports</param>
        /// <param name="step">The Meadow pin connected to the STEP pin of the A4988</param>
        /// <param name="direction">The Meadow pin connected to the DIR pin of the A4988</param>
        /// <param name="ms1">The (optional) Meadow pin connected to the MS1 pin of the A4988</param>
        /// <param name="ms2">The (optional) Meadow pin connected to the MS2 pin of the A4988</param>
        /// <param name="ms3">The (optional) Meadow pin connected to the MS3 pin of the A4988</param>
        /// <remarks>You must provide either all of the micro-step (MS) lines or none of them</remarks>
        public A4988( int step, int direction, int ms1, int ms2, int ms3)
            : this( step, direction, 0, ms1, ms2, ms3)
        {

        }

        /// <summary>
        /// Creates an instance of the A4988 Stepper Motor Driver
        /// </summary>
        /// <param name="device">The IIoDevice instance that can create Digital Output Ports</param>
        /// <param name="step">The Meadow pin connected to the STEP pin of the A4988</param>
        /// <param name="direction">The Meadow pin connected to the DIR pin of the A4988</param>
        /// <param name="enable">The (optional) Meadow pin connected to the ENABLE pin of the A4988</param>
        public A4988(int step, int direction, int enable)
            : this( step, direction, enable, -1, -1, -1)
        {

        }

        /// <summary>
        /// Creates an instance of the A4988 Stepper Motor Driver
        /// </summary>
        /// <param name="device">The IIoDevice instance that can create Digital Output Ports</param>
        /// <param name="step">The Meadow pin connected to the STEP pin of the A4988</param>
        /// <param name="direction">The Meadow pin connected to the DIR pin of the A4988</param>
        /// <param name="enable">The (optional) Meadow pin connected to the ENABLE pin of the A4988</param>
        /// <param name="ms1">The (optional) Meadow pin connected to the MS1 pin of the A4988</param>
        /// <param name="ms2">The (optional) Meadow pin connected to the MS2 pin of the A4988</param>
        /// <param name="ms3">The (optional) Meadow pin connected to the MS3 pin of the A4988</param>
        /// <remarks>You must provide either all of the micro-step (MS) lines or none of them</remarks>
        public A4988( int step, int direction, int enable, int ms1, int ms2, int ms3)
        {
            var gpio = GpioController.GetDefault();
            stepPort = gpio.OpenPin(step);
            stepPort.SetDriveMode(GpioPinDriveMode.Output);

            directionPort = gpio.OpenPin(direction);
            directionPort.SetDriveMode(GpioPinDriveMode.Output);
         
            if (enable != 0)
            {
                this.enable = gpio.OpenPin(enable);
                this.enable.SetDriveMode(GpioPinDriveMode.Output);
               
            }

            // micro-step lines (for now) are all-or-nothing
            // TODO: rethink this?
            if ( ms1 !=-1 && ms2 != -1 && ms3 != -1 )
            {
                this.ms1 = gpio.OpenPin(ms1);
                this.ms1.SetDriveMode(GpioPinDriveMode.Output);

                this.ms2 = gpio.OpenPin(ms2);
                this.ms2.SetDriveMode(GpioPinDriveMode.Output);

                this.ms3 = gpio.OpenPin(ms3);
                this.ms3.SetDriveMode(GpioPinDriveMode.Output);
               
            }
            else if (ms1 == -1 &&  ms2 == -1 && ms3 == -1)
            {
                // nop
            }
            else
            {
                throw new ArgumentException("All micro-step pins must be either null or valid pins");
            }

            StepAngle = 1.8f; // common default
            RotationSpeedDivisor = 2;
        }

        /// <summary>
        /// Gets the number of steps/micro-steps in the current configuration required for one 360-degree revolution.
        /// </summary>
        public int StepsPerRevolution
        {
            get
            {
                var v = (int)(360f / _stepAngle) * (int)StepDivisor;
                return v;
            }
        }

        /// <summary>
        /// Gets or sets the angle, in degrees, of one step for the connected stepper motor.
        /// </summary>
        public float StepAngle
        {
            get => _stepAngle;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException("Step angle must be positive");
                if (value == _stepAngle) return;
                _stepAngle = value;
            }
        }

        /// <summary>
        /// Divisor for micro-stepping a motor.  This requires the three micro-step control lines to be connected to the motor.
        /// </summary>
        public StepDivisor StepDivisor
        {
            get => _divisor;
            set
            {
                // micro-steps are either all available or not available, so only check one
                // TODO: should we allow partial (i.e. the user uses full or half steps)?
                if ((ms1 == null) && (value != StepDivisor.Divisor_1))
                {
                    throw new ArgumentException("No Micro Step Pins were provided");
                }

                lock (_syncRoot)
                {
                    switch (value)
                    {
                        case StepDivisor.Divisor_2:
                            ms1.Write(  GpioPinValue.High);
                            ms2.Write(GpioPinValue.Low);
                            ms3.Write(GpioPinValue.Low);
                            break;
                        case StepDivisor.Divisor_4:
                            ms2.Write(GpioPinValue.High);
                            ms1.Write(GpioPinValue.Low);
                            ms3.Write(GpioPinValue.Low);
                            break;
                        case StepDivisor.Divisor_8:
                            ms1.Write(GpioPinValue.High);
                            ms2.Write(GpioPinValue.High);
                            ms3.Write(GpioPinValue.Low);
                            break;
                        case StepDivisor.Divisor_16:
                            ms1.Write(GpioPinValue.High);
                            ms2.Write(GpioPinValue.High); 
                            ms3.Write(GpioPinValue.High); 
                            break;
                        default:
                            ms1.Write(GpioPinValue.Low);
                            ms2.Write(GpioPinValue.Low);
                            ms3.Write(GpioPinValue.Low);
                            break;
                    }

                    _divisor = value;
                }
            }
        }

        /// <summary>
        /// Rotates the stepper motor a specified number of degrees
        /// </summary>
        /// <param name="count">Degrees to rotate</param>
        /// <param name="direction">Direction of rotation</param>
        public void Rotate(float degrees, RotationDirection direction)
        {
            Direction = direction;
            Rotate(degrees);
        }

        /// <summary>
        /// Rotates the stepper motor a specified number of degrees
        /// </summary>
        /// <param name="count">Degrees to rotate</param>
        public void Rotate(float degrees)
        {
            // how many steps is it?
            var stepsRequired = (int)(StepsPerRevolution / 360f * degrees);
            Step(stepsRequired);
        }

        /// <summary>
        /// Divisor used to adjust rotaional speed of the stepper motor
        /// </summary>
        public int RotationSpeedDivisor
        {
            get => _stepDivisor;
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("Divisor must be >= 1");
                if (value == RotationSpeedDivisor) return;
                _stepDivisor = value;
            }

        }

        /// <summary>
        /// Rotates the stepper motor a specified number of steps (or microsteps)
        /// </summary>
        /// <param name="count">Number of steps to rotate</param>
        /// <param name="direction">Direction of rotation</param>
        public void Step(int count, RotationDirection direction)
        {
            Direction = direction;
            Step(count);
        }

        /// <summary>
        /// Rotates the stepper motor a specified number of steps (or microsteps)
        /// </summary>
        /// <param name="count">Number of steps to rotate</param>
        public void Step(int count)
        {
            lock (_syncRoot)
            {
                directionPort.Write( Direction == RotationDirection.Clockwise ? GpioPinValue.High: GpioPinValue.Low);

                // TODO: add acceleration
                for (int i = 0; i < count; i++)
                {
                    // HACK HACK HACK
                    // We know that each call to set state true == ~210us on Beta 3.10
                    // We could use unmanaged code to tune it better, but we need a <1ms sleep to do it
                    for (var s = 0; s < RotationSpeedDivisor; s++)
                    {
                        stepPort.Write(GpioPinValue.High);// = true;
                    }

                    stepPort.Write(GpioPinValue.Low);// = false;
                }
                // TODO: add deceleration
            }
        }
    }
}