using System;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.I2c;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Peripherals.Sensors.Temperature;

namespace Meadow.Foundation.Sensors.Temperature
{
    /// <summary>
    /// TMP102 Temperature sensor object.
    /// </summary>    
    public class Tmp102 :
       
        IAtmosphericSensor, ITemperatureSensor
    {
        #region Enums

        /// <summary>
        ///     Indicate the resolution of the sensor.
        /// </summary>
        public enum Resolution : byte
        {
            /// <summary>
            ///     Operate in 12-bit mode.
            /// </summary>
            Resolution12Bits,

            /// <summary>
            ///     Operate in 13-bit mode.
            /// </summary>
            Resolution13Bits
        }

        #endregion Enums

        #region Member variables / fields

        /// <summary>
        ///     TMP102 sensor.
        /// </summary>
        private readonly I2cDevice tmp102;

        #endregion Member variables / fields

        #region Properties

        /// <summary>
        ///     Backing variable for the SensorResolution property.
        /// </summary>
        private Resolution _sensorResolution;

        /// <summary>
        ///     Get / set the resolution of the sensor.
        /// </summary>
        public Resolution SensorResolution {
            get { return _sensorResolution; }
            set {
                var configuration = new byte[2];
                tmp102.WriteRead(new byte[] { 0x01  }, configuration);
                //var configuration = tmp102.ReadRegisters(0x01, 2);
                if (value == Resolution.Resolution12Bits) {
                    configuration[1] &= 0xef;
                } else {
                    configuration[1] |= 0x10;
                }
                var data = new byte[] { 0x01, configuration[0],configuration[1] };
                tmp102.Write(data);
                _sensorResolution = value;
            }
        }

        /// <summary>
        /// The temperature, in degrees celsius (°C), from the last reading.
        /// </summary>
        public float Temperature => Conditions.Temperature;

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

        #endregion Properties

        #region Events and delegates

        public event AtmosphericConditionChangeEventHandler Updated;

        #endregion Events and delegates

        #region Constructors

        /// <summary>
        ///     Create a new TMP102 object using the default configuration for the sensor.
        /// </summary>
        /// <param name="address">I2C address of the sensor.</param>
        public Tmp102(string i2cBus, byte address = 0x48)
        {
            var settings = new I2cConnectionSettings(address, 100_000); //The slave's address and the bus speed.
            var controller = I2cController.FromName(i2cBus);
            tmp102 = controller.GetDevice(settings);
            //tmp102 = new I2cPeripheral(i2cBus, address);

            var configuration = new byte[2];
            tmp102.WriteRead(new byte[] { 0x01 }, configuration);
            //var configuration = tmp102.ReadRegisters(0x01, 2);

            _sensorResolution = (configuration[1] & 0x10) > 0 ?
                                 Resolution.Resolution13Bits : Resolution.Resolution12Bits;
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

                //SamplingTokenSource = new CancellationTokenSource();
                //CancellationToken ct = SamplingTokenSource.Token;

                AtmosphericConditions oldConditions;
                AtmosphericConditionChangeResult result;
                var task1 = new Thread(new ThreadStart (() => {
                    while (true) {
                        if (!IsSampling) {
                            // do task clean up here
                            //_observers.ForEach(x => x.OnCompleted());
                            break;
                        }
                        // capture history
                        oldConditions = AtmosphericConditions.From(Conditions);

                        // read
                        Update(); //syncrhnous for this driver 

                        // build a new result with the old and new conditions
                        result = new AtmosphericConditionChangeResult(oldConditions, Conditions);

                        // let everyone know
                        RaiseChangedAndNotify(result);

                        // sleep for the appropriate interval
                        Thread.Sleep(standbyDuration);
                    }
                }));
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
                //if (!IsSampling) return;

                //SamplingTokenSource?.Cancel();

                // state muh-cheen
                IsSampling = false;
            }
        }

        /// <summary>
        ///     Update the Temperature property.
        /// </summary>
        public void Update()
        {
            var temperatureData = new byte[2];
            tmp102.WriteRead(new byte[] { 0x00 }, temperatureData);
          
            //var temperatureData = tmp102.ReadRegisters(0x00, 2);

            var sensorReading = 0;
            if (SensorResolution == Resolution.Resolution12Bits) {
                sensorReading = (temperatureData[0] << 4) | (temperatureData[1] >> 4);
            } else {
                sensorReading = (temperatureData[0] << 5) | (temperatureData[1] >> 3);
            }
            Conditions.Temperature = (float)(sensorReading * 0.0625);
        }

        #endregion Methods
    }
}