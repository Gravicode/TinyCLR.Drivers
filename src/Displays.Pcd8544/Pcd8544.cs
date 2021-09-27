using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
using Meadow.TinyCLR.Core;
using System;

namespace Meadow.TinyCLR.Displays
{
    public class Pcd8544 : DisplayBase
    {
        protected byte[] displayBuffer;
        public static int DEFAULT_SPEED = 4000;

        public override DisplayColorMode ColorMode => DisplayColorMode.Format1bpp;

        public override uint Height => 48;

        public override uint Width => 84;

        public bool InvertDisplay
        {
            get { return _invertDisplay; }
            set { Invert(value); }
        }
        protected bool _invertDisplay = false;

        protected Color currentPen = Color.White;

        protected GpioPin dataCommandPort;
        protected GpioPin resetPort;
        protected GpioPin chipSelectPort;
        //protected ISpiPeripheral spiDisplay;
        protected SpiDevice spi;

        protected byte[] spiBuffer;
        protected readonly byte[] spiReceive;

        public Pcd8544(string SpiControllerName, int chipSelectPin, int dcPin, int resetPin)
        {
            displayBuffer = new byte[Width * Height / 8];
            spiBuffer = new byte[Width * Height / 8];
            spiReceive = new byte[Width * Height / 8];

            var cs = GHIElectronics.TinyCLR.Devices.Gpio.GpioController.GetDefault().
    OpenPin(chipSelectPin);
            var settings = new SpiConnectionSettings()
            {
                ChipSelectType = SpiChipSelectType.Gpio,
                ChipSelectLine = cs,
                Mode = SpiMode.Mode1,
                ClockFrequency = 4_000_000,
            };

            var controller = SpiController.FromName(SpiControllerName);
            spi = controller.GetDevice(settings);

            var dataCommandPort = GHIElectronics.TinyCLR.Devices.Gpio.GpioController.GetDefault().
    OpenPin(dcPin);
            var resetPort = GHIElectronics.TinyCLR.Devices.Gpio.GpioController.GetDefault().
    OpenPin(resetPin);
            var chipSelectPort = GHIElectronics.TinyCLR.Devices.Gpio.GpioController.GetDefault().
    OpenPin(chipSelectPin);

            dataCommandPort.SetDriveMode(GpioPinDriveMode.Output);
            resetPort.SetDriveMode(GpioPinDriveMode.Output);
            dataCommandPort.Write(GpioPinValue.High);
            resetPort.Write(GpioPinValue.High);



            //spiDisplay = new SpiPeripheral(SpiDevice, chipSelectPort);

            Initialize();
        }

        private void Initialize()
        {
            resetPort.Write(GpioPinValue.Low);
            resetPort.Write(GpioPinValue.High);

            dataCommandPort.Write(GpioPinValue.Low);

            spi.Write(new byte[]
            {
                0x21, // LCD Extended Commands.
                0xBF, // Set LCD Vop (Contrast). //0xB0 for 5V, 0XB1 for 3.3v, 0XBF if screen too dark
                0x04, // Set Temp coefficient. //0x04
                0x14, // LCD bias mode 1:48. //0x13 or 0X14
                0x0D, // LCD in normal mode. 0x0d for inverse
                0x20, // We must send 0x20 before modifying the display control mode
                0x0C // Set display control, normal mode. 0x0D for inverse, 0x0C for normal
            });

            dataCommandPort.Write(GpioPinValue.High);

            Clear();
            Show();
        }


        /// <summary>
        ///     Clear the display
        /// </summary>
        /// <remarks>
        ///     Clears the internal memory buffer 
        /// </remarks>
        /// <param name="updateDisplay">If true, it will force a display update</param>
        public override void Clear(bool updateDisplay = false)
        {
            spiBuffer = new byte[Width * Height / 8];

            for (int i = 0; i < spiBuffer.Length; i++)
            {
                spiBuffer[i] = 0;
            }

            displayBuffer = new byte[Width * Height / 8];

            for (int i = 0; i < displayBuffer.Length; i++)
            {
                displayBuffer[i] = 0;
            }


            if (updateDisplay)
            {
                Show();
            }

        }

        public override void SetPenColor(Color pen)
        {
            currentPen = pen;
        }

        /// <summary>
        ///     Coordinates start with index 0
        /// </summary>
        /// <param name="x">Abscissa of the pixel to the set / reset.</param>
        /// <param name="y">Ordinate of the pixel to the set / reset.</param>
        public override void DrawPixel(int x, int y)
        {
            DrawPixel(x, y, currentPen);
        }

        /// <summary>
        ///     Coordinates start with index 0
        /// </summary>
        /// <param name="x">Abscissa of the pixel to the set / reset.</param>
        /// <param name="y">Ordinate of the pixel to the set / reset.</param>
        /// <param name="colored">True = turn on pixel, false = turn off pixel</param>
        public override void DrawPixel(int x, int y, bool colored)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return; // out of the range! return true to indicate failure.

            ushort index = (ushort)((x % 84) + (int)(y * 0.125) * 84);

            byte bitMask = (byte)(1 << (y % 8));

            if (colored)
            {
                spiBuffer[index] |= bitMask;
            }
            else
            {
                spiBuffer[index] &= (byte)~bitMask;
            }
        }

        /// <summary>
        ///     Coordinates start with index 0
        /// </summary>
        /// <param name="x">Abscissa of the pixel to the set / reset.</param>
        /// <param name="y">Ordinate of the pixel to the set / reset.</param>
        /// <param name="color">any value other than black will make the pixel visible</param>
        public override void DrawPixel(int x, int y, Color color)
        {
            var colored = color != Color.Black;

            DrawPixel(x, y, colored);
        }

        public override void Show()
        {
            //  spiDisplay.WriteBytes(spiBuffer);

            spi.TransferFullDuplex(spiBuffer, spiReceive);
            //chipSelectPort, ChipSelectMode.ActiveLow,
        }
        public override void InvertPixel(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            { return; } // out of the range! return true to indicate failure.

            ushort index = (ushort)((x % 84) + (int)(y * 0.125) * 84);

            byte bitMask = (byte)(1 << (y % 8));

            displayBuffer[index] = (displayBuffer[index] ^= bitMask);
        }

        private void Invert(bool inverse)
        {
            _invertDisplay = inverse;
            dataCommandPort.Write(GpioPinValue.Low);
            spi.Write(inverse ? new byte[] { 0x0D } : new byte[] { 0x0C });
            dataCommandPort.Write(GpioPinValue.High);
        }
    }
}