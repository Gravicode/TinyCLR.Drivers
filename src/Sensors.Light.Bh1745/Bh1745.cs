using GHIElectronics.TinyCLR.Devices.I2c;
using Meadow.TinyCLR.Core;
using System;
using System.Diagnostics;

namespace Meadow.TinyCLR.Sensors.Light
{
    public class Bh1745
    {
        #region Properties

        /// <summary>
        /// The primary I2c address of the BH1745
        /// </summary>
        public static byte PrimaryI2cAddress => 0x38;

        /// <summary>
        /// The secondary I2c address of the BH1745
        /// </summary>
        public static byte SecondaryI2cAddress => 0x39;

        public InterruptStatus InterruptReset
        {
            get
            {
                var intReset = new byte[1];
                bh1745.WriteRead(new byte[] { SYSTEM_CONTROL },intReset);
                intReset[0] = (byte)((intReset[0] & INT_RESET) >> 6);
                return (InterruptStatus)intReset[0];
            }
            set
            {
                //if (!Enum.IsDefined(typeof(InterruptStatus), value))
                //{
                //    throw new ArgumentOutOfRangeException();
                //}

                var intReset = new byte[1];
                bh1745.WriteRead(new byte[] { SYSTEM_CONTROL }, intReset);
                intReset[0] = (byte)((intReset[0] & ~INT_RESET) | (byte)value << 6);

                bh1745.Write(new byte[] { SYSTEM_CONTROL, intReset[0] });
            }
        }

        /// <summary>
        /// Gets or sets the currently set measurement time.
        /// </summary>
        public MeasurementTimeType MeasurementTime
        {
            get => _measurementTime;
            set
            {

                //if (!Enum.IsDefined(typeof(MeasurementTimeType), value))
                //{
                //    throw new ArgumentOutOfRangeException();
                //}
                var time = new byte[1];
                bh1745.WriteRead(new byte[] { MODE_CONTROL1 },time);
                time[0] = (byte)((time[0] & ~MEASUREMENT_TIME) | (byte)value);

                bh1745.Write(new byte[] { MODE_CONTROL1, time[0] });
                _measurementTime = value;
            }
        }
        private MeasurementTimeType _measurementTime;

        /// <summary>
        /// Is the sensor actively measuring
        /// </summary>
        public bool IsMeasurementActive
        {
            get => _isMeasurementActive;
            set
            {
                byte valueByte = value ? (byte)1 : (byte)0;
                var active = new byte[1];
                bh1745.WriteRead(new byte[] { MODE_CONTROL2 },active);
                active[0] = (byte)((active[0] & ~RGBC_EN) | valueByte << 4);

                bh1745.Write(new byte[] { MODE_CONTROL2, active[0] });
                _isMeasurementActive = value;
            }
        }
        private bool _isMeasurementActive;

        /// <summary>
        /// Gets or sets the ADC gain of the sensor
        /// </summary>
        public AdcGainType AdcGain
        {
            get => _adcGain;
            set
            {
                //if (!Enum.IsDefined(typeof(AdcGainType), value))
                //{
                //    throw new ArgumentOutOfRangeException();
                //}
                var adcGain = new byte[1];
                 bh1745.WriteRead(new byte[] { MODE_CONTROL2 },adcGain);
                adcGain[0] = (byte)((adcGain[0] & ~ADC_GAIN) | (byte)value);

                bh1745.Write(new byte[] { MODE_CONTROL2, adcGain[0] });
                _adcGain = value;
            }
        }
        private AdcGainType _adcGain;

        /// <summary>
        /// Is the interrupt active
        /// </summary>
        public bool InterruptSignalIsActive
        {
            get
            {
                var intStatus = new byte[1];
                bh1745.WriteRead(new byte[] { INTERRUPT },intStatus);
                intStatus[0] = (byte)((intStatus[0] & INT_STATUS) >> 7);
                return intStatus[0]==0;
            }
        }

