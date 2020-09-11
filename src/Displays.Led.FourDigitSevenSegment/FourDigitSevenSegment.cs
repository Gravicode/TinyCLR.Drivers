using GHIElectronics.TinyCLR.Devices.Gpio;
using Meadow.TinyCLR.Core.Displays;
using System;
using System.Threading;

namespace Meadow.TinyCLR.Displays.Led
{
    /// <summary>
    /// Four Digit Seven Segment Display
    /// </summary>
    public class FourDigitSevenSegment
    {
        protected bool IsAnimating = false;
        

        #region Member variables / fields

        protected GpioPin[] digits;

        protected SevenSegment[] sevenSegments;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a SevenSegment connected to the especified ints to a IODevice
        /// </summary>
        /// <param name="device"></param>
        /// <param name="pinA"></param>
        /// <param name="pinB"></param>
        /// <param name="pinC"></param>
        /// <param name="pinD"></param>
        /// <param name="pinE"></param>
        /// <param name="pinF"></param>
        /// <param name="pinG"></param>
        /// <param name="pinDecimal"></param>
        /// <param name="isCommonCathode"></param>
        public FourDigitSevenSegment(
          
            int pinDigit1, int pinDigit2,
            int pinDigit3, int pinDigit4,
            int pinA, int pinB,
            int pinC, int pinD,
            int pinE, int pinF,
            int pinG, int pinDecimal,
            bool isCommonCathode) 
             {
            /*
             this (
                    device.CreateDigitalOutputPort(pinDigit1),
                    device.CreateDigitalOutputPort(pinDigit2),
                    device.CreateDigitalOutputPort(pinDigit3),
                    device.CreateDigitalOutputPort(pinDigit4),
                    device.CreateDigitalOutputPort(pinA),
                    device.CreateDigitalOutputPort(pinB),
                    device.CreateDigitalOutputPort(pinC),
                    device.CreateDigitalOutputPort(pinD),
                    device.CreateDigitalOutputPort(pinE),
                    device.CreateDigitalOutputPort(pinF),
                    device.CreateDigitalOutputPort(pinG),
                    device.CreateDigitalOutputPort(pinDecimal),
                    isCommonCathode)
             */
            var gpio = GpioController.GetDefault();
            var PinDigit1 = gpio.OpenPin(pinDigit1);
            PinDigit1.SetDriveMode(GpioPinDriveMode.Output);

            var PinDigit2 = gpio.OpenPin(pinDigit2);
            PinDigit2.SetDriveMode(GpioPinDriveMode.Output);

            var PinDigit3 = gpio.OpenPin(pinDigit3);
            PinDigit3.SetDriveMode(GpioPinDriveMode.Output);

            var PinDigit4 = gpio.OpenPin(pinDigit4);
            PinDigit4.SetDriveMode(GpioPinDriveMode.Output);

            var PinA = gpio.OpenPin(pinA);
            PinA.SetDriveMode(GpioPinDriveMode.Output);

            var PinB = gpio.OpenPin(pinB);
            PinB.SetDriveMode(GpioPinDriveMode.Output);

            var PinC = gpio.OpenPin(pinC);
            PinC.SetDriveMode(GpioPinDriveMode.Output);

            var PinD = gpio.OpenPin(pinD);
            PinD.SetDriveMode(GpioPinDriveMode.Output);

            var PinE = gpio.OpenPin(pinE);
            PinE.SetDriveMode(GpioPinDriveMode.Output);

            var PinF = gpio.OpenPin(pinF);
            PinF.SetDriveMode(GpioPinDriveMode.Output);

            var PinG = gpio.OpenPin(pinG);
            PinG.SetDriveMode(GpioPinDriveMode.Output);

            var PinDecimal = gpio.OpenPin(pinDecimal);
            PinDecimal.SetDriveMode(GpioPinDriveMode.Output);

            Setup( PinDigit1,  PinDigit2,
             PinDigit3,  PinDigit4,
             PinA, PinB,
             PinC, PinD,
             PinE, PinF,
             PinG, PinDecimal,
             isCommonCathode);
        }

        /// <summary>
        /// Creates a SevenSegment connected to the especified GpioPins
        /// </summary>
        /// <param name="portA"></param>
        /// <param name="portB"></param>
        /// <param name="portC"></param>
        /// <param name="portD"></param>
        /// <param name="portE"></param>
        /// <param name="portF"></param>
        /// <param name="portG"></param>
        /// <param name="portDecimal"></param>
        /// <param name="isCommonCathode"></param>
        public FourDigitSevenSegment(
            GpioPin portDigit1, GpioPin portDigit2,
            GpioPin portDigit3, GpioPin portDigit4,
            GpioPin portA, GpioPin portB,
            GpioPin portC, GpioPin portD,
            GpioPin portE, GpioPin portF,
            GpioPin portG, GpioPin portDecimal, 
            bool isCommonCathode)
        {
           Setup( portDigit1,  portDigit2,
             portDigit3,  portDigit4,
             portA,  portB,
             portC,  portD,
             portE,  portF,
             portG,  portDecimal,
             isCommonCathode);
        }
        void Setup(GpioPin portDigit1, GpioPin portDigit2,
            GpioPin portDigit3, GpioPin portDigit4,
            GpioPin portA, GpioPin portB,
            GpioPin portC, GpioPin portD,
            GpioPin portE, GpioPin portF,
            GpioPin portG, GpioPin portDecimal,
            bool isCommonCathode)
        {
            AnimationInfo = new Animation();
            digits = new GpioPin[4];
            digits[0] = portDigit1;
            digits[1] = portDigit2;
            digits[2] = portDigit3;
            digits[3] = portDigit4;

            sevenSegments = new SevenSegment[4];
            for (int i = 0; i < 4; i++)
            {
                sevenSegments[i] = new SevenSegment(portA, portB, portC, portD, portE, portF, portG, portDecimal, isCommonCathode);
            }


        }
        #endregion

        #region Methods

        /// <summary>
        /// Displays the especified valid character
        /// </summary>
        /// <param name="character"></param>
        /// <param name="showDecimal"></param>
        public void SetDisplay(char[] character, bool showDecimal = false)
        {
            if (IsAnimating)
            {
                return;
            }
            AnimationInfo.character = character;
            AnimationInfo.showDecimal = showDecimal;
            IsAnimating = true;
            var tsk = new Thread(new ThreadStart(StartDisplayLoop));
            tsk.Start();
            //Task.Run(async ()=> await StartDisplayLoop(character, showDecimal, cts.Token));
        }

        protected void StartDisplayLoop() 
        {
            var character = AnimationInfo.character;
            var showDecimal = AnimationInfo.showDecimal;
            while (true)
            {
                if (!IsAnimating)
                {
                    break;
                }

                for (int i = 0; i < 4; i++)
                {
                    sevenSegments[i].SetDisplay(character[i], showDecimal);

                    digits[i].Write( GpioPinValue.Low);
                    digits[i].Write( GpioPinValue.High);
                }

                Thread.Sleep(7);
            }
        }

        public void Stop()
        {
            IsAnimating = false;
        }

        #endregion
        public Animation AnimationInfo { get; set; }
        public class Animation
        {
            public char[] character;
            public bool showDecimal;
        }
    }

   
}