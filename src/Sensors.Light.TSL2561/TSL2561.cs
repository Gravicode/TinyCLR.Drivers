﻿using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;
using Meadow.TinyCLR.Core;
using System;
using System.Threading;

namespace Meadow.TinyCLR.Sensors.Light
{
    public delegate void LightLevelHandler(object sender, float value);
    /// <summary>
    ///     Driver for the TSL2561 light-to-digital converter.
    /// </summary>
    public class Tsl2561 : IDisposable //, ILightSensor
    {
        #region Constants

        /// <summary>
        ///     The command bit in the Command Register.
        ///     See page 13 of the datasheet.
        /// </summary>
        private const byte CommandBit = 0x80;

        /// <summary>
        ///     The interrupt clear bit in the Command Register.
        ///     See page 13 of the datasheet.
        /// </summary>
        private const byte ClearInterruptBit = 0xC0;

        /// <summary>
        ///     This bit control the write operations for the TSL2561.  Setting
        ///     this bit puts the chip into Word mode for the specified register.
        /// </summary>
        /// <remarks>
        ///     See page 13 of the data sheet.
        /// </remarks>
        private const byte WordModeBit = 0x20;

        /// <summary>
        ///     Minimum value that should be used for the polling frequency.
        /// </summary>
        public const ushort MinimumPollingPeriod = 100;
        
        #endregion Constants

        #region Enums

        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Default = 0x39,
            Address0 = 0x29,
            Address1 = 0x49
        }

        /// <summary>
        ///     Integration timing.
        ///     See Timing Register on page 14 of the data sheet.
        /// </summary>
        /// <remarks>
        ///     Valid integration times are 13.7ms, 101ms, 402ms and Manual.
        /// </remarks>
        public enum IntegrationTiming : byte
        {
            Ms13 = 0,
            Ms101,
            Ms402,
            Manual
        }

        /// <summary>
        ///     Possible gain setting for the sensor.
        ///     See Timing Register on page 14 of the data sheet.
        /// </summary>
        /// <remarks>
        ///     Possible gain values are low (x1) and high (x16).
        /// </remarks>
        public enum Gain
        {
            Low,
            High
        }

        /// <summary>
        ///     Determine if interrupts are enabled or not.
        ///     See Interrupt Control Register on page 15 of the datasheet.
        /// </summary>
        public enum InterruptMode : byte
        {
            Disable = 0,
            Enable
        }

        #endregion Enums

        #region Classes / structures

        /// <summary>
        ///     TSL2561 register locations.
        ///     See Register Set on page 12 and Command Register on page 13 of the datasheet.
        /// </summary>
        /// <remarks>
        ///     All of the register numbers have 0x80 added to the register.  When reading
        ///     or witing to a register the application must set the CMD bit in the command
        ///     register (see page 13) and the register address is written into the lower
        ///     four bits of the Command Register.
        /// </remarks>
        private static class Registers
        {
            public static readonly byte Control = 0x80;
            public static readonly byte Timing = 0x81;
            public static readonly byte ThresholdLow = 0x82;
            public static readonly byte ThresholdHigh = 0x84;
            public static readonly byte InterruptControl = 0x86;
            public static readonly byte ID = 0x8a;
            public static readonly byte Data0 = 0x8c;
            public static readonly byte Data1 = 0x8e;
        }

        #endregion Classes / structures

        #region Member variables / fields.

        /// <summary>
        ///     GPIO pin on that is connected to the interrupt pin on the TSL2561.
        /// </summary>
        private GpioPin _interruptPin;

        /// <summary>
        ///     Update interval in milliseconds
        /// </summary>
        private readonly ushort updateInterval = 100;

        #endregion Member variables / fields.

        #region IDisposable

        /// <summary>
        ///     Implement IDisposable interface.
        /// </summary>
        public void Dispose()
        {
            if (_interruptPin != null)
            {
              //TODO - check if needed  _interruptPin.Dispose();
            }
        }

        #endregion IDisposable

        #region Properties

        /// <summary>
        ///     Get the sensor reading
        /// </summary>
        /// <remarks>
        ///     This can be used to get the raw sensor data from the TSL2561.
        /// </remarks>
        /// <returns>Sensor data.</returns>
        public byte[] SensorReading
        {
            get {

                var read = new byte[2];
                tsl2561.WriteRead(new byte[] { Registers.Data0 }, read);//, ByteOrder.LittleEndian);
                return read;
            }
        }

