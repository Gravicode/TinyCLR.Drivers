﻿using System;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.I2c;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Peripherals.Sensors.Temperature;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Grove TH02 temperature and humidity sensor.
    /// </summary>
    public class GroveTh02 :
        
        IAtmosphericSensor, ITemperatureSensor, IHumiditySensor
    {
        #region Constants

        /// <summary>
        ///     Start measurement bit in the configuration register.
        /// </summary>
        private const byte StartMeasurement = 0x01;

        /// <summary>
        ///     Measure temperature bit in the configuration register.
        /// </summary>
        private const byte MeasureTemperature = 0x10;

        /// <summary>
        ///     Heater control bit in the configuration register.
        /// </summary>
        private const byte HeaterOnBit = 0x02;

        /// <summary>
        ///     Mask used to turn the heater off.
        /// </summary>
        private const byte HeaterMask = 0xfd;

        /// <summary>
        ///     Minimum value that should be used for the polling frequency.
        /// </summary>
        public const ushort MinimumPollingPeriod = 200;

        #endregion

        #region Class

        /// <summary>
        ///     Register addresses in the Grove TH02 sensor.
        /// </summary>
        private class Registers
        {
            /// <summary>
            ///     Status register.
            /// </summary>
            public const byte Status = 0x00;

            /// <summary>
            ///     High byte of the data register.
            /// </summary>
            public const byte DataHigh = 0x01;

            /// <summary>
            ///     Low byte of the data register.
            /// </summary>
            public const byte DataLow = 0x02;

            /// <summary>
            ///     Addess of the configuration register.
            /// </summary>
            public const byte Config = 0x04;

            /// <summary>
            ///     Address of the ID register.
            /// </summary>
            public const byte ID = 0x11;
        }

        #endregion Class

        #region Member variables / fields

        /// <summary>
        ///     GroveTH02 object.
        /// </summary>
        private readonly I2cDevice groveTH02;

        /// <summary>
        ///     Update interval in milliseconds
        /// </summary>
        private readonly ushort _updateInterval = 100;

        #endregion Member variables / fields

        #region Properties
        public TempUpdateData UpdateData { get; set; }
        /// <summary>
        /// The temperature, in degrees celsius (°C), from the last reading.
        /// </summary>
        public float Temperature => Conditions.Temperature;

        /// <summary>
        /// The humidity, in percent relative humidity, from the last reading..
        /// </summary>
        public float Humidity => Conditions.Humidity;

        /// <summary>
        ///     Get / set the heater status.
        /// </summary>
        public bool HeaterOn {
            get {
                var readByte = new byte[1];
                groveTH02.WriteRead(new byte[] { Registers.Config }, readByte);
                return ((readByte[0] & HeaterOnBit) > 0);
            }
            set {
                var config = new byte[1];
                groveTH02.WriteRead(new byte[] { Registers.Config }, config);
             
                if (value) {
                    config[0] |= HeaterOnBit;
                } else {
                    config[0] &= HeaterMask;
                }
                groveTH02.Write(new byte[] { Registers.Config, config[0] });
            }
        }

        /// <summary>
        /// The AtmosphericConditions from the last reading.
        /// </summary>
        public AtmosphericConditions Conditions { get; protected set; } = new AtmosphericConditions();

        // internal thread lock
        private object _lock = new object();
        //private CancellationTokenSource SamplingTokenSource;

        /// <summary>
        /// Gets a value indicating whether the analog input port is currently
        /// sampling the ADC. Call StartSampling() to spin up the sampling process.
        /// </summary>
        /// <value><c>true</c> if sampling; otherwise, <c>false</c>.</value>
        public bool IsSampling { get; protected set; } = false;

        #endregion

        #region Events and delegates

        //public event EventHandler<AtmosphericConditionChangeResult> Updated;
        public event AtmosphericConditionChangeEventHandler Updated = delegate { };
        #endregion Events and delegates

        #region Constructors

        /// <summary>
        ///     Create a new GroveTH02 object using the default parameters for the component.
        /// </summary>
        /// <param name="address">Address of the Grove TH02 (default = 0x4-).</param>
        /// <param name="i2cBus">I2C bus (default = 100 KHz).</param>
        public GroveTh02(string i2cBus, byte address = 0x40)
        {
            var settings = new I2cConnectionSettings(address, 100_000); //The slave's address and the bus speed.
            var controller = I2cController.FromName(i2cBus);
            groveTH02 = controller.GetDevice(settings);
            //groveTH02 = new I2cPeripheral(i2cBus, address);
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Convenience method to get the current sensor readings. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        public AtmosphericConditions Read()
        {
            Conditions = Read();

            return Conditions;
        }

        public void StartUpdating(int standbyDuration = 1000)
        {
            // thread safety
            lock (_lock) {
                if (IsSampling) return;

                // state muh-cheen
                IsSampling = true;

                if (UpdateData == null)
                {
                    UpdateData = new TempUpdateData();

                }

                UpdateData.standbyDuration = standbyDuration;
                Thread task1 = new Thread(new ThreadStart(UpdateTemp));
                task1.Start();
               
            }
        }
        void UpdateTemp()
        {

            AtmosphericConditions oldConditions;
            AtmosphericConditionChangeResult result;
           
                while (true)
                {
                    if (!IsSampling)
                    {
                        // do task clean up here
                        //_observers.ForEach(x => x.OnCompleted());
                        break;
                    }
                    // capture history
                    oldConditions = AtmosphericConditions.From(Conditions);

                    // read
                    Update();

                    // build a new result with the old and new conditions
                    result = new AtmosphericConditionChangeResult(oldConditions, Conditions);

                    // let everyone know
                    RaiseChangedAndNotify(result);

                    // sleep for the appropriate interval
                    Thread.Sleep(UpdateData.standbyDuration);
                }
           
        }
        protected void RaiseChangedAndNotify(AtmosphericConditionChangeResult changeResult)
        {
            Updated?.Invoke(this, changeResult);
            //base.NotifyObservers(changeResult);
        }

        /// <summary>
        /// Stops sampling the temperature.
        /// </summary>
        public void StopUpdating()
        {
            lock (_lock) {
                if (!IsSampling) return;

                //SamplingTokenSource?.Cancel();

                // state muh-cheen
                IsSampling = false;
            }
        }

        /// <summary>
        ///     Force the sensor to make a reading and update the relevant properties.
        /// </summary>
        void Update()
        {
            int temp = 0;
            //
            //  Get the humidity first.
            //
            groveTH02.Write(new byte[] { Registers.Config, StartMeasurement });
            //
            //  Maximum conversion time should be 40ms but loop just in case 
            //  it takes longer.
            //

            Thread.Sleep(40);

            while (WaitStatus()) ;
            byte[] data = new byte[2];
            groveTH02.WriteRead(new byte[] { Registers.DataHigh }, data);
            temp = data[0] << 8;
            temp |= data[1];
            temp >>= 4;
            Conditions.Humidity = (((float)temp) / 16) - 24;
            //
            //  Now get the temperature.
            //
            groveTH02.Write(new byte[] { Registers.Config, StartMeasurement | MeasureTemperature });
            //
            //  Maximum conversion time should be 40ms but loop just in case 
            //  it takes longer.
            //
            Thread.Sleep(40);

            while (WaitStatus()) ;
            data = new byte[2];
            groveTH02.WriteRead(new byte[] { Registers.DataHigh }, data);
            temp = data[0] << 8;
            temp |= data[1];
            temp >>= 2;
            Conditions.Temperature = (((float)temp) / 32) - 50;
        }
        bool WaitStatus()
        {
            var readByte = new byte[1];
            groveTH02.WriteRead(new byte[] { Registers.Status }, readByte);
            return (readByte[1] & 0x01) > 0;
        }
        #endregion
    }
    public class TempUpdateData
    {
        public AtmosphericConditions oldConditions;
        public AtmosphericConditionChangeResult result;

        public int standbyDuration = 1000;
    }
}
