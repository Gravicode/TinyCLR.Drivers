using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;
using Meadow.Peripherals.Sensors.Distance;
using System;
using System.Threading;


namespace Meadow.TinyCLR.Sensors.Distance
{
    /// <summary>
    /// Represents the Vl53l0x distance sensor
    /// </summary>
    /// <remarks>Based on logic from https://github.com/adafruit/Adafruit_CircuitPython_VL53L0X/blob/master/adafruit_vl53l0x.py </remarks>
    public class Vl53l0x : IRangeFinder
    {
        #region const

        protected const byte RangeStart = 0x00;
        protected const byte SystemThreahHigh = 0x0C;
        protected const byte SystemThreshLow = 0x0E;
        protected const byte SystemSequenceConfig = 0x01;
        protected const byte SystemRangeConfig = 0x09;
        protected const byte SystemIntermeasurementPeriod = 0x04;
        protected const byte SystemInterruptConfigGpio = 0x0A;
        protected const byte GpioHvMuxActiveHigh = 0x84;
        protected const byte SystemInterruptClear = 0x0B;
        protected const byte ResultInterruptStatus = 0x13;
        protected const byte ResultRangeStatus = 0x14;
        protected const byte ResultCoreAmbientWindowEventsRtn = 0xBC;
        protected const byte ResultCoreRangingTotalEventsRtn = 0xC0;
        protected const byte ResultCoreAmbientWindowEventsRef = 0xD0;
        protected const byte ResultCoreRangingTotalEventsRef = 0xD4;
        protected const byte ResultPeakSignalRateRef = 0xB6;
        protected const byte AlgoPartToPartRangeOffsetMm = 0x28;
        protected const byte I2CSlaveDeviceAddress = 0x8A;
        protected const byte MsrcConfigControl = 0x60;
        protected const byte PreRangeConfigMinSnr = 0x27;
        protected const byte PreRangeConfigValidPhaseLow = 0x56;
        protected const byte PreRangeConfigValidPhaseHigh = 0x57;
        protected const byte PreRangeMinCountRateRtnLimit = 0x64;
        protected const byte FinalRangeConfigMinSnr = 0x67;
        protected const byte FinalRangeConfigValidPhaseLow = 0x47;
        protected const byte FinalRangeConfigValidPhaseHigh = 0x48;
        protected const byte FinalRangeConfigMinCountRateRtnLimit = 0x44;
        protected const byte PreRangeConfigSigmaThreshHi = 0x61;
        protected const byte PreRangeConfigSigmaThreshLo = 0x62;
        protected const byte PreRangeConfigVcselPeriod = 0x50;
        protected const byte PreRangeConfigTimeoutMacropHi = 0x51;
        protected const byte PreRangeConfigTimeoutMacropLo = 0x52;
        protected const byte SystemHistogramBin = 0x81;
        protected const byte HistogramConfigInitialPhaseSelect = 0x33;
        protected const byte HistogramConfigReadoutCtrl = 0x55;
        protected const byte FinalRangeConfigVcselPeriod = 0x70;
        protected const byte FinalRangeConfigTimeoutMacropHi = 0x71;
        protected const byte FinalRangeConfigTimeoutMacropLo = 0x72;
        protected const byte CrosstalkCompensationPeakRateMcps = 0x20;
        protected const byte MsrcConfigTimeoutMacrop = 0x46;
        protected const byte SoftResetGo2SoftResetN = 0xBF;
        protected const byte IdentificationModelId = 0xC0;
        protected const byte IdentificationRevisionId = 0xC2;
        protected const byte OscCalibrateVal = 0xF8;
        protected const byte GlobalConfigVcselWidth = 0x32;
        protected const byte GlobalConfigSpadEnablesRef0 = 0xB0;
        protected const byte GlobalConfigSpadEnablesRef1 = 0xB1;
        protected const byte GlobalConfigSpadEnablesRef2 = 0xB2;
        protected const byte GlobalConfigSpadEnablesRef3 = 0xB3;
        protected const byte GlobalConfigSpadEnablesRef4 = 0xB4;
        protected const byte GlobalConfigSpadEnablesRef5 = 0xB5;
        protected const byte GlobalConfigRefEnStartSelect = 0xB6;
        protected const byte DynamicSpadNumRequestedRefSpad = 0x4E;
        protected const byte DynamicSpadRefEnStartOffset = 0x4F;
        protected const byte PowerManagementGo1PowerForce = 0x80;
        protected const byte VhvConfigPadSclSdaExtsupHv = 0x89;
        protected const byte AlgoPhasecalLim = 0x30;
        protected const byte AlgoPhasecalConfigTimeout = 0x30;
        protected const int VcselPeriodPreRange = 0;
        protected const int VcselPeriodFinalRange = 1;

