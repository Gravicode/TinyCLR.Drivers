using GHIElectronics.TinyCLR.Devices.Adc;
using GHIElectronics.TinyCLR.Devices.Gpio;
using Meadow.Peripherals.Sensors.Moisture;
using System;
using System.Threading;

namespace Meadow.Foundation.Sensors.Moisture
{
    /// <summary>
    /// FC-28-D Soil Hygrometer Detection Module + Soil Moisture Sensor    
    /// </summary>
    public class Fc28 : IMoistureSensor
    {
        /// <summary>
        /// Raised when a new sensor reading has been made. To enable, call StartUpdating().
        /// </summary>
        public event FloatChangeResultHandler Updated = delegate { };

        // internal thread lock
        private object _lock = new object();
        //private CancellationTokenSource SamplingTokenSource;

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the sensor is currently in a sampling
        /// loop. Call StartSampling() to spin up the sampling process.
        /// </summary>
        /// <value><c>true</c> if sampling; otherwise, <c>false</c>.</value>
        public bool IsSampling { get; protected set; } = false;

        /// <summary>
        /// Returns the analog input port
        /// </summary>
        public AdcChannel AnalogInputPort { get; protected set; }

        /// <summary>
        /// Returns the digital output port
        /// </summary>
        public GpioPin DigitalPort { get; protected set; }

        /// <summary>
        /// Last value read from the moisture sensor.
        /// </summary>
        public float Moisture { get; private set; }

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
        /// Creates a FC28 soil moisture sensor object with the especified analog pin, digital pin and IO device.
        /// </summary>
        /// <param name="analogPort"></param>
        /// <param name="digitalPort"></param>
        public Fc28(
            string AnalogControllerName,
            int analogPin,
            int digitalPin,
            float minimumVoltageCalibration = 0f,
            float maximumVoltageCalibration = 3.3f)

        {
            var adc = AdcController.FromName(AnalogControllerName);
            var analog = adc.OpenChannel(analogPin);
            var gpio = GpioController.GetDefault();
            var dPin = gpio.OpenPin(digitalPin);
            dPin.SetDriveMode(GpioPinDriveMode.Output);
          
            //this(device.CreateAnalogInputPort(analogPin), device.CreateDigitalOutputPort(digitalPin), minimumVoltageCalibration, maximumVoltageCalibration)
            Setup(analog,dPin, minimumVoltageCalibration, maximumVoltageCalibration);
        }

        /// <summary>
        /// Creates a FC28 soil moisture sensor object with the especified analog pin and digital pin.
        /// </summary>
        /// <param name="analogPort"></param>
        /// <param name="digitalPort"></param>
        public Fc28(
            AdcChannel analogPort,
            GpioPin digitalPort,
            float minimumVoltageCalibration = 0f,
            float maximumVoltageCalibration = 3.3f)
        {
            Setup( analogPort,
             digitalPort,
             minimumVoltageCalibration ,
             maximumVoltageCalibration );
        }
        void Setup(AdcChannel analogPort,
            GpioPin digitalPort,
            float minimumVoltageCalibration = 0f,
            float maximumVoltageCalibration = 3.3f) {
            AnalogInputPort = analogPort;
            DigitalPort = digitalPort;
            MinimumVoltageCalibration = minimumVoltageCalibration;
            MaximumVoltageCalibration = maximumVoltageCalibration;
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
            DigitalPort.Write(GpioPinValue.High);
            //float voltage = await AnalogInputPort.Read(sampleCount, sampleInterval);
            float voltage = 0;
            // read the voltage
            for (int i = 0; i < sampleCount; i++)
            {
                var tmp = AnalogInputPort.ReadValue();
                voltage += tmp;
                Thread.Sleep(sampleInterval);
            }
            voltage /= sampleCount;
            DigitalPort.Write(GpioPinValue.Low);

            // convert and save to our temp property for later retrieval
            Moisture = VoltageToMoisture(voltage);
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
            // thread safety
            lock (_lock) {
                if (IsSampling)
                    return;
                IsSampling = true;

                //SamplingTokenSource = new CancellationTokenSource();
                //CancellationToken ct = SamplingTokenSource.Token;

                float oldConditions;
                FloatChangeResult result;
               var task = new Thread(new ThreadStart( () => {
                    while (true) {
                        // TODO: someone please review; is this the correct
                        // place to do this?
                        // check for cancel (doing this here instead of 
                        // while(!ct.IsCancellationRequested), so we can perform 
                        // cleanup
                        if (!IsSampling) {
                            // do task clean up here
                            //_observers.ForEach(x => x.OnCompleted());
                            break;
                        }
                        // capture history
                        oldConditions = Moisture;

                        // read                        
                        Moisture = Read(sampleCount, sampleIntervalDuration);

                        // build a new result with the old and new conditions
                        result = new FloatChangeResult(oldConditions, Moisture);

                        // let everyone know
                        RaiseChangedAndNotify(result);

                        // sleep for the appropriate interval
                        Thread.Sleep(standbyDuration);
                    }
                }));
                task.Start();
            }
        }

        /// <summary>
        /// Stops sampling the sensor.
        /// </summary>
        public void StopUpdating()
        {
            lock (_lock) {
                /*
                if (!IsSampling) return;

                if (SamplingTokenSource != null) {
                    SamplingTokenSource.Cancel();
                }
                */
                IsSampling = false;
            }
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