        /// <summary>
        ///     Luminosity reading from the TSL2561 sensor.
        /// </summary>
        public float Luminosity
        {
            get { return _lightLevel; }
            set
            {
                _lightLevel = value;
                if ((updateInterval > 0) && (Math.Abs(_lastNotifiedLux - value) >= LightLevelChangeNotificationThreshold))
                {
                    LightLevelChanged(this, value);
                    _lastNotifiedLux = value;
                }
            }
        }
        private float _lightLevel;
        private float _lastNotifiedLux = 0.001F;

        /// <summary>
        ///     ID of the sensor.
        /// </summary>
        /// <remarks>
        ///     The ID register (page 16 of the datasheet) gives two pieces of the information:
        ///     Part Number: bits 4-7 (0000 = TSL2560, 0001 = TSL2561)
        ///     Revision number: bits 0-3
        /// </remarks>
        public byte ID
        {
            get {
                var read = new byte[1];
                tsl2561.WriteRead(new byte[] { Registers.ID },read);
                return read[0];
            }
        }

        /// <summary>
        ///     Gain of the sensor.
        ///     The sensor gain can be set to high or low.
        /// </summary>
        /// <remarks>
        ///     The sensor Gain bit can be found in the Timing Register.  This allows the gain
        ///     to be set to High (16x) or Low (1x).
        ///     See page 14 of the datasheet.
        /// </remarks>
        public Gain SensorGain
        {
            get
            {
                var data = new byte[1];
                tsl2561.WriteRead(new byte[] { Registers.Timing }, data);
                data[0] =(byte) (data[0] & 0x10);
                return data[0] == 0 ? Gain.Low : Gain.High;
            }
            set
            {
                var data = new byte[1];
                tsl2561.WriteRead(new byte[] { Registers.Timing }, data);
                if (value == Gain.Low)
                {
                    data[0] &= 0xef; // Set bit 4 to 0.
                }
                else
                {
                    data[0] |= 0x10; // Set bit 4 to 1.
                }
                tsl2561.Write(new byte[] { Registers.Timing, data[0] });
            }
        }

        /// <summary>
        ///     Integration timing for the sensor reading.
        /// </summary>
        public IntegrationTiming Timing
        {
            get
            {
                var timing = new byte[1];
                tsl2561.WriteRead(new byte[] { Registers.Timing },timing);
                timing[0] &= 0x03;
                return (IntegrationTiming) timing[0];
            }
            set
            {
                var timing_b = new byte[1];
                tsl2561.WriteRead(new byte[] { Registers.Timing }, timing_b);
                var timing = (sbyte)timing_b[0];
                if (SensorGain == Gain.High)
                {
                    timing |= 0x10;
                }
                else
                {
                    timing &= ~0x10;
                }
                timing &= ~ 0x03;
                timing |= (sbyte) ((sbyte) value & 0x03);
                tsl2561.Write(new byte[] { Registers.Timing, (byte)timing });
            }
        }

        /// <summary>
        ///     Lower interrupt threshold.
        /// </summary>
        /// <remarks>
        ///     Get or se the lower interrupt threshold.  Any readings below this
        ///     value may trigger an interrupt <seealso cref="SetInterruptMode" />
        ///     See page 14/15 of the datasheet.
        /// </remarks>
        public ushort ThresholdLow
        {
            get {
                var data = new byte[1];
                tsl2561.WriteRead(new byte[] { Registers.ThresholdLow}, data);//ByteOrder.LittleEndian); 
                return data[0];
            }
                set
            {
                var data = new byte[1];
                tsl2561.Write(new byte[] { (byte)(Registers.ThresholdLow + WordModeBit), (byte)value } );//ByteOrder.LittleEndian
            }
        }

        /// <summary>
        ///     High interrupt threshold.
        /// </summary>
        /// <remarks>
        ///     Get or se the upper interrupt threshold.  Any readings above this
        ///     value may trigger an interrupt <seealso cref="SetInterruptMode" />
        ///     See page 14/15 of the datasheet.
        /// </remarks>
        public ushort ThresholdHigh
        {
            get {

                var read = new byte[1];
                tsl2561.WriteRead(new byte[] { Registers.ThresholdHigh }, read);//ByteOrder.LittleEndian
                return read[0];
            }
                set
            {
                tsl2561.Write(new byte[] { (byte)(Registers.ThresholdHigh + WordModeBit), (byte)value  });//ByteOrder.LittleEndian
            }
        }

        /// <summary>
        ///     Changes in light level greater than this value will generate an interrupt
        ///     in auto-update mode.
        /// </summary>
        public float LightLevelChangeNotificationThreshold { get; set; } = 0.001F;

        /// <summary>
        ///     ICommunicationBus object used to communicate with the sensor.
        /// </summary>
        /// <remarks>
        ///     In this case the actual object will always be an I2SBus object.
        /// </remarks>
        private readonly I2cDevice tsl2561;