        #endregion

        public enum UnitType
        {
            mm,
            cm,
            inches
        }

        public UnitType Units { get; set; }
        public bool IsShutdown
        {
            get
            {
                if (_shutdownPin != null)
                {
                    return !(_shutdownPin.Read() == GpioPinValue.High);
                }
                else
                {
                    return false;
                }
            }
        }

        public float CurrentDistance { get; private set; } = -1;

        /// <summary>
        /// Minimum valid distance in mm.
        /// </summary>
        public float MinimumDistance => 30;

        /// <summary>
        /// Maximum valid distance in mm (CurrentDistance returns -1 if above).
        /// </summary>
        public float MaximumDistance => 2000;

        I2cDevice _i2cPeripheral;
        GpioPin _shutdownPin;
        byte _adddress;
        byte _stopVariable;
        byte _configControl;
        float _signalRateLimit;

        public event DistanceEventHandler DistanceDetected;

        public Vl53l0x(string i2cBus, byte address = 0x29, UnitType units = UnitType.mm)
        {
            // : this (i2cBus,null,address,units)
            Setup(i2cBus, null, address, units);

        }

        /// <param name="i2cBus">I2C bus</param>
        /// <param name="address">VL53L0X address</param>
        /// <param name="units">Unit of measure</param>
        public Vl53l0x(string i2cBus, GpioPin shutdownPin, byte address = 0x29, UnitType units = UnitType.mm)
        {
            Setup(i2cBus, shutdownPin, address, units);
        }

        void Setup(string i2cBus, GpioPin shutdownPin, byte address = 0x29, UnitType units = UnitType.mm)
        {
            _adddress = address;
            var settings = new I2cConnectionSettings(address, 100_000); //The slave's address and the bus speed.
            var controller = I2cController.FromName(i2cBus);
            _i2cPeripheral = controller.GetDevice(settings);
            //_i2cPeripheral = new I2cPeripheral(i2cBus, _adddress);
            _shutdownPin = shutdownPin;
            Units = units;
        }

