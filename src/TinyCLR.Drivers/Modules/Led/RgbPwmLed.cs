using GHIElectronics.TinyCLR.Devices.Pwm;
using System;
using System.Collections;
using System.Text;
using System.Threading;
using TinyCLR.Drivers.Core;
using TinyCLR.Drivers.Interface;

namespace TinyCLR.Drivers.Modules.Led
{
    /// <summary>
    /// Represents a Pulse-Width-Modulation (PWM) controlled RGB LED. Controlling an RGB LED with 
    /// PWM allows for more colors to be expressed than if it were simply controlled with normal
    /// digital outputs which provide only binary control at each pin. As such, a PWM controlled 
    /// RGB LED can express millions of colors, as opposed to the 8 colors that can be expressed
    /// via binary digital output.  
    /// </summary>
    public class RgbPwmLed
    {
        readonly int DEFAULT_FREQUENCY = 200; //hz
        readonly float MAX_FORWARD_VOLTAGE = 3.3f;
        readonly float DEFAULT_DUTY_CYCLE = 0f;

        bool animationRunning;

        protected double maxRedDutyCycle = 1;
        protected double maxGreenDutyCycle = 1;
        protected double maxBlueDutyCycle = 1;

        /// <summary>
        /// Turns on LED with current color or turns it off
        /// </summary>
        public bool IsOn
        {
            get => isOn;
            set
            {
                SetColor(Color, value ? 1 : 0);
                isOn = value;
            }
        }
        protected bool isOn;

        /// <summary>
        /// The color the LED has been set to.
        /// </summary>
        public Color Color { get; protected set; } = Color.White;

        /// <summary>
        /// Get the red LED port
        /// </summary>
        public PwmChannel RedPwm { get; protected set; }
        /// <summary>
        /// Get the blue LED port
        /// </summary>
        public PwmChannel BluePwm { get; protected set; }
        /// <summary>
        /// Get the green LED port
        /// </summary>
        public PwmChannel GreenPwm { get; protected set; }

        /// <summary>
        /// Gets the common type
        /// </summary>        
        public CommonType Common { get; protected set; }

        /// <summary>
        /// Get the red LED forward voltage
        /// </summary>
        public float RedForwardVoltage { get; protected set; }
        /// <summary>
        /// Get the green LED forward voltage
        /// </summary>
        public float GreenForwardVoltage { get; protected set; }
        /// <summary>
        /// Get the blue LED forward voltage
        /// </summary>
        public float BlueForwardVoltage { get; protected set; }