        /// <summary>
        /// Gets or sets how the interrupt pin latches
        /// </summary>
        public LatchBehaviorType LatchBehavior
        {
            get => _latchBehavior;
            set
            {
                //if (!Enum.IsDefined(typeof(LatchBehaviorType), value))
                //{
                //    throw new ArgumentOutOfRangeException();
                //}
                var intLatch = new byte[1];
               bh1745.WriteRead(new byte[] { INTERRUPT },intLatch);
                intLatch[0] = (byte)((intLatch[0] & ~INT_LATCH) | (byte)value << 4);

                bh1745.Write(new byte[] { INTERRUPT, intLatch[0] });
                _latchBehavior = value;
            }
        }
        private LatchBehaviorType _latchBehavior;

        /// <summary>
        /// Gets or sets the source channel that triggers the interrupt
        /// </summary>
        public InterruptChannel InterruptSource
        {
            get => _interruptSource;
            set
            {
                //if (!Enum.IsDefined(typeof(InterruptChannel), value)) { throw new ArgumentOutOfRangeException(); }
                var intSource = new byte[1];
                bh1745.WriteRead(new byte[] { INTERRUPT },intSource);
                intSource[0] = (byte)((intSource[0] & ~INT_SOURCE) | (byte)value << 2);

                bh1745.Write(new byte[] { INTERRUPT, intSource[0] });
                _interruptSource = value;
            }
        }
        InterruptChannel _interruptSource;

        /// <summary>
        /// Gets or sets whether the interrupt pin is enabled
        /// </summary>
        public bool InterruptIsEnabled
        {
            get => _isInterruptEnabled;
            set
            {
                var intPin = new byte[1];
                    bh1745.WriteRead(new byte[] { INTERRUPT },intPin);
                intPin[0] = (byte)((intPin[0] & ~INT_ENABLE) | (value ? 1:0));

                bh1745.Write(new byte[] { INTERRUPT, intPin[0] });
                _isInterruptEnabled = value;
            }
        }
        private bool _isInterruptEnabled;

        /// <summary>
        /// Gets or sets the persistence function of the interrupt
        /// </summary>
        public InterruptType InterruptPersistence
        {
            get => _interruptPersistence;
            set
            {
                //if (!Enum.IsDefined(typeof(InterruptType), value))
                //{
                //    throw new ArgumentOutOfRangeException();
                //}
                var intPersistence = new byte[1];
                bh1745.WriteRead(new byte[] { PERSISTENCE },intPersistence);
                intPersistence[0] = (byte)((intPersistence[0] & ~PERSISTENCE_MASK) | (byte)value);

                bh1745.Write(new byte[] { PERSISTENCE, intPersistence[0] });
                _interruptPersistence = value;
            }
        }
        private InterruptType _interruptPersistence;

        /// <summary>
        /// Gets or sets the lower interrupt threshold
        /// </summary>
        public ushort LowerInterruptThreshold
        {
            get => _lowerInterruptThreshold;
            set
            {
                bh1745.Write(new byte[] { TL, (byte)value });
                _lowerInterruptThreshold = value;
            }
        }
        private ushort _lowerInterruptThreshold;

        /// <summary>
        /// Gets or sets the upper interrupt threshold
        /// </summary>
        public ushort UpperInterruptThreshold
        {
            get => _upperInterruptThreshold;
            set
            {
                bh1745.Write(new byte[] { TH, (byte)value });
                _upperInterruptThreshold = value;
            }
        }
        private ushort _upperInterruptThreshold;

        /// <summary>
        /// Gets or sets the channel compensation multipliers which are used to scale the channel measurements
        /// </summary>
        public ChannelMultipliers CompensationMultipliers { get; set; }

        #endregion

        #region Member variables / fields

        I2cDevice bh1745;

