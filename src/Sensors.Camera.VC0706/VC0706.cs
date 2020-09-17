using GHIElectronics.TinyCLR.Devices.Uart;
using System;
using System.Diagnostics;
using System.Threading;

namespace Meadow.Foundation.Sensors.Camera
{
    public class Vc0706 : ICamera, IDisposable
    {
        #region enums

        public enum ColorControl : byte
        {
            Automatic,
            Color,
            BlackWhite,
        }

        public enum ImageSize : byte
        {
            Res640x480 = 0x00,
            Res320x240 = 0x11,
            Res160x120 = 0x22,
        }

        public enum ComPortSpeed
        {
            Baud9600 = 0xAEC8,
            Baud19200 = 0x56E4,
            Baud38400 = 0x2AF2,
            Baud57600 = 0x1C4C,
            Baud115200 = 0x0DA6
        }

        public bool MotionDetectionEnabled
        {
            get { return vc0706.GetMotionDetectionStatus(); }
            set { vc0706.SetMotionDetection(value); }
        }

        public bool IsMotionDetected
        {
            get { return vc0706.IsMotionDetected(); }
        }

        #endregion

        private UartController serialPort;

        private Vc0706Core vc0706;

        public Vc0706(string serialPortName, int baudRate)
        {
            //serialPort = device.CreateSerialPort(serialPortName, baudRate);
            serialPort = UartController.FromName(serialPortName);
            
            var uartSetting = new UartSetting()
            {
                BaudRate = baudRate,
                DataBits = 8,
                Parity = UartParity.None,
                StopBits = UartStopBitCount.One,
                Handshaking = UartHandshake.None,
            };

            serialPort.SetActiveSettings(uartSetting);

            serialPort.Enable();

            vc0706 = new Vc0706Core();
            vc0706.BaudRate = baudRate;
            vc0706.PortName = serialPortName;
        }

        void Initialize(string comPort, ComPortSpeed baudRate = ComPortSpeed.Baud38400, ImageSize imageSize = ImageSize.Res640x480)
        {
            DetectBaudRate(comPort);
            SetImageSize(imageSize);
            DetectBaudRate(comPort);
            SetPortSpeed(baudRate);

            vc0706.Initialize(serialPort);
        }

        public void TakePicture(string path)
        {
            vc0706.TakePicture(path);
        }

        protected void DetectBaudRate(string port)
        {
            var supportedBaudRates = new int[] { 115200, 57600, 38400, 19200, 9600 };

            foreach (int rate in supportedBaudRates)
            {
                OpenComPort(port, rate);

                try
                {
                    vc0706.SetComPort(serialPort);
                    GetImageSize();

                    Debug.WriteLine("BaudRate detected: " + rate.ToString());
                    return;
                }
                catch //(Exception e)
                {

                }
                CloseComPort();
            }
            throw new ApplicationException("BaudRate detection failed - is your camera connected?");
        }

        protected void OpenComPort(string port = "COM1", int baudRate = 38400)
        {
            CloseComPort();

          //ToDo  serialPort = new SerialPort(port, baudRate, Parity.None, 8, StopBits.One);

            //serialPort.ReadTimeout = 10 * 2;
            vc0706.ReadTimeout = 10 * 2;
            // serialPort.WriteTimeout = 10 * 2;

            //serialPort.Enable();
            serialPort = UartController.FromName(port);
            var uartSetting = new UartSetting()
            {
                BaudRate = baudRate,
                DataBits = 8,
                Parity = UartParity.None,
                StopBits = UartStopBitCount.One,
                Handshaking = UartHandshake.None,
            };
            vc0706.BaudRate = baudRate;
            serialPort.SetActiveSettings(uartSetting);

            serialPort.Enable();
        }

        protected void SetPortSpeed(ComPortSpeed speed)
        {
            vc0706.SetPortSpeed((short)speed);

            var comPortName = vc0706.PortName;
            switch (speed)
            {
                case ComPortSpeed.Baud9600:
                    OpenComPort(comPortName, 9600);
                    break;
                case ComPortSpeed.Baud19200:
                    OpenComPort(comPortName, 19200);
                    break;
                case ComPortSpeed.Baud38400:
                    OpenComPort(comPortName, 38400);
                    break;
                case ComPortSpeed.Baud57600:
                    OpenComPort(comPortName, 57600);
                    break;
                case ComPortSpeed.Baud115200:
                    OpenComPort(comPortName, 115200);
                    break;
            }
            Thread.Sleep(100);
        }

        public void SetTVOutput(bool enabled)
        {
            vc0706.TvOutput(enabled);
        }

        public string GetVersion()
        {
            return vc0706.GetVersion();
        }

        public int GetCompression()
        {
            return vc0706.GetCompression();
        }

        public bool GetMotionDetectionCommStatus()
        {
            return vc0706.GetMotionDetectionStatus();
        }

        public ColorControl GetColorStatus()
        {
            return (ColorControl)vc0706.GetColorStatus();
        }

        public void SetColorStatus(ColorControl colorControl)
        {
            vc0706.SetColorControl((byte)colorControl);
        }

        public ImageSize GetImageSize()
        {
            return (ImageSize)vc0706.GetImageSize();
        }

        public void SetImageSize(ImageSize imageSize)
        {
            vc0706.SetImageSize((byte)imageSize);
        }

        public bool IsMotionDetectionEnabled()
        {
            return vc0706.GetMotionDetectionStatus();
        }

        void CloseComPort()
        {
            if (serialPort != null)
            {
                serialPort.Disable();
                serialPort.Dispose();
                serialPort = null;
            }
        }

        public void Dispose()
        {
            CloseComPort();
        }
    }
}