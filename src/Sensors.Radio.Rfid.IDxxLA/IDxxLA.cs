using System;
using System.Diagnostics;
using Meadow.TinyCLR.Sensors.Radio.Rfid.Serial.Helpers;
using Meadow.Utilities;
using GHIElectronics.TinyCLR.Devices.Uart;
using Meadow.TinyCLR.System;
using System.Text;

namespace Meadow.TinyCLR.Sensors.Radio.Rfid
{
    //TODO: update to new serial stuff
    /// <summary>
    /// RFID reader for ID-2LA, ID-12LA and ID-20LA serial readers.
    /// </summary>
    /// <remarks>
    /// Only supports reading ASCII output formats. Magnet emulation and Wiegand26 are not supported.
    /// Based on the datasheet, this code should also work for the non-LA variants of the RFID readers. The only significant
    /// change is the different voltage that the device will need to be supplied with.
    /// </remarks>
    public class IDxxLA : IRfidReader
    {
        public bool IsEnable { get; set; }
        public const int BaudRate = 9600;
        public const int DataBits = 7;

        private const byte StartToken = 2;
        private const byte EndToken = 3;

        //private readonly IList<IObserver<byte[]>> _observers = new List<IObserver<byte[]>>();
        private SerialEventPoller _serialPoller;

        private byte[] _internalBuffer;
        /*
        /// <summary>
        /// Exposed BufferOverrun from <see cref="SerialPort" />.
        /// </summary>
        public event EventHandler BufferOverrun
        {
            add => SerialPort.BufferOverrun += value;
            remove => SerialPort.BufferOverrun -= value;
        }
        */
        /// <inheritdoc />
        public event RfidReadEventHandler RfidRead = delegate { };

        /// <summary>
        /// Create an IDxxLA RFID reader
        /// </summary>
        /// <param name="device">Device to use</param>
        /// <param name="serialPortName">Port name to use</param>
        public IDxxLA(string serialPortName)

        {
            var myUart = UartController.FromName(serialPortName);

            var uartSetting = new UartSetting()
            {
                BaudRate = 115200,
                DataBits = 8,
                Parity = UartParity.None,
                StopBits = UartStopBitCount.One,
                Handshaking = UartHandshake.None,
            };

            myUart.SetActiveSettings(uartSetting);

            
            Setup(myUart);
            //device.CreateSerialPort(serialPortName, BaudRate, DataBits))
        }

        /// <summary>
        /// Create an IDxxLA RFID reader using an existing port.
        /// </summary>
        /// <param name="serialPort"></param>
        /// <remarks>
        /// Be sure to use suitable settings when creating the serial port.
        /// Default <see cref="BaudRate" /> and <see cref="DataBits" /> are exposed as constants.
        /// </remarks>
        public IDxxLA(UartController serialPort)
        {
            Setup(serialPort);
        }
        void Setup(UartController serialPort)
        {
            SerialPort = serialPort;
            
            _serialPoller = new SerialEventPoller(SerialPort);
            _serialPoller.DataReceived += OnDataReceivedEvent;
        }
        public UartController SerialPort { get; set; }

        /// <inheritdoc />
        public byte[] LastRead { get; private set; }

        /// <summary>
        /// Dispose of this instance.
        /// </summary>
        public void Dispose()
        {
            //foreach (var observer in _observers)
            //{
            //    observer?.OnCompleted();
            //}
            //_observers.Clear();

            _serialPoller?.Dispose();
            if (IsEnable)
            {
                IsEnable = false;
                SerialPort.Disable();
            }
        }

        /// <inheritdoc />
        public void StartReading()
        {
            _serialPoller.Start();
            SerialPort.Enable();
            IsEnable = true;
            //SerialPort.Enable();
        }


        /// <inheritdoc />
        public void StopReading()
        {
            _serialPoller.Stop();
            IsEnable = false;
            SerialPort.Disable();
            SerialPort.ClearReadBuffer();
        }