        //masks
        private readonly byte PART_ID = 0x3F;
        private readonly byte SW_RESET = 0x80;
        private readonly byte INT_RESET = 0x40;
        private readonly byte MEASUREMENT_TIME = 0x07;
        private readonly byte VALID = 0x80;
        private readonly byte RGBC_EN = 0x10;
        private readonly byte ADC_GAIN = 0x03;
        private readonly byte INT_STATUS = 0x80;
        private readonly byte INT_LATCH = 0x10;
        private readonly byte INT_SOURCE = 0x0C;
        private readonly byte INT_ENABLE = 0x01;
        private readonly byte PERSISTENCE_MASK = 0x03;

        // control registers
        private readonly byte SYSTEM_CONTROL = 0x40;
        private readonly byte MODE_CONTROL1 = 0x41;
        private readonly byte MODE_CONTROL2 = 0x42;
        private readonly byte MODE_CONTROL3 = 0x44;

        private readonly byte RED_DATA = 0x50;
        private readonly byte GREEN_DATA = 0x52;
        private readonly byte BLUE_DATA = 0x54;
        private readonly byte CLEAR_DATA = 0x56;
        private readonly byte DINT_DATA = 0x58;
        private readonly byte INTERRUPT = 0x60;
        private readonly byte PERSISTENCE = 0x61;
        private readonly byte TH = 0x62;
        private readonly byte TL = 0x64;
        private readonly byte MANUFACTURER_ID = 0x92;

        #endregion Member variables / fields

        #region Enums

        /// <summary>
        /// The available ADC gain scaling options for the Bh1745
        /// </summary>
        public enum AdcGainType : byte
        {
            X1 = 0x0,
            X2 = 0x1,
            X16 = 0x2
        }

        public enum InterruptType : byte
        {
            ToggleMeasurementEnd = 0x0,
            UpdateMeasurementEnd = 0x1,
            UpdateConsecutiveX4 = 0x2,
            UpdateConsecutiveX8 = 0x3 
        }

        public enum InterruptChannel : byte
        {
            Red = 0x0,
            Green = 0x1,
            Blue = 0x2,
            Clear = 0x3
        }

        public enum LatchBehaviorType : byte
        {
            LatchUntilReadOrInitialized = 0,
            LatchEachMeasurement = 1
        }

        public enum InterruptStatus
        {
            Active,
            Inactive
        }

        public enum MeasurementTimeType
        {
            Ms160 = 160,
            Ms320 = 320,
            Ms640 = 640,
            Ms1280 = 1280,
            Ms2560 = 2560,
            Ms5120 = 5120
        }


        #endregion Enums

        #region Constructors