        /// <summary>
        /// Initializes the VL53L0X
        /// </summary>
        public void Initialize()
        {
            if (IsShutdown)
                Shutdown(false);

            if (Read(0xC0) != 0xEE || Read(0xC1) != 0xAA || Read(0xC2) != 0x10)
            {
                throw new Exception("Failed to find expected ID register values");
            }

            _i2cPeripheral.Write(new byte[] { 0x88, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0x80, 0x01 });
            _i2cPeripheral.Write(new byte[] { 0xFF, 0x01 });
            _i2cPeripheral.Write(new byte[] { 0x00, 0x00 });

            _stopVariable = Read(0x91);

            _i2cPeripheral.Write(new byte[] { 0x00, 0x01 });
            _i2cPeripheral.Write(new byte[] { 0xFF, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0x80, 0x00 });

            _configControl = ((byte)(Read(MsrcConfigControl) | 0x12));
            _signalRateLimit = 0.25f;

            _i2cPeripheral.Write(new byte[] { SystemSequenceConfig, 0xFF });
            GetSpadInfo(out int val1, out bool val2);
            int spad_count = val1;
            bool spad_is_aperture = val2;

            byte[] ref_spad_map = new byte[7];
            ref_spad_map[0] = GlobalConfigSpadEnablesRef0;

            _i2cPeripheral.Write(new byte[] { 0xFF, 0x01 });
            _i2cPeripheral.Write(new byte[] { DynamicSpadRefEnStartOffset, 0x00 });
            _i2cPeripheral.Write(new byte[] { DynamicSpadNumRequestedRefSpad, 0x2C });
            _i2cPeripheral.Write(new byte[] { 0xFF, 0x00 });
            _i2cPeripheral.Write(new byte[] { GlobalConfigRefEnStartSelect, 0xB4 });

            var first_spad_to_enable = (spad_is_aperture) ? 12 : 0;
            var spads_enabled = 0;

            for (int i = 0; i < 48; i++)
            {
                if (i < first_spad_to_enable || spads_enabled == spad_count)
                {
                    ref_spad_map[1 + (i / 8)] &= (byte)~(1 << (i % 8));
                }
                else if ((ref_spad_map[1 + (i / 8)] >> (byte)((i % 8)) & 0x1) > 0)
                {
                    spads_enabled += 1;
                }
            }

            _i2cPeripheral.Write(new byte[] { 0xFF, 0x01 });
            _i2cPeripheral.Write(new byte[] { 0x00, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0xFF, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0x09, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0x10, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0x11, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0x24, 0x01 });
            _i2cPeripheral.Write(new byte[] { 0x25, 0xFF });
            _i2cPeripheral.Write(new byte[] { 0x75, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0xFF, 0x01 });
            _i2cPeripheral.Write(new byte[] { 0x4E, 0x2C });
            _i2cPeripheral.Write(new byte[] { 0x48, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0x30, 0x20 });
            _i2cPeripheral.Write(new byte[] { 0xFF, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0x30, 0x09 });
            _i2cPeripheral.Write(new byte[] { 0x54, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0x31, 0x04 });
            _i2cPeripheral.Write(new byte[] { 0x32, 0x03 });
            _i2cPeripheral.Write(new byte[] { 0x40, 0x83 });
            _i2cPeripheral.Write(new byte[] { 0x46, 0x25 });
            _i2cPeripheral.Write(new byte[] { 0x60, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0x27, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0x50, 0x06 });
            _i2cPeripheral.Write(new byte[] { 0x51, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0x52, 0x96 });
            _i2cPeripheral.Write(new byte[] { 0x56, 0x08 });
            _i2cPeripheral.Write(new byte[] { 0x57, 0x30 });
            _i2cPeripheral.Write(new byte[] { 0x61, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0x62, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0x64, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0x65, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0x66, 0xA0 });
            _i2cPeripheral.Write(new byte[] { 0xFF, 0x01 });
            _i2cPeripheral.Write(new byte[] { 0x22, 0x32 });
            _i2cPeripheral.Write(new byte[] { 0x47, 0x14 });
            _i2cPeripheral.Write(new byte[] { 0x49, 0xFF });
            _i2cPeripheral.Write(new byte[] { 0x4A, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0xFF, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0x7A, 0x0A });
            _i2cPeripheral.Write(new byte[] { 0x7B, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0x78, 0x21 });
            _i2cPeripheral.Write(new byte[] { 0xFF, 0x01 });
            _i2cPeripheral.Write(new byte[] { 0x23, 0x34 });
            _i2cPeripheral.Write(new byte[] { 0x42, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0x44, 0xFF });
            _i2cPeripheral.Write(new byte[] { 0x45, 0x26 });
            _i2cPeripheral.Write(new byte[] { 0x46, 0x05 });
            _i2cPeripheral.Write(new byte[] { 0x40, 0x40 });
            _i2cPeripheral.Write(new byte[] { 0x0E, 0x06 });
            _i2cPeripheral.Write(new byte[] { 0x20, 0x1A });
            _i2cPeripheral.Write(new byte[] { 0x43, 0x40 });
            _i2cPeripheral.Write(new byte[] { 0xFF, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0x34, 0x03 });
            _i2cPeripheral.Write(new byte[] { 0x35, 0x44 });
            _i2cPeripheral.Write(new byte[] { 0xFF, 0x01 });
            _i2cPeripheral.Write(new byte[] { 0x31, 0x04 });
            _i2cPeripheral.Write(new byte[] { 0x4B, 0x09 });
            _i2cPeripheral.Write(new byte[] { 0x4C, 0x05 });
            _i2cPeripheral.Write(new byte[] { 0x4D, 0x04 });
            _i2cPeripheral.Write(new byte[] { 0xFF, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0x44, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0x45, 0x20 });
            _i2cPeripheral.Write(new byte[] { 0x47, 0x08 });
            _i2cPeripheral.Write(new byte[] { 0x48, 0x28 });
            _i2cPeripheral.Write(new byte[] { 0x67, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0x70, 0x04 });
            _i2cPeripheral.Write(new byte[] { 0x71, 0x01 });
            _i2cPeripheral.Write(new byte[] { 0x72, 0xFE });
            _i2cPeripheral.Write(new byte[] { 0x76, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0x77, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0xFF, 0x01 });
            _i2cPeripheral.Write(new byte[] { 0x0D, 0x01 });
            _i2cPeripheral.Write(new byte[] { 0xFF, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0x80, 0x01 });
            _i2cPeripheral.Write(new byte[] { 0x01, 0xF8 });
            _i2cPeripheral.Write(new byte[] { 0xFF, 0x01 });
            _i2cPeripheral.Write(new byte[] { 0x8E, 0x01 });
            _i2cPeripheral.Write(new byte[] { 0x00, 0x01 });
            _i2cPeripheral.Write(new byte[] { 0xFF, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0x80, 0x00 });
            _i2cPeripheral.Write(new byte[] { SystemInterruptConfigGpio, 0x04 });
            var gpio_hv_mux_active_high = Read(GpioHvMuxActiveHigh);
            _i2cPeripheral.Write(new byte[] { GpioHvMuxActiveHigh, (byte)(gpio_hv_mux_active_high & ~0x10) });
            _i2cPeripheral.Write(new byte[] { GpioHvMuxActiveHigh, 0x01 });
            _i2cPeripheral.Write(new byte[] { SystemSequenceConfig, 0xE8 });
            _i2cPeripheral.Write(new byte[] { SystemSequenceConfig, 0x01 });
            PerformSingleRefCalibration(0x40);
            _i2cPeripheral.Write(new byte[] { SystemSequenceConfig, 0x02 });
            PerformSingleRefCalibration(0x00);

            _i2cPeripheral.Write(new byte[] { SystemSequenceConfig, 0xE8 });
        }

