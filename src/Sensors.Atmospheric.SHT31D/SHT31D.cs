using System;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.I2c;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Peripherals.Sensors.Temperature;

namespace Meadow.TinyCLR.Sensors.Atmospheric
{
    /// <summary>
    /// Provide a mechanism for reading the temperature and humidity from
    /// a SHT31D temperature / humidity sensor.
    /// </summary>
    /// <remarks>
    /// Readings from the sensor are made in Single-shot mode.
    /// </remarks>
    public class Sht31D :
        IAtmosphericSensor, ITemperatureSensor, IHumiditySensor
    {
        #region Member variables / fields

        /// <summary>
        ///     SH31D sensor communicates using I2C.
        /// </summary>
        private readonly I2cDevice sht31d;

        #endregion Member variables / fields

        #region Properties

        /// <summary>
        /// The temperature, in degrees celsius (°C), from the last reading.
        /// </summary>
        public float Temperature => Conditions.Temperature;

        /// <summary>
        /// The humidity, in percent relative humidity, from the last reading..
        /// </summary>
        public float Humidity => Conditions.Humidity;

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
        public TempUpdateData UpdateData { get; set; }
        #endregion Properties

        #region Events and delegates

        //public event EventHandler<AtmosphericConditionChangeResult> Updated;
        public event AtmosphericConditionChangeEventHandler Updated = delegate { };
        #endregion Events and delegates

        #region Constructors

        /// <summary>
        ///     Create a new SHT31D object.
        /// </summary>
        /// <param name="address">Sensor address (should be 0x44 or 0x45).</param>
        /// <param name="i2cBus">I2cBus (0-1000 KHz).</param>
        public Sht31D(string i2cBus, byte address = 0x44)
        {
            var settings = new I2cConnectionSettings(address, 100_000); //The slave's address and the bus speed.
            var controller = I2cController.FromName(i2cBus);
            sht31d = controller.GetDevice(settings);
            //sht31d = new I2cPeripheral(i2cBus, address);
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Convenience method to get the current sensor readings. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        public AtmosphericConditions Read()
        {
            Update();

            return Conditions;
        }

        public void StartUpdating(int standbyDuration = 1000)
        {
            // thread safety
            lock (_lock)
            {
                if (IsSampling) { return; }

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

            //SamplingTokenSource = new CancellationTokenSource();
            //CancellationToken ct = SamplingTokenSource.Token;

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
            lock (_lock)
            {
                if (!IsSampling) { return; }

                //SamplingTokenSource?.Cancel();

                // state muh-cheen
                IsSampling = false;
            }
        }

        /// <summary>
        ///     Get a reading from the sensor and set the Temperature and Humidity properties.
        /// </summary>
        public void Update()
        {
            var data = new byte[6];
            sht31d.WriteRead(new byte[] { 0x2c, 0x06 }, data);
            Conditions.Humidity = (100 * (float)((data[3] << 8) + data[4])) / 65535;
            Conditions.Temperature = ((175 * (float)((data[0] << 8) + data[1])) / 65535) - 45;
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