﻿//using Meadow.Hardware;
using GHIElectronics.TinyCLR.Devices.I2c;
using System;
//using System.Buffers.Binary;

namespace Meadow.TinyCLR.Sensors.Light
{
    public class Bh1750
    {
        #region Properties

        /// <summary>
        /// I2C address when address pin is high
        /// </summary>
        public static byte I2cAddressHigh => 0x5c;

        /// <summary>
        /// I2C address when address pin is low
        /// </summary>
        public static byte I2cAddressLow => 0x23;

        /// <summary>
        /// BH1750 Light Transmittance (27.20-222.50%)
        /// </summary>
        public double LightTransmittance
        {
            get => _lightTransmittance;
            set => SetLightTransmittance(_lightTransmittance = value);
        }
        private double _lightTransmittance;

        /// <summary>
        /// BH1750 Measuring Mode
        /// </summary>
        public MeasuringModes MeasuringMode { get; set; }

        #endregion

        #region Member variables / fields

        private I2cDevice bh1750;

        private const byte DefaultLightTransmittance = 0b_0100_0101;
        private const float MaxTransmittance = 2.225f;
        private const float MinTransmittance = 0.272f;

        #endregion Member variables / fields

        #region Enums

        /// <summary>
        /// The measuring mode of BH1750FVI
        /// </summary>
        public enum MeasuringModes : byte
        {
            // Details in the datasheet P5
            /// <summary>
            /// Start measurement at 1lx resolution
            /// Measurement Time is typically 120ms
            /// </summary>
            ContinuouslyHighResolutionMode = 0b_0001_0000,

            /// <summary>
            /// Start measurement at 0.5lx resolution
            /// Measurement Time is typically 120ms
            /// </summary>
            ContinuouslyHighResolutionMode2 = 0b_0001_0001,

            /// <summary>
            /// Start measurement at 4lx resolution
            /// Measurement Time is typically 16ms
            /// </summary>
            ContinuouslyLowResolutionMode = 0b_0001_0011,

            /// <summary>
            /// Start measurement at 1lx resolution once
            /// Measurement Time is typically 120ms
            /// Automatically set to powerdown mode after measurement
            /// </summary>
            OneTimeHighResolutionMode = 0b_0010_0000,

            /// <summary>
            /// Start measurement at 0.5lx resolution once
            /// Measurement Time is typically 120ms.
            /// It is automatically set to Power Down mode after measurement.
            /// </summary>
            OneTimeHighResolutionMode2 = 0b_0010_0001,

            /// <summary>
            /// Start measurement at 4lx resolution once
            /// Measurement Time is typically 16ms.
            /// It is automatically set to Power Down mode after measurement.
            /// </summary>
            OneTimeLowResolutionMode = 0b_0010_0011
        }

        internal enum Command : byte
        {
            PowerDown = 0b_0000_0000,
            PowerOn = 0b_0000_0001,
            Reset = 0b_0000_0111,
            MeasurementTimeHigh = 0b_0100_0000,
            MeasurementTimeLow = 0b_0110_0000,
        }

        #endregion Enums

        #region Constructors

        /// <summary>
        ///     Create a new BH1750 light sensor object using a static reference voltage.
        /// </summary>
        public Bh1750(string i2cBus, byte address, MeasuringModes measuringMode = MeasuringModes.ContinuouslyHighResolutionMode, double lightTransmittance = 1)
        {
            var settings = new I2cConnectionSettings(address, 100_000); //The slave's address and the bus speed.
            var controller = I2cController.FromName(i2cBus);
            bh1750 = controller.GetDevice(settings);
            //bh1750 = new I2cPeripheral(i2cBus, address);

            LightTransmittance = lightTransmittance;
            MeasuringMode = measuringMode;

            Initialize();
        }

        #endregion Constructors

        #region Methods

        private void Initialize()
        {
            bh1750.Write(new byte[] { (byte)Command.PowerOn });
            bh1750.Write(new byte[] { (byte)Command.Reset });
        }

        /// <summary>
        /// Set BH1750FVI Light Transmittance
        /// </summary>
        /// <param name="transmittance">Light Transmittance, from 27.20% to 222.50%</param>
        private void SetLightTransmittance(double transmittance)
        {
            if (transmittance > MaxTransmittance || transmittance < MinTransmittance)
            {
                throw new ArgumentOutOfRangeException(nameof(transmittance), $"{nameof(transmittance)} needs to be in the range of 27.20% to 222.50%.");
            }

            byte val = (byte)(DefaultLightTransmittance / transmittance);

            bh1750.Write(new byte[] { (byte)((byte)Command.MeasurementTimeHigh | (val >> 5)) });
            bh1750.Write(new byte[] { (byte)((byte)Command.MeasurementTimeLow | (val & 0b_0001_1111)) });
        }

        /// <summary>
        /// Get BH1750 Illuminance
        /// </summary>
        /// <returns>Illuminance (Lux)</returns>
        public double GetIlluminance()
        {
            if (MeasuringMode == MeasuringModes.OneTimeHighResolutionMode ||
                MeasuringMode == MeasuringModes.OneTimeHighResolutionMode2 ||
                MeasuringMode == MeasuringModes.OneTimeLowResolutionMode)
            {
                bh1750.Write(new byte[] { (byte)Command.PowerOn });
            }

            bh1750.Write(new byte[] { (byte)MeasuringMode });
            var data = new byte[2];
            bh1750.Read(data);

            ushort raw = BitConverter.ToUInt16(data,0);//BinaryPrimitives.ReadUInt16BigEndian(data);

            double result = raw / (1.2 * _lightTransmittance);

            if (MeasuringMode == MeasuringModes.ContinuouslyHighResolutionMode2 ||
                MeasuringMode == MeasuringModes.OneTimeHighResolutionMode2)
            {
                result *= 2;
            }

            return result;
        }

        #endregion
    }
}