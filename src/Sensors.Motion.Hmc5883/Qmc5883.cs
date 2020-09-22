using GHIElectronics.TinyCLR.Devices.I2c;
using Meadow.TinyCLR.Modules.Spatial;
using System;
using System.Diagnostics;
using System.Threading;

namespace Meadow.TinyCLR.Sensors.Motion
{
    public class Qmc5883 : Hmc5883
    {
        /// <summary>
        /// QMC5883L Default I2C Address
        /// 
        /// This driver is untested
        /// </summary>
        new public const byte DefaultI2cAddress = 0x0D;

        public Qmc5883(string i2cBus, byte address = 0x0D,
            Gain gain = Gain.Gain1090,
            MeasuringMode measuringMode = MeasuringMode.Continuous,
            OutputRate outputRate = OutputRate.Rate15,
            SamplesAmount samplesAmount = SamplesAmount.One,
            MeasurementConfiguration measurementConfig = MeasurementConfiguration.Normal)
        {
            var settings = new I2cConnectionSettings(address, 100_000); //The slave's address and the bus speed.
            var controller = I2cController.FromName(i2cBus);
            i2cPeripheral = controller.GetDevice(settings);
            //i2cPeripheral = new I2cPeripheral(i2cBus, address);

            base.gain = (byte)gain;
            base.measuringMode = (byte)measuringMode;
            base.outputRate = (byte)outputRate;
            sampleAmount = (byte)samplesAmount;
            base.measurementConfig = (byte)measurementConfig;

            Initialize();
        }

        override protected void Initialize()
        {
            i2cPeripheral.Write(new byte[] { 0x0B, 0x01 });

            Thread.Sleep(50);

            i2cPeripheral.Write(new byte[] { 0x20, 0x40 });

            Thread.Sleep(50);

            i2cPeripheral.Write(new byte[] { 0x21, 0x01 });

            Thread.Sleep(50);

            i2cPeripheral.Write(new byte[] { 0x09, 0x0D });
        }

        public Vector GetDirection()
        {
            var data = new byte[6];
            i2cPeripheral.WriteRead(new byte[] { 0x00 }, data);

            ushort X = (ushort)(data[0] | data[1] << 8);
            ushort Y = (ushort)(data[2] | data[3] << 8);
            ushort Z = (ushort)(data[4] | data[5] << 8);

            var v = new Vector(X, Y, Z);

            Debug.WriteLine($"{X}, {Y}, {Z} : {DirectionToHeading(v)}");

            return v;
        }
    }
}