        /// <summary>
        /// Returns the current distance/range
        /// </summary>
        /// <returns>The distance in the specified Units. Default mm. Returns -1 if the shutdown pin is used and is off</returns>
        public virtual float Range()
        {
            if (IsShutdown)
                return -1;

            var dist = GetRange();

            if (dist > MaximumDistance)
                CurrentDistance = -1;
            else if (Units == UnitType.inches)
                CurrentDistance = dist * 0.0393701f;
            else if (Units == UnitType.cm)
                CurrentDistance = dist / 10;
            else
                CurrentDistance = dist;

            return CurrentDistance;
        }

        /// <summary>
        /// Set a new I2C address
        /// </summary>
        /// <param name="newAddress"></param>
        public virtual void SetAddress(byte newAddress)
        {
            if (IsShutdown)
                return;

            byte address = (byte)(newAddress & 0x7F);
            _i2cPeripheral.Write(new byte[] { I2CSlaveDeviceAddress, address });
        }

        /// <summary>
        /// Set the Shutdown state of the device
        /// </summary>
        /// <param name="state">true = off/shutdown. false = on</param>
        public virtual void Shutdown(bool state)
        {
            if (_shutdownPin == null)
                return;

            _shutdownPin.Write(!state ? GpioPinValue.High : GpioPinValue.Low);
            Thread.Sleep(2);
            if (state == false)
            {
                Initialize();
                Thread.Sleep(2);
            }
        }