        /// <summary>
        /// Subscribe to RFID tag reads.
        /// Observer will only receive valid reads, with invalid reads triggering an OnError call.
        /// OnComplete will be called if this instance is disposed.
        /// This call is thread-safe.
        /// </summary>
        /// <param name="observer">The observer to subscribe</param>
        /// <returns>Disposable unsubscriber</returns>
        //public IDisposable Subscribe(IObserver<byte[]> observer)
        //{
        //    // Ensure thread safety
        //    // See https://docs.microsoft.com/en-us/dotnet/standard/events/observer-design-pattern-best-practices
        //    lock (_observers)
        //    {
        //        if (!_observers.Contains(observer))
        //        {
        //            _observers.Add(observer);
        //        }
        //    }

        //    return new Unsubscriber(_observers, observer);
        //}

        private static byte[] AsciiHexByteArrayToBytes(byte[] hexBytes)
        {
            if (hexBytes.Length % 2 != 0)
            {
                throw new ArgumentException(
                    "Byte array must contain an event number of bytes to be parsed",
                    nameof(hexBytes));
            }

            var result = new byte[hexBytes.Length / 2];
            for (var i = 0; i < hexBytes.Length; i += 2)
            {
                result[i / 2] = (byte) ((AsciiHexByteToByte(hexBytes[i]) << 4) | AsciiHexByteToByte(hexBytes[i + 1]));
            }

            return result;
        }

        private static byte AsciiHexByteToByte(byte hexByte)
        {
            switch (hexByte)
            {
                case 48:
                    return 0;
                case 49:
                    return 1;
                case 50:
                    return 2;
                case 51:
                    return 3;
                case 52:
                    return 4;
                case 53:
                    return 5;
                case 54:
                    return 6;
                case 55:
                    return 7;
                case 56:
                    return 8;
                case 57:
                    return 9;
                case 65:
                    return 10;
                case 66:
                    return 11;
                case 67:
                    return 12;
                case 68:
                    return 13;
                case 69:
                    return 14;
                case 70:
                    return 15;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(hexByte),
                       
                        "Value must be a valid ASCII representation of a hex character (0-F)");
            }
        }

        private static Rfidresult GetValidatedRfidTag(byte[] data)
        {
            // Valid format is as follows:
            // STX, 0-F x10 tag, 0-F x2 checksum, CR, LF, ETX
            // example:
            // STX 7 C 0 0 5 5 F 8 C 4 1 5 CR LF ETX
            const int validLength = 16;
            const int startByte = 0;
            const int endByte = 15;
            const int tagStartByte = 1;
            const int tagLength = 10;
            const int checksumStartByte = 11;
            const int checksumLength = 2;


            if (data.Length != validLength)
            {
                Debug.WriteLine(
                    $"Serial data is not of expected length for RFID tag format. Expected {validLength}, actual {data.Length}");
                return new Rfidresult(tag: null, status: RfidValidationStatus.InvalidDataFormat);
            }

            if (data[startByte] != StartToken)
            {
                Debug.WriteLine(
                    $"Invalid start byte in serial data for RFID tag format. Expected '{StartToken}', actual '{data[startByte]}'");
                return new Rfidresult(tag: null, status: RfidValidationStatus.InvalidDataFormat);
            }

            if (data[endByte] != EndToken)
            {
                Debug.WriteLine(
                    $"Invalid end byte in serial data for RFID tag format. Expected '{EndToken}', actual '{data[endByte]}'");
                return new Rfidresult(tag: null, status: RfidValidationStatus.InvalidDataFormat);
            }

            // verify tag is hexadecimal
            var tagSlice = Slice(data , tagStartByte, tagLength);
            if (!IsHexChars(tagSlice))
            {
                Debug.WriteLine(
                    "Invalid end byte in serial data for RFID tag format. Expected hex ASCII character (48-57, 65-70)");
                return new Rfidresult(tag: null, status: RfidValidationStatus.InvalidDataFormat);
            }

            // verify checksum is hexadecimal
            var checksumSlice = Slice(data,checksumStartByte, checksumLength);
            if (!IsHexChars(checksumSlice))
            {
                Debug.WriteLine(
                    "Invalid end byte in serial data for RFID tag format. Expected hex ASCII character (48-57, 65-70)");
                return new Rfidresult(tag: null, status: RfidValidationStatus.InvalidDataFormat);
            }

            var tag = AsciiHexByteArrayToBytes(tagSlice);
            var checkSum = AsciiHexByteArrayToBytes(checksumSlice)[0];

            return ChecksumCalculator.XOR(tag) == checkSum
                ? new Rfidresult(tag, status: RfidValidationStatus.Ok)
                : new Rfidresult(tag, status: RfidValidationStatus.ChecksumFailed);
        }