        /// <summary>
        /// Instantiates a RgbPwmLed object with the especified IO device, connected
        /// to three digital pins for red, green and blue channels, respectively
        /// </summary>
        /// <param name="device"></param>
        /// <param name="redPwmPin"></param>
        /// <param name="greenPwmPin"></param>
        /// <param name="bluePwmPin"></param>
        /// <param name="redLedForwardVoltage"></param>
        /// <param name="greenLedForwardVoltage"></param>
        /// <param name="blueLedForwardVoltage"></param>
        /// <param name="isCommonCathode"></param>
        public RgbPwmLed(string PwmControllerName,
            int redPwmPin, int greenPwmPin, int bluePwmPin,
            float redLedForwardVoltage = TypicalForwardVoltage.ResistorLimited,
            float greenLedForwardVoltage = TypicalForwardVoltage.ResistorLimited,
            float blueLedForwardVoltage = TypicalForwardVoltage.ResistorLimited,
            CommonType commonType = CommonType.CommonCathode)
        {
            var controller = PwmController.FromName(PwmControllerName);
            var redLed = controller.OpenChannel(redPwmPin);
            var greenLed = controller.OpenChannel(greenPwmPin);
            var blueLed = controller.OpenChannel(bluePwmPin);
            SetRgbPwm(redLed, greenLed, blueLed, redLedForwardVoltage, greenLedForwardVoltage, blueLedForwardVoltage, commonType);
            //controller.SetDesiredFrequency(100);

            /*
             
                this(device.CreatePwmPort(redPwmPin),
                      device.CreatePwmPort(greenPwmPin),
                      device.CreatePwmPort(bluePwmPin),
                      redLedForwardVoltage, greenLedForwardVoltage, blueLedForwardVoltage,
                      commonType)
             */
        }
        void SetRgbPwm(PwmChannel redPwm, PwmChannel greenPwm, PwmChannel bluePwm,
            float redLedForwardVoltage = TypicalForwardVoltage.ResistorLimited,
            float greenLedForwardVoltage = TypicalForwardVoltage.ResistorLimited,
            float blueLedForwardVoltage = TypicalForwardVoltage.ResistorLimited,
            CommonType commonType = CommonType.CommonCathode)
        {
            // validate and persist forward voltages
            if (redLedForwardVoltage < 0 || redLedForwardVoltage > MAX_FORWARD_VOLTAGE)
            {
                throw new ArgumentOutOfRangeException(nameof(redLedForwardVoltage), "error, forward voltage must be between 0, and 3.3");
            }
            RedForwardVoltage = redLedForwardVoltage;

            if (greenLedForwardVoltage < 0 || greenLedForwardVoltage > MAX_FORWARD_VOLTAGE)
            {
                throw new ArgumentOutOfRangeException(nameof(greenLedForwardVoltage), "error, forward voltage must be between 0, and 3.3");
            }
            GreenForwardVoltage = greenLedForwardVoltage;

            if (blueLedForwardVoltage < 0 || blueLedForwardVoltage > MAX_FORWARD_VOLTAGE)
            {
                throw new ArgumentOutOfRangeException(nameof(blueLedForwardVoltage), "error, forward voltage must be between 0, and 3.3");
            }
            BlueForwardVoltage = blueLedForwardVoltage;

            Common = commonType;

            // calculate and set maximum PWM duty cycles
            maxRedDutyCycle = LedHelpers.CalculateMaximumDutyCycle(RedForwardVoltage);
            maxGreenDutyCycle = LedHelpers.CalculateMaximumDutyCycle(GreenForwardVoltage);
            maxBlueDutyCycle = LedHelpers.CalculateMaximumDutyCycle(BlueForwardVoltage);

            RedPwm = redPwm;
            GreenPwm = greenPwm;
            BluePwm = bluePwm;

            ResetPwms();
        }
        /// <summary>
        /// 
        /// Implementation notes: Architecturally, it would be much cleaner to construct this class
        /// as three PwmLeds. Then each one's implementation would be self-contained. However, that
        /// would require three additional threads during ON; one contained by each PwmLed. For this
        /// reason, I'm basically duplicating the functionality for all three in here. 
        /// </summary>
        /// <param name="redPwm"></param>
        /// <param name="greenPwm"></param>
        /// <param name="bluePwm"></param>
        /// <param name="isCommonCathode"></param>
        public RgbPwmLed(PwmChannel redPwm, PwmChannel greenPwm, PwmChannel bluePwm,
            float redLedForwardVoltage = TypicalForwardVoltage.ResistorLimited,
            float greenLedForwardVoltage = TypicalForwardVoltage.ResistorLimited,
            float blueLedForwardVoltage = TypicalForwardVoltage.ResistorLimited,
            CommonType commonType = CommonType.CommonCathode)
        {
            SetRgbPwm(redPwm, greenPwm, bluePwm, redLedForwardVoltage, greenLedForwardVoltage, blueLedForwardVoltage,commonType);
        }

        /// <summary>
        /// Resets all PWM ports
        /// </summary>
        protected void ResetPwms()
        {

            RedPwm.Controller.SetDesiredFrequency(DEFAULT_FREQUENCY);
            GreenPwm.Controller.SetDesiredFrequency(DEFAULT_FREQUENCY);
            BluePwm.Controller.SetDesiredFrequency(DEFAULT_FREQUENCY);

            RedPwm.SetActiveDutyCyclePercentage(DEFAULT_DUTY_CYCLE);
            GreenPwm.SetActiveDutyCyclePercentage(DEFAULT_DUTY_CYCLE);
            BluePwm.SetActiveDutyCyclePercentage(DEFAULT_DUTY_CYCLE);
            // invert the PWM signal if it common anode
            var inverted = (Common == CommonType.CommonAnode);
            RedPwm.Polarity = inverted ? PwmPulsePolarity.ActiveHigh : PwmPulsePolarity.ActiveLow;
            GreenPwm.Polarity = inverted ? PwmPulsePolarity.ActiveHigh : PwmPulsePolarity.ActiveLow;
            BluePwm.Polarity = inverted ? PwmPulsePolarity.ActiveHigh : PwmPulsePolarity.ActiveLow;
           
            RedPwm.Start(); GreenPwm.Start(); BluePwm.Start();
        }

        /// <summary>
        /// Sets the current color of the LED.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="brightness"></param>
        public void SetColor(Color color, float brightness = 1)
        {
            Color = color;

            RedPwm.SetActiveDutyCyclePercentage((float)(Color.R * maxRedDutyCycle * brightness));
            GreenPwm.SetActiveDutyCyclePercentage((float)(Color.G * maxGreenDutyCycle * brightness));
            BluePwm.SetActiveDutyCyclePercentage((float)(Color.B * maxBlueDutyCycle * brightness));
        }