        #endregion Properties

        #region Event definitions

        /// <summary>
        ///     Allow the user to attach an interrupt to the TSL2561.
        /// </summary>
        /// <remarks>
        ///     This interrupt requires the interrupts to be set up correctly.
        ///     <see cref="SetInterruptMode" />
        /// </remarks>
        /// <param name="time">Date and time the interrupt was generated.</param>
        public delegate void ThresholdInterrupt(DateTime time);

        /// <summary>
        ///     Interrupt generated when the reading is outside of the threshold window.
        /// </summary>
        /// <remarks>
        ///     This interrupt requires the threshold window to be defined <see cref="SetInterruptMode" />
        ///     and for the interrupts to be enabled.
        /// </remarks>
        public event ThresholdInterrupt ReadingOutsideThresholdWindow;

        /// <summary>
        ///     Event raised when the temperature change is greater than the 
        ///     TemperatureChangeNotificationThreshold value.
        /// </summary>
        public event LightLevelHandler LightLevelChanged = delegate { };

        #endregion Event definitions

        #region Constructor(s)

        /// <summary>
        ///     Create a new instance of the TSL2561 class with the specified I2C address.
        /// </summary>
        /// <remarks>
        ///     By default the sensor will be set to low gain.
        /// <remarks>
        /// <param name="address">I2C address of the TSL2561</param>
        /// <param name="i2cBus">I2C bus (default = 100 KHz).</param>
        /// <param name="updateInterval">Update interval for the sensor (in milliseconds).</param>
        /// <param name="lightLevelChangeNotificationThreshold">Changes in light level greater than this value will generate an interrupt in auto-update mode.</param>
        public Tsl2561(string i2cBus, byte address = (byte) Addresses.Default, ushort updateInterval = MinimumPollingPeriod,
            float lightLevelChangeNotificationThreshold = 10.0F)
        {
            if (lightLevelChangeNotificationThreshold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lightLevelChangeNotificationThreshold), "Light level threshold change values should be >= 0");
            }

            LightLevelChangeNotificationThreshold = lightLevelChangeNotificationThreshold;
            this.updateInterval = updateInterval;

            var settings = new I2cConnectionSettings(address, 100_000); //The slave's address and the bus speed.
            var controller = I2cController.FromName(i2cBus);
            var device = controller.GetDevice(settings);

            //var device = new I2cPeripheral(i2cBus, address);
            tsl2561 = device;
            //
            //  Wait for the sensor to prepare the first reading (402ms after power on).
            //
            Thread.Sleep(410);
            if (updateInterval > 0)
            {
                StartUpdating();
            }
            else
            {
                Update();
            }
        }

        #endregion Constructor(s)

        #region Methods

        /// <summary>
        ///     Start the update process.
        /// </summary>
        private void StartUpdating()
        {
            Thread t = new Thread(() => {
                while (true)
                {
                    Update();
                    Thread.Sleep(updateInterval);
                }
            });
            t.Start();
        }

        /// <summary>
        ///     Update the Luminosity reading.
        /// </summary>
        public void Update()
        {
            var adcData = SensorReading;
            if (adcData[0] != 0)
            {
                var data0 = adcData[0];
                var data1 = adcData[1];
                if ((data0 == 0xffff) | (data1 == 0xffff))
                {
                    Luminosity = 0.0F;
                }
                double d0 = data0;
                double d1 = data1;
                var ratio = d1 / d0;

                var milliseconds = 0;
                switch (Timing)
                {
                    case IntegrationTiming.Ms13:
                        milliseconds = 14;
                        break;
                    case IntegrationTiming.Ms101:
                        milliseconds = 101;
                        break;
                    case IntegrationTiming.Ms402:
                        milliseconds = 402;
                        break;
                    case IntegrationTiming.Manual:
                        milliseconds = 0;
                        break;
                }
                var result = 0.0;
                if (milliseconds != 0)
                {
                    d0 *= 402.0 / milliseconds;
                    d1 *= 402.0 / milliseconds;
                    if (SensorGain == Gain.Low)
                    {
                        d0 *= 16;
                        d1 *= 16;
                    }
                    if (ratio < 0.5)
                    {
                        result = (0.0304 * d0) - (0.062 * d0 * Math.Pow(ratio, 1.4));
                    }
                    else
                    {
                        if (ratio < 0.61)
                        {
                            result = (0.0224 * d0) - (0.031 * d1);
                        }
                        else
                        {
                            if (ratio < 0.80)
                            {
                                result = (0.0128 * d0) - (0.0153 * d1);
                            }
                            else
                            {
                                if (ratio < 1.30)
                                {
                                    result = (0.00146 * d0) - (0.00112 * d1);
                                }
                            }
                        }
                    }
                }
                Luminosity = (float)result;
            }
        }

        /// <summary>
        ///     Turn the TSL2561 off.
        /// </summary>
        /// <remarks>
        ///     Reset the power bits in the control register (page 13 of the datasheet).
        /// </remarks>
        public void TurnOff()
        {
            tsl2561.Write(new byte[] { Registers.Control, 0x00 });
        }

        /// <summary>
        ///     Turn the TSL2561 on.
        /// </summary>
        /// <remarks>
        ///     Set the power bits in the control register (page 13 of the datasheet).
        /// </remarks>
        public void TurnOn()
        {
            tsl2561.Write(new byte[] { Registers.Control, 0x03 });
        }

        /// <summary>
        ///     Clear the interrupt flag.
        ///     Se Command Register on page 13 of the datasheet.
        /// </summary>
        /// <remarks>
        ///     According to the datasheet, writing a 1 into bit 6 of the command
        ///     register will clear any pending interrupts.
        /// </remarks>
        public void ClearInterrupt()
        {
            tsl2561.Write(new byte[] { ClearInterruptBit });
            if (_interruptPin != null)
            {
                //Port: TODO - check if needed          _interruptPin.ClearInterrupt();
            }
        }

        /// <summary>
        ///     Put the sensor into manual integration mode.
        /// </summary>
        public void ManualStart()
        {
            var timing = new byte[1];
            tsl2561.WriteRead(new byte[] { Registers.Timing },timing);
            timing[0] |= 0x03;
            tsl2561.Write(new byte[] { Registers.Timing, timing[0] });
            timing[0] |= 0xf7; //  ~0x08;
            tsl2561.Write(new byte[] { Registers.Timing, timing[0] });
        }

        /// <summary>
        ///     Turn off manual integration mode.
        /// </summary>
        public void ManualStop()
        {
            var timing = new byte[1];
            tsl2561.WriteRead(new byte[] { Registers.Timing },timing);
            timing[0] &= 0xf7; //  ~0x08;
            tsl2561.Write(new byte[] { Registers.Timing, timing[0] });
        }

        /// <summary>
        ///     Turn interrupts on or off and set the conversion trigger count.
        /// </summary>
        /// <remarks>
        ///     The conversion count is the number of conversions that must be outside
        ///     of the upper and lower limits before and interrupt is generated.
        ///     See Interrupt Control Register on page 15 and 16 of the datasheet.
        /// </remarks>
        /// <param name="mode"></param>
        /// <param name="conversionCount">
        ///     Number of conversions that must be outside of the threshold before an interrupt is
        ///     generated.
        /// </param>
        /// <param name="pin">GPIO pin connected to the TSL2561 interrupt pin.  Set to null to use the previously supplied pin.</param>
        public void SetInterruptMode( InterruptMode mode, byte conversionCount, int pin = -1)
        {
            if (conversionCount > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(conversionCount), "Conversion count must be in the range 0-15 inclusive.");
            }
            //
            //  Attach the interrupt event before we turn on interrupts.
            //
            if (pin > -1)
            {
                if (_interruptPin != null)
                {
                  //Port: TODO check  _interruptPin.Dispose();
                }
                //_interruptPin = device.CreateDigitalInputPort(pin, Hardware.InterruptMode.EdgeRising, ResistorMode.PullUp);
                //_interruptPin.Changed += InterruptPin_Changed;
                var gpio = GpioController.GetDefault();

                _interruptPin = gpio.OpenPin(pin);
                _interruptPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
                _interruptPin.ValueChanged += _interruptPin_ValueChanged; 
            }
            else
            {
                if (_interruptPin == null)
                {
                    throw new ArgumentException("Interrupt pin must be supplied");
                }
            }
            //
            // Put interrupt control in bits 4 & 5 of the Interrupt Control Register.
            // Using the enum above makes sure that mode is in the range 0-3 inclusive.
            //
            var registerValue = (byte) mode;
            registerValue <<= 4;
            //
            // conversionCount is known to be 0-15, put this in the lower four bits of
            // the Interrupt Control Register.
            //
            registerValue |= conversionCount;
            //
            //  Clear the interrupt bit before we turn them on.
            //
            ClearInterrupt();
            tsl2561.Write(new byte[] { Registers.InterruptControl, registerValue });
        }

        private void _interruptPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            ReadingOutsideThresholdWindow?.Invoke(DateTime.Now);
        }


        /// <summary>
        ///     Process the interrupt generated by the TSL2561.
        /// </summary>
        //private void InterruptPin_Changed(object sender, DigitalInputPortEventArgs e)
        //{
           
        //}

        #endregion Methods
    }
}