        static byte[] Slice(byte[] data, int start,int length)
        {
            var temp = new byte[length];
            for(int i = 0; i < length; i++)
            {
                temp[i] = data[start + i];
            }
            return temp;
        }
        private static bool IsHexChars(byte[] asciiBytes)
        {
            foreach (var asciiByte in asciiBytes)
            {
                if (!(asciiByte >= 48 && asciiByte <= 57) && !(asciiByte >= 65 && asciiByte <= 70))
                {
                    return false;
                }
            }

            return true;
        }

        private void OnDataReceivedEvent(object sender, PolledSerialDataReceivedEventArgs e)
        {
            ReadBuffer(e.SerialPort);
        }

        private void OnTagReadEvent(RfidValidationStatus status, byte[] tag)
        {
            if (status == RfidValidationStatus.Ok)
            {
                LastRead = tag;
            }

            RfidRead(this, new RfidReadResult { Status = status, RfidTag = tag });
            //foreach (var observer in _observers)
            //{
            //    if (status == RfidValidationStatus.Ok)
            //    {
            //        observer?.OnNext(tag);
            //    }
            //    else
            //    {
            //        observer?.OnError(new RfidValidationException(status));
            //    }
            //}
        }
        

        private void ReadBuffer(UartController port)
        {
            while (port.BytesToRead > 0)
            {
                //throw new Exception("Need to update this to new serial stuff");

                //TODO: UPDATE UPDATE
                var rxBuffer = new byte[SerialPort.BytesToRead];
                var bytesReceived = SerialPort.Read(rxBuffer, 0, SerialPort.BytesToRead);

                var data = rxBuffer;//Encoding.UTF8.GetString(rxBuffer, 0, bytesReceived);// new byte[0];// port.ReadToToken(EndToken);
                if (data.Length == 0)
                {
                    break;
                }

                if (_internalBuffer != null && _internalBuffer.Length > 0)
                {
                    var newData = data;
                    data = new byte[_internalBuffer.Length + newData.Length];
                    _internalBuffer.CopyTo(data, 0);
                    newData.CopyTo(data, _internalBuffer.Length);
                    _internalBuffer = null;
                }

                // check we have a valid end token
                if (data[data.Length - 1] != EndToken)
                {
                    // missing end token, skip until next read
                    _internalBuffer = data;
                    continue;
                }

                var result = GetValidatedRfidTag(data);
                OnTagReadEvent(result.status, result.tag);
            }
        }
        /*
        private class Unsubscriber : IDisposable
        {
            private readonly IObserver<byte[]> _observer;
            private readonly IList<IObserver<byte[]>> _observers;

            public Unsubscriber(IList<IObserver<byte[]>> observers, IObserver<byte[]> observer)
            {
                _observers = observers;
                _observer = observer;
            }

            public void Dispose()
            {
                // Ensure thread safety
                // See https://docs.microsoft.com/en-us/dotnet/standard/events/observer-design-pattern-best-practices
                lock (_observers)
                {
                    if (_observer != null)
                    {
                        _observers?.Remove(_observer);
                    }
                }
            }
        }*/
    }

    public class Rfidresult
    {
        public byte[] tag;
        public RfidValidationStatus status;
        public Rfidresult(byte[] tag, RfidValidationStatus status)
        {
            this.tag = tag;
            this.status = status;
        }
    }
}