        protected void GetSpadInfo(out int Val1, out bool Val2)
        {
            _i2cPeripheral.Write(new byte[] { 0x80, 0x01 });
            _i2cPeripheral.Write(new byte[] { 0xFF, 0x01 });
            _i2cPeripheral.Write(new byte[] { 0x00, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0xFF, 0x06 });

            var result = (byte)(Read(0x83) | 0x04);
            _i2cPeripheral.Write(new byte[] { 0x83, result });
            _i2cPeripheral.Write(new byte[] { 0xFF, 0x07 });
            _i2cPeripheral.Write(new byte[] { 0x81, 0x01 });
            _i2cPeripheral.Write(new byte[] { 0x94, 0x6B });
            _i2cPeripheral.Write(new byte[] { 0x83, 0x00 });

            int tCount = 0;
            while (Read(0x83) == 0x00)
            {
                Thread.Sleep(5);
                tCount++;
                if (tCount > 100)
                {
                    throw new Exception("Timeout");
                }
            }

            _i2cPeripheral.Write(new byte[] { 0x83, 0x01 });
            var tmp = Read(0x92);
            var count = tmp & 0x7F;
            var is_aperture = ((tmp >> 7) & 0x01) == 1;

            _i2cPeripheral.Write(new byte[] { 0x81, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0xFF, 0x06 });

            var t = (byte)(Read(0x83) & ~0x04);
            _i2cPeripheral.Write(new byte[] { 0xFF, t });
            _i2cPeripheral.Write(new byte[] { 0xFF, 0x01 });
            _i2cPeripheral.Write(new byte[] { 0x00, 0x01 });
            _i2cPeripheral.Write(new byte[] { 0xFF, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0x80, 0x00 });

            Val1 = count;
            Val2 = is_aperture;

        }

        protected virtual void PerformSingleRefCalibration(byte vhvInitByte)
        {
            _i2cPeripheral.Write(new byte[] { RangeStart, (byte)(0x01 | vhvInitByte & 0xFF) });

            int tCount = 0;
            while ((byte)(Read(ResultInterruptStatus) & 0x07) == 0)
            {
                Thread.Sleep(5);
                tCount++;
                if (tCount > 100)
                    throw new Exception("Interrupt Status Timeout");
            }

            _i2cPeripheral.Write(new byte[] { GpioHvMuxActiveHigh, 0x01 });
            _i2cPeripheral.Write(new byte[] { RangeStart, 0x00 });
        }

        protected virtual byte Read(byte address)
        {
            var result = new byte[1];
            _i2cPeripheral.WriteRead(new byte[] { address }, result);
            return result[0];
        }

        protected virtual int Read16(byte address)
        {
            var result = new byte[2];
            _i2cPeripheral.WriteRead(new byte[] { address }, result);

            return (result[0] << 8) | result[1];
        }

        protected virtual int GetRange()
        {
            _i2cPeripheral.Write(new byte[] { 0x80, 0x01 });
            _i2cPeripheral.Write(new byte[] { 0xFF, 0x01 });
            _i2cPeripheral.Write(new byte[] { 0x00, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0x91, _stopVariable });
            _i2cPeripheral.Write(new byte[] { 0x00, 0x01 });
            _i2cPeripheral.Write(new byte[] { 0xFF, 0x00 });
            _i2cPeripheral.Write(new byte[] { 0x80, 0x00 });
            _i2cPeripheral.Write(new byte[] { RangeStart, 0x01 });

            int tCount = 0;
            while ((byte)(Read(RangeStart) & 0x01) > 0)
            {
                Thread.Sleep(5);
                tCount++;
                if (tCount > 100)
                {
                    throw new Exception("VL53L0X Range Start Timeout");
                }
            }

            tCount = 0;
            while ((byte)(Read(ResultInterruptStatus) & 0x07) == 0)
            {
                Thread.Sleep(5);
                tCount++;
                if (tCount > 100)
                {
                    throw new Exception("VL53L0X Interrupt Status Timeout");
                }
            }

            var range_mm = Read16(ResultRangeStatus + 10);
            _i2cPeripheral.Write(new byte[] { GpioHvMuxActiveHigh, 0x01 });

            return range_mm;
        }
    }
}