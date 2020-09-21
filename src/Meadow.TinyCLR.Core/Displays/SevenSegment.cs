using GHIElectronics.TinyCLR.Devices.Gpio;
using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace Meadow.TinyCLR.Core.Displays
{
    public class SevenSegment
    {
        private  GpioPin _portA;
        private  GpioPin _portB;
        private  GpioPin _portC;
        private  GpioPin _portD;
        private  GpioPin _portE;
        private  GpioPin _portF;
        private  GpioPin _portG;
        private  GpioPin _portdouble;
        private  bool _isCommonCathode;
        private byte[,] _segments = new byte[17, 7]
        {
      {
        (byte) 1,
        (byte) 1,
        (byte) 1,
        (byte) 1,
        (byte) 1,
        (byte) 1,
        (byte) 0
      },
      {
        (byte) 0,
        (byte) 1,
        (byte) 1,
        (byte) 0,
        (byte) 0,
        (byte) 0,
        (byte) 0
      },
      {
        (byte) 1,
        (byte) 1,
        (byte) 0,
        (byte) 1,
        (byte) 1,
        (byte) 0,
        (byte) 1
      },
      {
        (byte) 1,
        (byte) 1,
        (byte) 1,
        (byte) 1,
        (byte) 0,
        (byte) 0,
        (byte) 1
      },
      {
        (byte) 0,
        (byte) 1,
        (byte) 1,
        (byte) 0,
        (byte) 0,
        (byte) 1,
        (byte) 1
      },
      {
        (byte) 1,
        (byte) 0,
        (byte) 1,
        (byte) 1,
        (byte) 0,
        (byte) 1,
        (byte) 1
      },
      {
        (byte) 1,
        (byte) 0,
        (byte) 1,
        (byte) 1,
        (byte) 1,
        (byte) 1,
        (byte) 1
      },
      {
        (byte) 1,
        (byte) 1,
        (byte) 1,
        (byte) 0,
        (byte) 0,
        (byte) 0,
        (byte) 0
      },
      {
        (byte) 1,
        (byte) 1,
        (byte) 1,
        (byte) 1,
        (byte) 1,
        (byte) 1,
        (byte) 1
      },
      {
        (byte) 1,
        (byte) 1,
        (byte) 1,
        (byte) 1,
        (byte) 0,
        (byte) 1,
        (byte) 1
      },
      {
        (byte) 1,
        (byte) 1,
        (byte) 1,
        (byte) 0,
        (byte) 1,
        (byte) 1,
        (byte) 1
      },
      {
        (byte) 0,
        (byte) 0,
        (byte) 1,
        (byte) 1,
        (byte) 1,
        (byte) 1,
        (byte) 1
      },
      {
        (byte) 1,
        (byte) 0,
        (byte) 0,
        (byte) 1,
        (byte) 1,
        (byte) 1,
        (byte) 0
      },
      {
        (byte) 0,
        (byte) 1,
        (byte) 1,
        (byte) 1,
        (byte) 1,
        (byte) 0,
        (byte) 1
      },
      {
        (byte) 1,
        (byte) 0,
        (byte) 0,
        (byte) 1,
        (byte) 1,
        (byte) 1,
        (byte) 1
      },
      {
        (byte) 1,
        (byte) 0,
        (byte) 0,
        (byte) 0,
        (byte) 1,
        (byte) 1,
        (byte) 1
      },
      {
        (byte) 0,
        (byte) 0,
        (byte) 0,
        (byte) 0,
        (byte) 0,
        (byte) 0,
        (byte) 0
      }
        };

        private SevenSegment()
        {
        }

        public SevenSegment(
      
          int pinA,
          int pinB,
          int pinC,
          int pinD,
          int pinE,
          int pinF,
          int pinG,
          int pindouble,
          bool isCommonCathode)
         
        {
            // : this(device.CreateDigitalOutputPort(pinA), device.CreateDigitalOutputPort(pinB), device.CreateDigitalOutputPort(pinC), device.CreateDigitalOutputPort(pinD), device.CreateDigitalOutputPort(pinE), device.CreateDigitalOutputPort(pinF), device.CreateDigitalOutputPort(pinG), device.CreateDigitalOutputPort(pindouble), isCommonCathode)
            var gpio = GpioController.GetDefault();
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

            var Pindouble = gpio.OpenPin(pindouble);
            Pindouble.SetDriveMode(GpioPinDriveMode.Output);

            Setup(PinA,
          PinB,
          PinC,
          PinD,
          PinE,
          PinF,
          PinG,
          Pindouble,
           isCommonCathode);
        }

        public SevenSegment(
          GpioPin portA,
          GpioPin portB,
          GpioPin portC,
          GpioPin portD,
          GpioPin portE,
          GpioPin portF,
          GpioPin portG,
          GpioPin portdouble,
          bool isCommonCathode)
        {
            Setup( portA,
          portB,
          portC,
          portD,
          portE,
          portF,
          portG,
          portdouble,
           isCommonCathode);
        }
        void Setup(GpioPin portA,
          GpioPin portB,
          GpioPin portC,
          GpioPin portD,
          GpioPin portE,
          GpioPin portF,
          GpioPin portG,
          GpioPin portdouble,
          bool isCommonCathode)
        {
            this._portA = portA;
            this._portB = portB;
            this._portC = portC;
            this._portD = portD;
            this._portE = portE;
            this._portF = portF;
            this._portG = portG;
            this._portdouble = portdouble;
            this._isCommonCathode = isCommonCathode;
        }
        public void SetDisplay(SevenSegment.CharacterType character, bool showdouble = false)
        {
            this._portdouble.Write((this._isCommonCathode ? showdouble : !showdouble) ? GpioPinValue.High : GpioPinValue.Low);
            int index = (int)character;
            this._portA.Write((this.IsEnabled(this._segments[index, 0])) ? GpioPinValue.High : GpioPinValue.Low );
            this._portB.Write((this.IsEnabled(this._segments[index, 1])) ? GpioPinValue.High : GpioPinValue.Low );
            this._portC.Write((this.IsEnabled(this._segments[index, 2])) ? GpioPinValue.High : GpioPinValue.Low );
            this._portD.Write((this.IsEnabled(this._segments[index, 3])) ? GpioPinValue.High : GpioPinValue.Low );
            this._portE.Write((this.IsEnabled(this._segments[index, 4])) ? GpioPinValue.High : GpioPinValue.Low );
            this._portF.Write((this.IsEnabled(this._segments[index, 5])) ? GpioPinValue.High : GpioPinValue.Low );
            this._portG.Write((this.IsEnabled(this._segments[index, 6])) ? GpioPinValue.High : GpioPinValue.Low );
        }

        private bool IsEnabled(byte value) => !this._isCommonCathode ? value == (byte)0 : value == (byte)1;

        public void SetDisplay(char character, bool showdouble = false)
        {
            SevenSegment.CharacterType character1;
            if (character == ' ')
                character1 = SevenSegment.CharacterType.Blank;
            else if (character >= '0' && character <= '9')
            {
                character1 = (SevenSegment.CharacterType)((int)character - 48);
            }
            else
            {
                if (character < 'a' || character > 'f')
                    throw new ArgumentOutOfRangeException();
                character1 = (SevenSegment.CharacterType)((int)character - 97 + 10);
            }
            this.SetDisplay(character1, showdouble);
        }

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
            count,
        }
    }
}
