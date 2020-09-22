using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;
using System;

namespace Meadow.TinyCLR.Sensors.Motion
{
    public class Mag3110
    {
        #region Structures

        /// <summary>
        ///     Sensor readings to be passed back when an interrupt is generated.
        /// </summary>
        public struct SensorReading
        {
            public short X;
            public short Y;
            public short Z;
        }

        /// <summary>
        ///     Register addresses in the sensor.
        /// </summary>
        private static class Registers
        {
            public static readonly byte DRStatus = 0x00;
            public static readonly byte XMSB = 0x01;
            public static readonly byte XLSB = 0x02;
            public static readonly byte YMSB = 0x03;
            public static readonly byte YLSB = 0x04;
            public static readonly byte ZMSB = 0x05;
            public static readonly byte ZLSB = 0x06;
            public static readonly byte WhoAmI = 0x07;
            public static readonly byte SystemMode = 0x08;
            public static readonly byte XOffsetMSB = 0x09;
            public static readonly byte XOffsetLSB = 0x0a;
            public static readonly byte YOffsetMSB = 0x0b;
            public static readonly byte YOffsetLSB = 0x0c;
            public static readonly byte ZOffsetMSB = 0x0d;
            public static readonly byte ZOffsetLSB = 0x0e;
            public static readonly byte Temperature = 0x0f;
            public static readonly byte Control1 = 0x10;
            public static readonly byte Control2 = 0x11;
        }

        #endregion Structures

        #region Delegates and events

        /// <summary>
        ///     Delegate for the OnDataReceived event.
        /// </summary>
        /// <param name="sensorReading">Sensor readings from the MAG3110.</param>
        public delegate void ReadingComplete(SensorReading sensorReading);

        /// <summary>
        ///     Generated when the sensor indicates that data is ready for processing.
        /// </summary>
        public event ReadingComplete OnReadingComplete;

        #endregion Delegates and events

        #region Member variables / fields

        /// <summary>
        ///     MAG3110 object.
        /// </summary>
        private I2cDevice _mag3110;

        /// <summary>
        ///     Interrupt port used to detect then end of a conversion.
        /// </summary>
        private GpioPin _digitalInputPort;

        #endregion Member variables / fields

        #region Properties

        /// <summary>
        ///     Reading from the X axis.
        /// </summary>
        /// <remarks>
        ///     Data in this property is only current after a call to Read.
        /// </remarks>
        public short X { get; private set; }

        /// <summary>
        ///     Reading from the Y axis.
        /// </summary>
        /// <remarks>
        ///     Data in this property is only current after a call to Read.
        /// </remarks>
        public short Y { get; private set; }

        /// <summary>
        ///     Reading from the Z axis.
        /// </summary>
        /// <remarks>
        ///     Data in this property is only current after a call to Read.
        /// </remarks>
        public short Z { get; private set; }

        /// <summary>
        ///     Temperature of the sensor die.
        /// </summary>
        public sbyte Temperature
        {
            get {
                var read = new byte[1];
                _mag3110.WriteRead(new byte[] { Registers.Temperature }, read);
                return (sbyte)read[0];
                //return (sbyte) _mag3110.ReadRegister((byte) Registers.Temperature);
            }
        }

        /// <summary>
        ///     Change or get the standby status of the sensor.
        /// </summary>
        public bool Standby
        {
            get
            {
                var controlRegister = new byte[1];
                _mag3110.WriteRead(new byte[] { Registers.Control1 }, controlRegister);
                //var controlRegister = _mag3110.ReadRegister((byte) Registers.Control1);
                return (controlRegister[0] & 0x03) == 0;
            }
            set
            {
                var controlRegister = new byte[1];
                _mag3110.WriteRead(new byte[] { Registers.Control1 }, controlRegister);
                //var controlRegister = _mag3110.ReadRegister((byte) Registers.Control1);
                if (value)
                {
                    controlRegister[0] &= 0xfc; // ~0x03
                }
                else
                {
                    controlRegister[0] |= 0x01;
                }
                _mag3110.Write(new byte[] { (byte)Registers.Control1, controlRegister[0] });
            }
        }

        /// <summary>
        ///     Indicate if there is any data ready for reading (x, y or z).
        /// </summary>
        /// <remarks>
        ///     See section 5.1.1 of the datasheet.
        /// </remarks>
        public bool DataReady
        {
            get
            {
                var read = new byte[1];
                _mag3110.WriteRead(new byte[] { Registers.DRStatus }, read);
                return (read[0] & 0x08) > 0;
            }
        }

        /// <summary>
        ///     Enable or disable interrupts.
        /// </summary>
        /// <remarks>
        ///     Interrupts can be triggered when a conversion completes (see section 4.2.5
        ///     of the datasheet).  The interrupts are tied to the ZYXDR bit in the DR Status
        ///     register.
        /// </remarks>
        private bool _digitalInputsEnabled;

