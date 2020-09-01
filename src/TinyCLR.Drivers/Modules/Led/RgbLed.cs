using GHIElectronics.TinyCLR.Devices.Gpio;
using System;
using System.Collections;
using System.Text;
using System.Threading;
using TinyCLR.Drivers.Interface;

namespace TinyCLR.Drivers.Modules.Led
{
    /// <summary>
    /// Represents an RGB LED
    /// </summary>
    public partial class RgbLed : IRgbLed
    {
        protected bool animationRunning;

        /// <summary>
        /// Get the color the LED has been set to.
        /// </summary>
        public Colors Color { get; protected set; } = Colors.White;

        /// <summary>
        /// Get the red LED port
        /// </summary>
        public GpioPin RedPort { get; protected set; }
        /// <summary>
        /// Get the green LED port
        /// </summary>
        public GpioPin GreenPort { get; protected set; }
        /// <summary>
        /// Get the blue LED port
        /// </summary>
        public GpioPin BluePort { get; protected set; }

        /// <summary>
        /// Is the LED using a common cathode
        /// </summary>
        public CommonType Common { get; protected set; }

        /// <summary>
        /// Turns on LED with current color or turns it off
        /// </summary>
        public bool IsOn
        {
            get => isOn;
            set
            {
                SetColor(value ? Color : Colors.Black);
                isOn = value;
            }
        }
        protected bool isOn;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Meadow.Foundation.Leds.RgbLed"/> class.
        /// </summary>
        /// <param name="device">IO Device</param>
        /// <param name="redPin">Red Pin</param>
        /// <param name="greenPin">Green Pin</param>
        /// <param name="bluePin">Blue Pin</param>
        /// <param name="commonType">Is Common Cathode</param>
        public RgbLed(
            
            int redPin,
            int greenPin,
            int bluePin,
            CommonType commonType = CommonType.CommonCathode) 
          
               
        {
           
            var gpio = GpioController.GetDefault();
            this.RedPort = gpio.OpenPin(redPin);
            this.RedPort.SetDriveMode(GpioPinDriveMode.Output);
            this.RedPort.Write(GpioPinValue.Low);

            this.GreenPort = gpio.OpenPin(greenPin);
            this.GreenPort.SetDriveMode(GpioPinDriveMode.Output);
            this.GreenPort.Write(GpioPinValue.Low);

            this.BluePort = gpio.OpenPin(bluePin);
            this.BluePort.SetDriveMode(GpioPinDriveMode.Output);
            this.BluePort.Write(GpioPinValue.Low);

            Common = commonType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Meadow.Foundation.Leds.RgbLed"/> class.
        /// </summary>
        /// <param name="redPort">Red Port</param>
        /// <param name="greenPort">Green Port</param>
        /// <param name="bluePort">Blue Port</param>
        /// <param name="commonType">Is Common Cathode</param>
        public RgbLed(
            GpioPin redPort,
            GpioPin greenPort,
            GpioPin bluePort,
            CommonType commonType = CommonType.CommonCathode)
        {
            RedPort = redPort;
            GreenPort = greenPort;
            BluePort = bluePort;
            Common = commonType;
        }

        /// <summary>
        /// Stops any running animations.
        /// </summary>
        public void Stop()
        {
            animationRunning = false;
            IsOn = false;
        }

        private GpioPinValue GetPinValue(bool State)
        {
            return State ? GpioPinValue.High : GpioPinValue.Low;
        }

        /// <summary>
        /// Sets the current color of the LED.
        /// </summary>
        /// <param name="color"></param>
        public void SetColor(Colors color)
        {
            Color = color;

            bool onState = (Common == CommonType.CommonCathode);

            switch (color)
            {
                case Colors.Red:
                    RedPort.Write(GetPinValue(onState));
                    GreenPort.Write(GetPinValue(!onState));
                    BluePort.Write(GetPinValue(!onState));
                    break;
                case Colors.Green:
                    RedPort.Write(GetPinValue(!onState));
                    GreenPort.Write(GetPinValue(onState));
                    BluePort.Write(GetPinValue(!onState));
                    break;
                case Colors.Blue:
                    RedPort.Write(GetPinValue(!onState));
                    GreenPort.Write(GetPinValue(!onState));
                    BluePort.Write(GetPinValue(onState));
                    break;
                case Colors.Yellow:
                    RedPort.Write(GetPinValue(onState));
                    GreenPort.Write(GetPinValue(onState));
                    BluePort.Write(GetPinValue(!onState));
                    break;
                case Colors.Magenta:
                    RedPort.Write(GetPinValue(onState));
                    GreenPort.Write(GetPinValue(!onState));
                    BluePort.Write(GetPinValue(onState));
                    break;
                case Colors.Cyan:
                    RedPort.Write(GetPinValue(!onState));
                    GreenPort.Write(GetPinValue(onState));
                    BluePort.Write(GetPinValue(onState));
                    break;
                case Colors.White:
                    RedPort.Write(GetPinValue(onState));
                    GreenPort.Write(GetPinValue(onState));
                    BluePort.Write(GetPinValue(onState));
                    break;
                case Colors.Black:
                    RedPort.Write(GetPinValue(!onState));
                    GreenPort.Write(GetPinValue(!onState));
                    BluePort.Write(GetPinValue(!onState));
                    break;
            }
        }

        /// <summary>
        /// Starts the blink animation.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="onDuration"></param>
        /// <param name="offDuration"></param>
        public void StartBlink(Colors color, uint onDuration = 200, uint offDuration = 200)
        {
            Stop();
            animationParam = new RgbAnimationParam();
            animationParam.color = color;
            animationParam.onDuration = onDuration;
            animationParam.offDuration = offDuration;
            var animationTask = new Thread(new  ThreadStart (StartBlinkAsync));
            animationRunning = true;

            animationTask.Start();
        }

        protected void StartBlinkAsync()
        {
            var color = animationParam.color;
            var onDuration = animationParam.onDuration;
            var offDuration = animationParam.offDuration;
            while (true)
            {
                if (!animationRunning)
                {
                    break;
                }

                SetColor(color);
                Thread.Sleep((int)onDuration);
                SetColor(Colors.Black);
                Thread.Sleep((int)offDuration);
            }
        }
        RgbAnimationParam animationParam;
        struct RgbAnimationParam
        {
            public Colors color;
            public uint onDuration;
            public uint offDuration;
        }
    }

    
}