        /// <summary>
        ///     Create a new BH17545 color sensor object
        /// </summary>
        public Bh1745(string i2cBus, byte address = 0x38)
        {
            var settings = new I2cConnectionSettings(address, 100_000); //The slave's address and the bus speed.
            var controller = I2cController.FromName(i2cBus);
            bh1745 = controller.GetDevice(settings);
            //bh1745 = new I2cDevice(i2cBus, address);

            CompensationMultipliers = new ChannelMultipliers
            {
                Red = 1.0, //2.2,
                Green = 1.0,
                Blue = 1.0, //1.8,
                Clear = 10.0
            };

            Reset();
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Resets the device to the default configuration
        /// On reset the sensor goes to power down mode
        /// </summary>
        public void Reset()
        {
            Debug.WriteLine("Reset");
            var status = new byte[1];
           bh1745.WriteRead(new byte[] { SYSTEM_CONTROL },status);
            status[0] = (byte)((status[0] & ~SW_RESET) | 0x01 << 7);

            bh1745?.Write(new byte[] { SYSTEM_CONTROL, status[0] });

            // set default measurement configuration
            MeasurementTime = MeasurementTimeType.Ms160;
            AdcGain = AdcGainType.X1;
            IsMeasurementActive = true;
            InterruptIsEnabled = true;

            // set fields to reset state
            _interruptPersistence = InterruptType.UpdateMeasurementEnd;
            _latchBehavior = LatchBehaviorType.LatchUntilReadOrInitialized;
            _interruptSource = InterruptChannel.Blue;
            _isInterruptEnabled = false;
            _lowerInterruptThreshold = 0x0000;
            _upperInterruptThreshold = 0xFFFF;

            // write default value to Mode_Control3
            bh1745.Write(new byte[] { MODE_CONTROL3, 0x02 });
        }

        /// <summary>
        /// Reads whether the last measurement is valid
        /// </summary>
        public bool ReadMeasurementIsValid()
        {
            var valid = new byte[1];
               bh1745.WriteRead(new byte[] { MODE_CONTROL2 },valid);
            valid[0] = (byte)(valid[0] & VALID);
            return valid[0]==0;
        }

        /// <summary>
        /// Reads the red data register of the sensor
        /// </summary>
        /// <returns></returns>
        public ushort ReadRedDataRegister()
        {
            var read = new byte[1];
            bh1745.WriteRead(new byte[] { RED_DATA },read);
            return read[0];
        }
        /// <summary>
        /// Reads the green data register of the sensor
        /// </summary>
        /// <returns></returns>
        public ushort ReadGreenDataRegister()
        {
            var read = new byte[1];
            bh1745.WriteRead(new byte[] { GREEN_DATA }, read);
            return read[0];
            //bh1745.ReadUShort(GREEN_DATA);
        }
        /// <summary>
        /// Reads the blue data register of the sensor
        /// </summary>
        /// <returns></returns>
        public ushort ReadBlueDataRegister()
        {
            var read = new byte[1];
            bh1745.WriteRead(new byte[] { BLUE_DATA }, read);
            return read[0];
            //bh1745.ReadUShort(BLUE_DATA);
        }
        /// <summary>
        /// Reads the clear data register of the sensor
        /// </summary>
        /// <returns></returns>
        public ushort ReadClearDataRegister()
        {
            var read = new byte[1];
            bh1745.WriteRead(new byte[] { CLEAR_DATA }, read);
            return read[0];
            //bh1745.ReadUShort(CLEAR_DATA);
        }
        /// <summary>
        /// Gets the compensated color reading from the sensor
        /// </summary>
        /// <returns></returns>
        public Color GetColor()
        {
            var clearData = ReadClearDataRegister();

            if (clearData == 0) { return Color.Black; }

            // apply channel multipliers and normalize
            double compensatedRed = ReadRedDataRegister() * CompensationMultipliers.Red / (int)MeasurementTime * 360;
            double compensatedGreen = ReadGreenDataRegister() * CompensationMultipliers.Green / (int)MeasurementTime * 360;
            double compensatedBlue = ReadBlueDataRegister() * CompensationMultipliers.Blue / (int)MeasurementTime * 360;
            double compensatedClear = clearData * CompensationMultipliers.Clear / (int)MeasurementTime * 360;

            // scale relative to clear
            int red = (int)Math.Min(255, compensatedRed / compensatedClear * 255);
            int green = (int)Math.Min(255, compensatedGreen / compensatedClear * 255);
            int blue = (int)Math.Min(255, compensatedBlue / compensatedClear * 255);

            return Color.FromRgb(red, green, blue);
        }

        #endregion

        #region Classes

        /// <summary>
        /// Channel compensation multipliers used to compensate the four (4) color channels of the Bh1745
        /// </summary>
        public class ChannelMultipliers
        {
            /// <summary>
            /// Multiplier for the red color channel
            /// </summary>
            public double Red { get; set; } = 1;
            /// <summary>
            /// Multiplier for the green color channel
            /// </summary>
            public double Green { get; set; } = 1;
            /// <summary>
            /// Multiplier for the blue color channel
            /// </summary>
            public double Blue { get; set; } = 1;
            /// <summary>
            /// Multiplier for the clear color channel.
            /// </summary>
            public double Clear { get; set; } = 1;
        }

        #endregion
    }
}