        /// <summary>
        /// Stops any running animations.
        /// </summary>
        public void Stop()
        {
            animationRunning = false;
            IsOn = false;
        }

        /// <summary>
        /// Start the Blink animation which sets the brightness of the LED alternating between a low and high brightness setting, using the durations provided.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="onDuration"></param>
        /// <param name="offDuration"></param>
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        public void StartBlink(Color color, uint onDuration = 200, uint offDuration = 200, float highBrightness = 1f, float lowBrightness = 0f)
        {
            if (highBrightness > 1 || highBrightness <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(highBrightness), "onBrightness must be > 0 and <= 1");
            }
            if (lowBrightness >= 1 || lowBrightness < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lowBrightness), "lowBrightness must be >= 0 and < 1");
            }
            if (lowBrightness >= highBrightness)
            {
                throw new Exception("offBrightness must be less than onBrightness");
            }

            Color = color;

            Stop();
            animParam = new PwmAnimationParameter();
            animParam.color = color;
            animParam.onDuration = onDuration;
            animParam.offDuration = offDuration;
            animParam.highBrightness = highBrightness;
            animParam.lowBrightness = lowBrightness;
            var AnimationTask = new Thread(new ThreadStart(StartBlinkAsync));
            animationRunning = true;
            AnimationTask.Start();

        }

        protected void StartBlinkAsync()
        {
            var color = animParam.color;
            var highBrightness = animParam.highBrightness;
            var onDuration = animParam.onDuration;
            var lowBrightness = animParam.lowBrightness;
            var offDuration = animParam.offDuration;

            while (true)
            {
                if (!animationRunning)
                {
                    break;
                }

                SetColor(color, highBrightness);
                Thread.Sleep((int)onDuration);
                SetColor(color, lowBrightness);
                Thread.Sleep((int)offDuration);
            }
        }

        /// <summary>
        /// Start the Pulse animation which gradually alternates the brightness of the LED between a low and high brightness setting, using the durations provided.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="pulseDuration"></param>
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        public void StartPulse(Color color, uint pulseDuration = 600, float highBrightness = 1, float lowBrightness = 0.15F)
        {
            if (highBrightness > 1 || highBrightness <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(highBrightness), "onBrightness must be > 0 and <= 1");
            }
            if (lowBrightness >= 1 || lowBrightness < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lowBrightness), "lowBrightness must be >= 0 and < 1");
            }
            if (lowBrightness >= highBrightness)
            {
                throw new Exception("offBrightness must be less than onBrightness");
            }

            Color = color;

            Stop();

            animParam = new PwmAnimationParameter();
            animParam.color = color;
            animParam.pulseDuration = pulseDuration;
            animParam.highBrightness = highBrightness;
            animParam.lowBrightness = lowBrightness;
            var AnimationTask = new Thread(new ThreadStart(StartPulseAsync));
            animationRunning = true;
            AnimationTask.Start();
        }

        protected void StartPulseAsync()
        {
            var color = animParam.color;
            var highBrightness = animParam.highBrightness;
            var pulseDuration = animParam.pulseDuration;
            var lowBrightness = animParam.lowBrightness;
          
            //Color color, uint pulseDuration, float highBrightness, float lowBrightness
            float brightness = lowBrightness;
            bool ascending = true;
            int intervalTime = 60; // 60 miliseconds is probably the fastest update we want to do, given that threads are given 20 miliseconds by default. 
            float steps = pulseDuration / intervalTime;
            float changeAmount = (highBrightness - lowBrightness) / steps;
            float changeUp = changeAmount;
            float changeDown = -1 * changeAmount;

            while (true)
            {
                if (!animationRunning)
                {
                    break;
                }

                if (brightness <= lowBrightness)
                {
                    ascending = true;
                }
                else if (Math.Abs(brightness - highBrightness) < 0.001)
                {
                    ascending = false;
                }

                brightness += (ascending) ? changeUp : changeDown;

                if (brightness < 0)
                {
                    brightness = 0;
                }
                else
                if (brightness > 1)
                {
                    brightness = 1;
                }

                SetColor(color, brightness);

                // TODO: what is this 80 ms delay? shouldn't it be intervalTime?
                //await Task.Delay(80);
                Thread.Sleep(intervalTime);
            }
        }
        PwmAnimationParameter animParam;
        struct PwmAnimationParameter
        {
            public Color color;
          

            public uint pulseDuration;
            public uint onDuration;
            public uint offDuration;
            public float highBrightness;
            public float lowBrightness;
        }
    }
}
