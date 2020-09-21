using System;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Adc;
using Meadow.Peripherals.Sensors.Moisture;
using Meadow.TinyCLR.Core;
using Meadow.TinyCLR.Modules.HallEffect;

namespace Meadow.TinyCLR.Sensors.Moisture
{
   
    /// <summary>
    /// Capacitive Soil Moisture Sensor
    /// </summary>
    public class Capacitive : IMoistureSensor
    {
        /// <summary>
        /// Raised when a new sensor reading has been made. To enable, call StartUpdating().
        /// </summary>
        public event FloatChangeResultHandler Updated = delegate { };
        AnalogSamplingSetting samplingSetting;
        #region Member Variables / fields

        // internal thread lock
        private object _lock = new object();

        #endregion

        #region Properties

        /// <summary>
        /// Returns the analog input port
        /// </summary>
        public AdcChannel AnalogInputPort { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether the analog input port is currently
        /// sampling the ADC. Call StartSampling() to spin up the sampling process.
        /// </summary>
        /// <value><c>true</c> if sampling; otherwise, <c>false</c>.</value>
        public bool IsSampling { get; protected set; } = false;

        /// <summary>
        /// Last value read from the moisture sensor.
        /// </summary>
        public float Moisture { get; protected set; }

        /// <summary>
        /// Voltage value of most dry soil 
        /// </summary>
        public float MinimumVoltageCalibration { get; set; }

        /// <summary>
        /// Voltage value of most moist soil
        /// </summary>
        public float MaximumVoltageCalibration { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a Capacitive soil moisture sensor object with the specified analog pin and a IO device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="analogPin"></param>
        public Capacitive(
            string AnalogControllerName,
            int analogPin,
            float minimumVoltageCalibration = 0f,
            float maximumVoltageCalibration = 3.3f) 
            
        {
            var adc = AdcController.FromName(AnalogControllerName);
            var analog = adc.OpenChannel(analogPin);
            Setup(analog, minimumVoltageCalibration, maximumVoltageCalibration);
        }

        /// <summary>
        /// Creates a Capacitive soil moisture sensor object with the especified AnalogInputPort.
        /// </summary>
        /// <param name="analogPort"></param>
        public Capacitive(
            AdcChannel analogPort,
            float minimumVoltageCalibration = 0f,
            float maximumVoltageCalibration = 3.3f)
        {
            Setup( analogPort,
               minimumVoltageCalibration,
               maximumVoltageCalibration );
        }
        void Setup(AdcChannel analogPort,
            float minimumVoltageCalibration = 0f,
            float maximumVoltageCalibration = 3.3f)
        {
            samplingSetting = new AnalogSamplingSetting();
            AnalogInputPort = analogPort;
            MinimumVoltageCalibration = minimumVoltageCalibration;
            MaximumVoltageCalibration = maximumVoltageCalibration;
            /*
            // wire up our observable
            // have to convert from voltage to temp units for our consumers
            // this is where the magic is: this allows us to extend the IObservable
            // pattern through the sensor driver
            AnalogInputPort.Subscribe(
                new FilterableChangeObserver<FloatChangeResult, float>(
                    h => {
                        var newMoisture = VoltageToMoisture(h.New);
                        var oldMoisture = VoltageToMoisture(h.Old);
                        Moisture = newMoisture; // save state
                        RaiseChangedAndNotify(new FloatChangeResult(
                            newMoisture,
                            oldMoisture));
                    })
                );*/
        }
        #endregion

        #region Methods

        /// <summary>
        /// Convenience method to get the current soil moisture. For frequent reads, use
        /// StartUpdating() and StopUpdating().
        /// </summary>
        /// <param name="sampleCount">The number of sample readings to take. 
        /// Must be greater than 0.</param>
        /// <param name="sampleInterval">The interval, in milliseconds, between
        /// sample readings.</param>
        /// <returns></returns>
        public float Read(int sampleCount = 10, int sampleInterval = 40)
        {
            float voltage = 0;
            // read the voltage
            for (int i = 0; i < sampleCount; i++)
            {
                var tmp = AnalogInputPort.ReadValue();
                voltage += tmp;
                Thread.Sleep(sampleInterval);
            }
            voltage /= sampleCount;
            // convert and save to our temp property for later retrieval
            Moisture = VoltageToMoisture(voltage);
            // return
            return Moisture;
        }

        /// <summary>
        /// Starts continuously sampling the sensor.
        ///
        /// This method also starts raising `Updated` events and IObservable
        /// subscribers getting notified. Use the `standbyDuration` parameter
        /// to specify how often events and notifications are raised/sent.
        /// </summary>
        /// <param name="sampleCount">How many samples to take during a given
        /// reading. These are automatically averaged to reduce noise.</param>
        /// <param name="sampleIntervalDuration">The time, in milliseconds,
        /// to wait in between samples during a reading.</param>
        /// <param name="standbyDuration">The time, in milliseconds, to wait
        /// between sets of sample readings. This value determines how often
        /// `Updated` events are raised and `IObservable` consumers are notified.</param>
        public void StartUpdating(
            int sampleCount = 10,
            int sampleIntervalDuration = 40,
            int standbyDuration = 1000)
        {
            samplingSetting.sampleCount = sampleCount;
            samplingSetting.sampleIntervalDuration = sampleIntervalDuration;
            samplingSetting.standbyDuration = standbyDuration;
            IsSampling = true;
            var task = new Thread(new ThreadStart(StartSampling));
            task.Start();
            //AnalogInputPort.StartSampling(sampleCount, sampleIntervalDuration, standbyDuration);
        }
        void StartSampling()
        {
            int sampleCount = samplingSetting.sampleCount;
            int sampleIntervalDuration = samplingSetting.sampleIntervalDuration;
            int standbyDuration = samplingSetting.standbyDuration;
            var counter = 0;
            var total = 0f;
            var average = 0f;
            for (int i = 0; i < sampleCount; i++)
            {
                // read the voltage
                float temp = AnalogInputPort.ReadValue();
               
                total += temp;
                counter++;
                average = total / counter;
                if (!IsSampling) break;
                Thread.Sleep(sampleIntervalDuration);
            }
            IsSampling = false;
            RaiseChangedAndNotify(new FloatChangeResult(average, 0));
        }
        /// <summary>
        /// Stops sampling the sensor.
        /// </summary>
        public void StopUpdating()
        {
            IsSampling = false;
            //AnalogInputPort.StopSampling();
        }

        protected void RaiseChangedAndNotify(FloatChangeResult changeResult)
        {
            Updated?.Invoke(this, changeResult);
            //base.NotifyObservers(changeResult);
        }

        protected float VoltageToMoisture(float voltage)
        {
            if (MinimumVoltageCalibration > MaximumVoltageCalibration) {
                return 1f - Map(voltage, MaximumVoltageCalibration, MinimumVoltageCalibration, 0f, 1.0f);
            }

            return 1f - Map(voltage, MinimumVoltageCalibration, MaximumVoltageCalibration, 0f, 1.0f);
        }

        /// <summary>
        /// Re-maps a value from one range (fromLow - fromHigh) to another (toLow - toHigh).
        /// </summary>
        /// <param name="value"></param>
        /// <param name="fromLow"></param>
        /// <param name="fromHigh"></param>
        /// <param name="toLow"></param>
        /// <param name="toHigh"></param>
        /// <returns></returns>
        protected float Map(float value, float fromLow, float fromHigh, float toLow, float toHigh)
        {
            return (((toHigh - toLow) * (value - fromLow)) / (fromHigh - fromLow)) - toLow;
        }

        #endregion
    }
}