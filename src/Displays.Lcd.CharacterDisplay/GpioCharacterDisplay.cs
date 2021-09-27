using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Pwm;
using Meadow.TinyCLR.Core.Interface;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Meadow.TinyCLR.Displays.Lcd
{
    public class GpioCharacterDisplay : ICharacterDisplay
    {
        private byte LCD_LINE_1 = 0x80; // # LCD RAM address for the 1st line
        private byte LCD_LINE_2 = 0xC0; // # LCD RAM address for the 2nd line
        private byte LCD_LINE_3 = 0x94; // # LCD RAM address for the 3rd line
        private byte LCD_LINE_4 = 0xD4; // # LCD RAM address for the 4th line

        private byte cursorLine = 0;
        private byte cursorColumn = 0;

        private const byte LCD_SETDDRAMADDR = 0x80;
        private const byte LCD_SETCGRAMADDR = 0x40;

        protected PwmChannel LCD_V0;
        protected GpioPin LCD_E;
        protected GpioPin LCD_RS;
        protected GpioPin LCD_D4;
        protected GpioPin LCD_D5;
        protected GpioPin LCD_D6;
        protected GpioPin LCD_D7;
        protected GpioPin LED_ON;

        private bool LCD_INSTRUCTION = false;
        private bool LCD_DATA = true;
        private static object _lock = new object();

        public TextDisplayConfig DisplayConfig { get; protected set; }

        public GpioCharacterDisplay(

            int pinRS,
            int pinE,
            int pinD4,
            int pinD5,
            int pinD6,
            int pinD7,
            ushort rows = 4, ushort columns = 20)
        {
            /*
             this ( 
                    device.CreateDigitalOutputPort(pinRS), 
                    device.CreateDigitalOutputPort(pinE), 
                    device.CreateDigitalOutputPort(pinD4),
                    device.CreateDigitalOutputPort(pinD5),
                    device.CreateDigitalOutputPort(pinD6),
                    device.CreateDigitalOutputPort(pinD7),
                    rows, columns) 
             */
            var gpio = GpioController.GetDefault();

            var DpinRS = gpio.OpenPin(pinRS);
            DpinRS.SetDriveMode(GpioPinDriveMode.Output);

            var DpinE = gpio.OpenPin(pinE);
            DpinE.SetDriveMode(GpioPinDriveMode.Output);

            var DpinD4 = gpio.OpenPin(pinD4);
            DpinD4.SetDriveMode(GpioPinDriveMode.Output);

            var DpinD5 = gpio.OpenPin(pinD5);
            DpinD5.SetDriveMode(GpioPinDriveMode.Output);

            var DpinD6 = gpio.OpenPin(pinD6);
            DpinD6.SetDriveMode(GpioPinDriveMode.Output);

            var DpinD7 = gpio.OpenPin(pinD7);
            DpinD7.SetDriveMode(GpioPinDriveMode.Output);

            setDisplay(DpinRS, DpinE, DpinD4, DpinD5, DpinD6, DpinD7, rows, columns);
        }

        public GpioCharacterDisplay(
            GpioPin portRS,
            GpioPin portE,
            GpioPin portD4,
            GpioPin portD5,
            GpioPin portD6,
            GpioPin portD7,
            ushort rows = 4, ushort columns = 20)
        {
            setDisplay(portRS,
             portE,
             portD4,
             portD5,
             portD6,
             portD7,
             rows, columns);
        }

        void setDisplay(
            GpioPin portRS,
            GpioPin portE,
            GpioPin portD4,
            GpioPin portD5,
            GpioPin portD6,
            GpioPin portD7,
            ushort rows = 4, ushort columns = 20)
        {
            DisplayConfig = new TextDisplayConfig { Height = rows, Width = columns };

            LCD_RS = portRS;
            LCD_E = portE;
            LCD_D4 = portD4;
            LCD_D5 = portD5;
            LCD_D6 = portD6;
            LCD_D7 = portD7;

            Initialize();
        }

        public GpioCharacterDisplay(
            string PwmControllerName,
            int pinV0,
            int pinRS,
            int pinE,
            int pinD4,
            int pinD5,
            int pinD6,
            int pinD7,
            ushort rows = 4, ushort columns = 20)

        {
            /*
             this(
                device.CreatePwmPort(pinV0, 100, 0.5f, true),
                device.CreateDigitalOutputPort(pinRS),
                device.CreateDigitalOutputPort(pinE),
                device.CreateDigitalOutputPort(pinD4),
                device.CreateDigitalOutputPort(pinD5),
                device.CreateDigitalOutputPort(pinD6),
                device.CreateDigitalOutputPort(pinD7),
                rows, columns)
             */
            var controller = PwmController.FromName(PwmControllerName);
            var pwmPort = controller.OpenChannel(pinV0);
            pwmPort.Controller.SetDesiredFrequency(100);
            pwmPort.SetActiveDutyCyclePercentage(0.5f);
            bool inverted = true;
            pwmPort.Polarity = inverted ? PwmPulsePolarity.ActiveHigh : PwmPulsePolarity.ActiveLow;
            var gpio = GpioController.GetDefault();

            var DpinRS = gpio.OpenPin(pinRS);
            DpinRS.SetDriveMode(GpioPinDriveMode.Output);

            var DpinE = gpio.OpenPin(pinE);
            DpinE.SetDriveMode(GpioPinDriveMode.Output);

            var DpinD4 = gpio.OpenPin(pinD4);
            DpinD4.SetDriveMode(GpioPinDriveMode.Output);

            var DpinD5 = gpio.OpenPin(pinD5);
            DpinD5.SetDriveMode(GpioPinDriveMode.Output);

            var DpinD6 = gpio.OpenPin(pinD6);
            DpinD6.SetDriveMode(GpioPinDriveMode.Output);

            var DpinD7 = gpio.OpenPin(pinD7);
            DpinD7.SetDriveMode(GpioPinDriveMode.Output);

            setDisplay(pwmPort, DpinRS, DpinE, DpinD4, DpinD5, DpinD6, DpinD7, rows, columns);

        }

        public GpioCharacterDisplay(
            PwmChannel portV0,
            GpioPin portRS,
            GpioPin portE,
            GpioPin portD4,
            GpioPin portD5,
            GpioPin portD6,
            GpioPin portD7,
            ushort rows = 4, ushort columns = 20)
        {
            setDisplay(portV0,
             portRS,
             portE,
             portD4,
             portD5,
             portD6,
             portD7,
             rows, columns);
        }
        void setDisplay(PwmChannel portV0,
            GpioPin portRS,
            GpioPin portE,
            GpioPin portD4,
            GpioPin portD5,
            GpioPin portD6,
            GpioPin portD7,
            ushort rows = 4, ushort columns = 20)
        {
            Debug.WriteLine("Constructor with Contrast pin");

            DisplayConfig = new TextDisplayConfig { Height = rows, Width = columns };

            LCD_V0 = portV0; LCD_V0.Start();
            LCD_RS = portRS;
            LCD_E = portE;
            LCD_D4 = portD4;
            LCD_D5 = portD5;
            LCD_D6 = portD6;
            LCD_D7 = portD7;

            Initialize();
        }
        

        private void Initialize()
        {
            SendByte(0x33, LCD_INSTRUCTION); // 110011 Initialise
            SendByte(0x32, LCD_INSTRUCTION); // 110010 Initialise
            SendByte(0x06, LCD_INSTRUCTION); // 000110 Cursor move direction
            SendByte(0x0C, LCD_INSTRUCTION); // 001100 Display On,Cursor Off, Blink Off
            SendByte(0x28, LCD_INSTRUCTION); // 101000 Data length, number of lines, font size
            SendByte(0x01, LCD_INSTRUCTION); // 000001 Clear display
            Thread.Sleep(5);
        }

        private void SendByte(byte value, bool mode)
        {
            lock (_lock)
            {
                LCD_RS.Write((mode) ? GpioPinValue.High : GpioPinValue.Low);

                // high bits
                LCD_D4.Write(((value & 0x10) == 0x10) ? GpioPinValue.High : GpioPinValue.Low);
                LCD_D5.Write(((value & 0x20) == 0x20) ? GpioPinValue.High : GpioPinValue.Low);
                LCD_D6.Write(((value & 0x40) == 0x40) ? GpioPinValue.High : GpioPinValue.Low);
                LCD_D7.Write(((value & 0x80) == 0x80) ? GpioPinValue.High : GpioPinValue.Low);

                ToggleEnable();

                // low bits
                LCD_D4.Write(((value & 0x01) == 0x01) ? GpioPinValue.High : GpioPinValue.Low);
                LCD_D5.Write(((value & 0x02) == 0x02) ? GpioPinValue.High : GpioPinValue.Low);
                LCD_D6.Write(((value & 0x04) == 0x04) ? GpioPinValue.High : GpioPinValue.Low);
                LCD_D7.Write(((value & 0x08) == 0x08) ? GpioPinValue.High : GpioPinValue.Low);

                ToggleEnable();

                Thread.Sleep(5);
            }
        }

        private void ToggleEnable()
        {
            LCD_E.Write(GpioPinValue.Low);
            LCD_E.Write(GpioPinValue.High);
            LCD_E.Write(GpioPinValue.Low);
        }

        private byte GetLineAddress(int line)
        {
            switch (line)
            {
                case 0:
                    return LCD_LINE_1;
                case 1:
                    return LCD_LINE_2;
                case 2:
                    return LCD_LINE_3;
                case 3:
                    return LCD_LINE_4;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void SetLineAddress(int line)
        {
            SendByte(GetLineAddress(line), LCD_INSTRUCTION);
        }
        public static string padRight(string text, int n, string s)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(text);
            var iterate = n - s.Length;
            for (int i = 0; i < iterate; i++)
            {
                sb.Append(s);
            }
            return sb.ToString();
        }
        public void WriteLine(string text, byte lineNumber)
        {
            SetLineAddress(lineNumber);

            // Instead of clearing the line first, pad it with empty space on the end
            var screenText = padRight(text, DisplayConfig.Width, " ");
            if (screenText.Length > DisplayConfig.Width)
            {

                screenText = screenText.Substring(0, DisplayConfig.Width);
            }

            var bytes = Encoding.UTF8.GetBytes(screenText);
            foreach (var b in bytes)
            {
                SendByte(b, LCD_DATA);
            }
        }

        public void Write(string text)
        {
            string screentText = text;

            if (screentText.Length + (int)cursorColumn > DisplayConfig.Width)
            {
                screentText = screentText.Substring(0, DisplayConfig.Width - cursorColumn);
            }

            var bytes = Encoding.UTF8.GetBytes(screentText);
            foreach (var b in bytes)
            {
                SendByte(b, LCD_DATA);
            }
        }

        public void SetCursorPosition(byte column, byte line)
        {
            if (column >= DisplayConfig.Width || line >= DisplayConfig.Height)
            {
                throw new Exception($"GpioCharacterDisplay: cursor out of bounds {column}, {line}");
            }

            cursorColumn = column;
            cursorLine = line;

            byte lineAddress = GetLineAddress(line);
            var address = column + lineAddress;
            SendByte(((byte)(LCD_SETDDRAMADDR | address)), LCD_INSTRUCTION);
        }

        public void ClearLines()
        {
            SendByte(0x01, LCD_INSTRUCTION);
            SetCursorPosition(1, 0);
            Thread.Sleep(5);
        }

        public void ClearLine(byte lineNumber)
        {
            SetLineAddress(lineNumber);

            for (int i = 0; i < DisplayConfig.Width; i++)
            {
                Write(" ");
            }
        }

        public void SetBrightness(float brightness = 0.75F)
        {
            Debug.WriteLine("Set brightness not enabled");
        }

        public void SetContrast(float contrast = 0.5f)
        {
            if (contrast < 0 || contrast > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(contrast), "err: contrast must be between 0 and 1, inclusive.");
            }

            Debug.WriteLine($"Contrast: {contrast}");
            LCD_V0.SetActiveDutyCyclePercentage(contrast);
        }

        public void SaveCustomCharacter(byte[] characterMap, byte address)
        {
            address &= 0x7; // we only have 8 locations 0-7
            SendByte((byte)(LCD_SETCGRAMADDR | (address << 3)), LCD_INSTRUCTION);

            for (var i = 0; i < 8; i++)
            {
                SendByte(characterMap[i], LCD_DATA);
            }
        }

        public void WriteLine(string text, byte lineNumber, bool showCursor = false)
        {
            SetLineAddress(lineNumber);

            // Instead of clearing the line first, pad it with empty space on the end
            var screenText = padRight(text, DisplayConfig.Width, " ");
            if (screenText.Length > DisplayConfig.Width)
            {
                screenText = screenText.Substring(0, DisplayConfig.Width);
            }

            var bytes = Encoding.UTF8.GetBytes(screenText);
            foreach (var b in bytes)
            {
                SendByte(b, LCD_DATA);
            }
        }

        public void Show()
        {
            //can safely ignore
            //required for ITextDisplayMenu
        }
    }
}