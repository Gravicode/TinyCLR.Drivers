using GHIElectronics.TinyCLR.Devices.Gpio;
using System;

namespace Meadow.TinyCLR.Displays.Led
{
    /// <summary>
    /// Seven Segment Display
    /// </summary>
    public class SevenSegment
    {
        #region Enums
        /// <summary>
        /// Valid Characters to display
        /// </summary>
        public enum CharacterType
        {
            Zero,
            One,
            Two,
            Three,
            Four,
            Five,
            Six,
            Seven,
            Eight,
            Nine,
            A,
            B,
            C,
            D,
            E,
            F,
            Blank,
            count
        }

        #endregion

        #region Member variables / fields

        private  GpioPin _portA;
        private  GpioPin _portB;
        private  GpioPin _portC;
        private  GpioPin _portD;
        private  GpioPin _portE;
        private  GpioPin _portF;
        private  GpioPin _portG;
        private  GpioPin _portDecimal;

        private  bool _isCommonCathode;

        private byte[,] _segments =
        {
             {1, 1, 1, 1, 1, 1, 0}, //0
             {0, 1, 1, 0, 0, 0, 0}, //1
             {1, 1, 0, 1, 1, 0, 1}, //2
             {1, 1, 1, 1, 0, 0, 1}, //3
             {0, 1, 1, 0, 0, 1, 1}, //4
             {1, 0, 1, 1, 0, 1, 1}, //5
             {1, 0, 1, 1, 1, 1, 1}, //6
             {1, 1, 1, 0, 0, 0, 0}, //7
             {1, 1, 1, 1, 1, 1, 1}, //8
             {1, 1, 1, 1, 0, 1, 1}, //9
             {1, 1, 1, 0, 1, 1, 1}, //A
             {0, 0, 1, 1, 1, 1, 1}, //b
             {1, 0, 0, 1, 1, 1, 0}, //C
             {0, 1, 1, 1, 1, 0, 1}, //d
             {1, 0, 0, 1, 1, 1, 1}, //E
             {1, 0, 0, 0, 1, 1, 1}, //F
             {0, 0, 0, 0, 0, 0, 0}, //blank
        };

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
        public SevenSegment(
          
            int pinA, int pinB,
            int pinC, int pinD,
            int pinE, int pinF,
            int pinG, int pinDecimal,
            bool isCommonCathode) 
             {
            /*
             this(device.CreateDigitalOutputPort(pinA),
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
            var PinDigit1 = gpio.OpenPin(pinA);
         

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
            Setup(PinA, PinB,
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
        public SevenSegment(
            GpioPin portA, GpioPin portB,
            GpioPin portC, GpioPin portD,
            GpioPin portE, GpioPin portF,
            GpioPin portG, GpioPin portDecimal, 
            bool isCommonCathode)
        {
            Setup( portA,  portB,
             portC,  portD,
             portE,  portF,
             portG,  portDecimal,
             isCommonCathode);
        }
        void Setup(GpioPin portA, GpioPin portB,
            GpioPin portC, GpioPin portD,
            GpioPin portE, GpioPin portF,
            GpioPin portG, GpioPin portDecimal,
            bool isCommonCathode)
        {
            _portA = portA;
            _portB = portB;
            _portC = portC;
            _portD = portD;
            _portE = portE;
            _portF = portF;
            _portG = portG;
            _portDecimal = portDecimal;

            _isCommonCathode = isCommonCathode;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Displays the especified character
        /// </summary>
        /// <param name="character"></param>
        /// <param name="showDecimal"></param>
        public void SetDisplay(CharacterType character, bool showDecimal = false)
        {
            _portDecimal.Write((_isCommonCathode ? showDecimal : !showDecimal)? GpioPinValue.High: GpioPinValue.Low);

            int index = (int)character;

            _portA.Write(IsEnabled(_segments[index, 0]) ? GpioPinValue.High : GpioPinValue.Low);
            _portB.Write(IsEnabled(_segments[index, 1]) ? GpioPinValue.High : GpioPinValue.Low);
            _portC.Write(IsEnabled(_segments[index, 2]) ? GpioPinValue.High : GpioPinValue.Low);
            _portD.Write(IsEnabled(_segments[index, 3]) ? GpioPinValue.High : GpioPinValue.Low);
            _portE.Write(IsEnabled(_segments[index, 4]) ? GpioPinValue.High : GpioPinValue.Low);
            _portF.Write(IsEnabled(_segments[index, 5]) ? GpioPinValue.High : GpioPinValue.Low);
            _portG.Write(IsEnabled(_segments[index, 6]) ? GpioPinValue.High : GpioPinValue.Low);
        }

        /// <summary>
        /// Returns 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        bool IsEnabled (byte value)
        {
            return _isCommonCathode ? value == 1 : value == 0;
        }

        /// <summary>
        /// Displays the especified valid character
        /// </summary>
        /// <param name="character"></param>
        /// <param name="showDecimal"></param>
        public void SetDisplay(char character, bool showDecimal = false)
        {
            CharacterType charType;

            if (character == ' ')
                charType = CharacterType.Blank;
            else if (character >= '0' && character <= '9')
                charType = (CharacterType)(character - '0');
            else if (character >= 'a' && character <= 'f')
                charType = (CharacterType)(character - 'a' + 10);
            else
                throw new ArgumentOutOfRangeException();

            SetDisplay(charType, showDecimal);
        }

        #endregion
    }
}