        public bool DigitalInputsEnabled
        {
            get { return _digitalInputsEnabled; }
            set
            {
                Standby = true;
                var cr2 = new byte[1];
                _mag3110.WriteRead(new byte[] { Registers.Control2 }, cr2);
                if (value)
                {
                    cr2[0] |= 0x80;
                }
                else
                {
                    cr2[0] &= 0x7f;
                }
                _mag3110.Write(new byte[] { (byte)Registers.Control2, cr2[0] });
                _digitalInputsEnabled = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new MAG3110 object using the default parameters for the component.
        /// </summary>
        /// <param name="device">IO Device.</param>
        /// <param name="interruptPin">Interrupt pin used to detect end of conversions.</param>
        /// <param name="address">Address of the MAG3110 (default = 0x0e).</param>
        /// <param name="speed">Speed of the I2C bus (default = 400 KHz).</param>        
        public Mag3110( string i2cBus, int interruptPin = -1, byte address = 0x0e, ushort speed = 400) 
           {
            var gpio = GpioController.GetDefault();
            if (interruptPin > -1)
            {
                var intr = gpio.OpenPin(interruptPin);
                intr.SetDriveMode(GpioPinDriveMode.Input);
                Setup(i2cBus, intr, address);
            }
            else
            {
                Setup(i2cBus, null, address);
            }
            // this (i2cBus, device.CreateDigitalInputPort(interruptPin, InterruptMode.EdgeRising, ResistorMode.Disabled), address) 
        }

        /// <summary>
        /// Create a new MAG3110 object using the default parameters for the component.
        /// </summary>
        /// <param name="interruptPort">Interrupt port used to detect end of conversions.</param>
        /// <param name="address">Address of the MAG3110 (default = 0x0e).</param>
        /// <param name="i2cBus">I2C bus object - default = 400 KHz).</param>        
        public Mag3110(string i2cBus, GpioPin interruptPort = null, byte address = 0x0e)
        {
            Setup( i2cBus,  interruptPort,  address );
        }

        void Setup(string i2cBus, GpioPin interruptPort = null, byte address = 0x0e)
        {
            var settings = new I2cConnectionSettings(address, 100_000); //The slave's address and the bus speed.
            var controller = I2cController.FromName(i2cBus);
            _mag3110 = controller.GetDevice(settings);
            //_mag3110 = new I2cPeripheral(i2cBus, address);
            var deviceID = new byte[1];
            _mag3110.WriteRead(new byte[] { Registers.WhoAmI }, deviceID);
            //var deviceID = _mag3110.ReadRegister((byte)Registers.WhoAmI);
            if (deviceID[0] != 0xc4)
            {
                throw new Exception("Unknown device ID, " + deviceID + " retruend, 0xc4 expected");
            }

            if (interruptPort != null)
            {
                _digitalInputPort = interruptPort;
                _digitalInputPort.ValueChanged += _digitalInputPort_ValueChanged;
            }
            Reset();
        }

      

        #endregion Constructors

        #region Methods

        /// <summary>
        ///     Reset the sensor.
        /// </summary>
        public void Reset()
        {
            Standby = true;
            _mag3110.Write(new byte[] { (byte)Registers.Control1, 0x00 });
            _mag3110.Write(new byte[] { (byte)Registers.Control2, 0x80 });
            var write = new byte[] { Registers.XOffsetMSB, 0, 0, 0, 0, 0, 0 };
            _mag3110.Write(write);
        }

        /// <summary>
        ///     Force the sensor to make a reading and update the relevanyt properties.
        /// </summary>
        public void Read()
        {
            var controlRegister = new byte[1];
            _mag3110.WriteRead(new byte[] { Registers.Control1 }, controlRegister);
         
            //var controlRegister = _mag3110.ReadRegister((byte) Registers.Control1);
            controlRegister[0] |= 0x02;
            _mag3110.Write(new byte[] { (byte)Registers.Control1, controlRegister[0] });
            var data = new byte[6];
            _mag3110.WriteRead(new byte[] { Registers.XMSB }, data);
            //return read[0];
            //var data = _mag3110.ReadRegisters((byte) Registers.XMSB, 6);
            X = (short) ((data[0] << 8) | data[1]);
            Y = (short) ((data[2] << 8) | data[3]);
            Z = (short) ((data[4] << 8) | data[5]);
        }

        #endregion Methods

        #region Event handlers

        /// <summary>
        ///     Interrupt from the MAG3110 conversion complete interrupt.
        /// </summary>
        //private void DigitalInputPortChanged(object sender, DigitalInputPortEventArgs e)
        //{
        //    //DigitalInputPortChanged;
            
        //}
        private void _digitalInputPort_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            if (OnReadingComplete != null)
            {
                Read();
                var readings = new SensorReading();
                readings.X = X;
                readings.Y = Y;
                readings.Z = Z;
                OnReadingComplete(readings);
            }
        }
        #endregion Event handlers
    }
}