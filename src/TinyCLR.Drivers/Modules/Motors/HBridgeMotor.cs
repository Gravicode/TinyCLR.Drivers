using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Pwm;
using System;
using System.Collections;
using System.Text;
using System.Threading;
using TinyCLR.Drivers.Interface;

namespace TinyCLR.Drivers.Modules.Motors
{
    /// <summary>
    /// Generic h-bridge motor controller.
    /// </summary>
    public class HBridgeMotor : IDCMotor
    {
        protected PwmChannel _motorLeftPwm = null; // H-Bridge 1A pin
        protected PwmChannel _motorRighPwm = null; // H-Bridge 2A pin
        protected GpioPin _enablePort = null; // if enabled, then IsNeutral = false

        /// <summary>
        /// When true, the wheels spin "freely"
        /// </summary>
        public bool IsNeutral
        {
            get => _isNeutral;
            set
            {
                _isNeutral = value;
                // if neutral, we disable the port
                _enablePort.Write( !_isNeutral ? GpioPinValue.High : GpioPinValue.Low);
            }
        }
        protected bool _isNeutral = true;

        /// <summary>
        /// 0 - 1 for the speed.
        /// </summary>
        public float Speed
        {
            get => _speed;
            set
            {
                _motorLeftPwm.Stop();
                _motorRighPwm.Stop();

                _speed = value;

                var calibratedSpeed = _speed * MotorCalibrationMultiplier;
                var absoluteSpeed = Math.Min(Math.Abs(calibratedSpeed), 1);
                var isForward = calibratedSpeed > 0;

                _motorLeftPwm.SetActiveDutyCyclePercentage((isForward) ? absoluteSpeed : 0);
                _motorRighPwm.SetActiveDutyCyclePercentage((isForward) ? 0 : absoluteSpeed);
                IsNeutral = false;

                _motorLeftPwm.Start();
                _motorRighPwm.Start();
            }
        }
        protected float _speed = 0;

        /// <summary>
        /// The frequency of the PWM used to drive the motors. 
        /// Default value is 1600.
        /// </summary>
        public float PwmFrequency
        {
            get => _pwmFrequency;
        }
        protected float _pwmFrequency;

        /// <summary>
        /// Not all motors are created equally. This number scales the Speed Input so
        /// that you can match motor speeds without changing your logic.
        /// </summary>
        public float MotorCalibrationMultiplier { get; set; } = 1;

        public HBridgeMotor(string PwmControllerName, int a1Pin, int a2Pin, GpioPin enablePin, float pwmFrequency = 1600) 
        {
            //this(device.CreatePwmPort(a1Pin), device.CreatePwmPort(a2Pin), enablePin, pwmFrequency)
            var controller = PwmController.FromName(PwmControllerName);
            var a1Port = controller.OpenChannel(a1Pin);
            var a2Port = controller.OpenChannel(a2Pin);
            setPwm(a1Port,a2Port, enablePin, pwmFrequency);
        }

        public HBridgeMotor(PwmChannel a1Pin, PwmChannel a2Pin, GpioPin enablePin, float pwmFrequency = 1600)
        {
            setPwm( a1Pin,  a2Pin,  enablePin,  pwmFrequency );
        }

        void setPwm(PwmChannel a1Pin, PwmChannel a2Pin, GpioPin enablePin, float pwmFrequency = 1600)
        {
            _pwmFrequency = pwmFrequency;

            _motorLeftPwm = a1Pin;
            _motorLeftPwm.Controller.SetDesiredFrequency(1600);
            _motorLeftPwm.Start();

            _motorRighPwm = a2Pin;
            _motorRighPwm.Controller.SetDesiredFrequency( 1600);
            _motorRighPwm.Start();

            _enablePort = enablePin;
        }
    }
}
