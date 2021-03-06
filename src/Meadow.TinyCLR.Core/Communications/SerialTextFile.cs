﻿using GHIElectronics.TinyCLR.Devices.Uart;
using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace Meadow.TinyCLR.Communications
{
    /// <summary>
    ///     Provide a mechanism for reading lines of text from a SerialPort.
    /// </summary>
    public class SerialTextFile
    {
        #region Constants

        /// <summary>
        ///     Default buffer size for the incoming data from the serial port.
        /// </summary>
        private const int MAXIMUM_BUFFER_SIZE = 512;

        #endregion Constants

        #region Member variables / fields

        /// <summary>
        ///     Serial port object that the
        /// </summary>
        private readonly UartController serialPort;

        /// <summary>
        ///     Buffer to hold the incoming text from the serial port.
        /// </summary>
        private string buffer = string.Empty;

        /// <summary>
        ///     The static buffer is used when processing the text coming in from the
        ///     serial port.
        /// </summary>
        private readonly byte[] staticBuffer = new byte[MAXIMUM_BUFFER_SIZE];

        /// <summary>
        ///     Character(s) that indicate an end of line in the text stream.
        /// </summary>
        private readonly string LINE_END = "\n";

        #endregion Member variables / fields

        #region Events and delegates

        /// <summary>
        ///     Delegate for the line ready event.
        /// </summary>
        /// <param name="line">Line of text ready for processing.</param>
        /// <param name="sender">Reference to the object generating the event.</param>
        public delegate void LineReceived(object sender, string line);

        /// <summary>
        ///     A complete line of text has been read, send this to the event subscriber.
        /// </summary>
        public event LineReceived OnLineReceived;

        #endregion Events and delegates

        #region Constructors

        /// <summary>
        ///     Default constructor for the SerialTextFile class, made private to prevent the
        ///     programmer from using this method of construcing an object.
        /// </summary>
        private SerialTextFile()
        {
        }
      
        public bool IsEnabled { get; set; }
        /// <summary>
        ///     Create a new SerialTextFile and attach the instance to the specfied serial port.
        /// </summary>
        /// <param name="port">Serial port name.</param>
        /// <param name="baudRate">Baud rate.</param>
        /// <param name="parity">Parity.</param>
        /// <param name="dataBits">Data bits.</param>
        /// <param name="stopBits">Stop bits.</param>
        /// <param name="endOfLine">Text indicating the end of a line of text.</param>
        public SerialTextFile(string UartControllerName,int baudRate, UartParity parity, int dataBits, UartStopBitCount stopBits,
            string endOfLine)
        {
                serialPort = UartController.FromName(UartControllerName);
            var uartSetting = new UartSetting()
            {
                BaudRate = baudRate,
                DataBits = dataBits,
                Parity = parity,
                StopBits = stopBits,
                Handshaking = UartHandshake.None,
            };

            serialPort.SetActiveSettings(uartSetting);

            serialPort.Enable();
            IsEnabled = true;
            LINE_END = endOfLine;
            serialPort.DataReceived += SerialPort_DataReceived;
            

        }

        private void SerialPort_DataReceived(UartController sender, DataReceivedEventArgs e)
        {
            if (e.Count > 0)
            {  
                ReadDataFromSerialPort();
            }
        }

        /// <summary>
        ///     Create a new SerialTextFile and attach the instance to the specfied serial port.
        /// </summary>
        /// <param name="serialPort">Serial port object.</param>
        /// <param name="endOfLine">Text indicating the end of a line of text.</param>
        public SerialTextFile(UartController serialPort, string endOfLine, bool useSerialEvents = true)
        {
            this.serialPort = serialPort;
            LINE_END = endOfLine;

            if (useSerialEvents)
            {
                serialPort.DataReceived += SerialPort_DataReceived;
            }
            else
            {
                while (true)
                {
                    ReadDataFromSerialPort();
                    Thread.Sleep(200);
                }
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        ///     Open the serial port and start processing the data from the serial port.
        /// </summary>
        public void Open()
        {
            //Console.WriteLine("SerialTextFile: Open");

            if (!IsEnabled)
            {
                //Console.WriteLine("SerialTextFile: _serialPort.Open");
                serialPort.Enable();
                IsEnabled = true;
            }
        }

        /// <summary>
        ///     Close the serial port and stop processing data.
        /// </summary>
        /// <remarks>
        ///     This method clears the buffer and destroys any pending text.
        /// </remarks>
        public void Close()
        {
            if (IsEnabled)
            {
                serialPort.Disable();
                IsEnabled = false;
            }
            buffer = string.Empty;
        }

        #endregion Methods

        #region Interrupt handlers

       

        private void ReadDataFromSerialPort()
        {
            lock (buffer)
            {
                int amount = serialPort.Read(staticBuffer, 0, MAXIMUM_BUFFER_SIZE);
                // var bytesReceived = serialPort.Read(rxBuffer, 0, e.Count);
                //Console.WriteLine($"Data amount: {amount}");

                if (amount > 0)
                {
                    for (var index = 0; index < amount; index++)
                    {
                        buffer += (char)staticBuffer[index];
                    }
                }
                var eolMarkerPosition = buffer.IndexOf(LINE_END);

                while (eolMarkerPosition >= 0)
                {
                    var line = buffer.Substring(0, eolMarkerPosition);
                    buffer = buffer.Substring(eolMarkerPosition + 2);
                    eolMarkerPosition = buffer.IndexOf(LINE_END);

                    // Console.WriteLine($"Line: {line}");

                    OnLineReceived?.Invoke(this, line);
                }
            }
        }

        #endregion Interrupt handlers
    }
}
