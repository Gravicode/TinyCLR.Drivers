﻿
using GHIElectronics.TinyCLR.Devices.Adc;
using Meadow.Peripherals.Sensors.Motion;
using Meadow.TinyCLR.Modules.Spatial;
using System;
using System.Threading;

namespace Meadow.TinyCLR.Sensors.Motion
{
    /// <summary>
    ///     Driver for the ADXL335 triple axis accelerometer.
    ///     +/- 3g
    /// </summary>
    public class Adxl335 : IAccelerometer
    {
        #region Constants

        /// <summary>
        ///     Minimum value that can be used for the update interval when the
        ///     sensor is being configured to generate interrupts.
        /// </summary>
        public const ushort MinimumPollingPeriod = 100;

        #endregion Constants

        #region Member variables / fields

        /// <summary>
        ///     Analog input channel connected to the x axis.
        /// </summary>
        private readonly AdcChannel _xPort;

        /// <summary>
        ///     Analog input channel connected to the x axis.
        /// </summary>
        private readonly AdcChannel _yPort;

        /// <summary>
        ///     Analog input channel connected to the x axis.
        /// </summary>
        private readonly AdcChannel _zPort;

        /// <summary>
        ///     Voltage that represents 0g.  This is the supply voltage / 2.
        /// </summary>
        private float _zeroGVoltage => SupplyVoltage / 2f;

        #endregion Member variables / fields

        #region Properties

        /// <summary>
        ///     Acceleration along the X-axis.
        /// </summary>
        /// <remarks>
        ///     This property will only contain valid data after a call to Read or after
        ///     an interrupt has been generated.
        /// </remarks>
        public float XAcceleration => Conditions.XAcceleration;

        /// <summary>
        ///     Acceleration along the Y-axis.
        /// </summary>
        /// <remarks>
        ///     This property will only contain valid data after a call to Read or after
        ///     an interrupt has been generated.
        /// </remarks>
        public float YAcceleration => Conditions.YAcceleration;

        /// <summary>
        ///     Acceleration along the Z-axis.
        /// </summary>
        /// <remarks>
        ///     This property will only contain valid data after a call to Read or after
        ///     an interrupt has been generated.
        /// </remarks>
        public float ZAcceleration => Conditions.ZAcceleration;

        /// <summary>
        ///     Volts per G for the X axis.
        /// </summary>
        public float XVoltsPerG { get; set; }

        /// <summary>
        ///     Volts per G for the X axis.
        /// </summary>
        public float YVoltsPerG { get; set; }

        /// <summary>
        ///     Volts per G for the X axis.
        /// </summary>
        public float ZVoltsPerG { get; set; }

        /// <summary>
        ///     Power supply voltage applied to the sensor.  This will be set (in the constructor)
        ///     to 3.3V by default.
        /// </summary>
        public float SupplyVoltage { get; set; }

        public AccelerationConditions Conditions { get; protected set; } = new AccelerationConditions();

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

        public event AccelerationConditionChangeResultHandler Updated;

        #endregion Events and delegates

        #region Constructors

        /// <summary>
        ///     Create a new ADXL335 sensor object.
        /// </summary>
        /// <param name="xPin">Analog pin connected to the X axis output from the ADXL335 sensor.</param>
        /// <param name="yPin">Analog pin connected to the Y axis output from the ADXL335 sensor.</param>
        /// <param name="zPin">Analog pin connected to the Z axis output from the ADXL335 sensor.</param>
        public Adxl335(string AnalogControllerName, int xPin, int yPin, int zPin)
        {
            var adc = AdcController.FromName(AnalogControllerName);
            _xPort = adc.OpenChannel(xPin);
            _yPort = adc.OpenChannel(yPin);
            _zPort = adc.OpenChannel(zPin);
           
            //
            //  Now set the default calibration data.
            //
            XVoltsPerG = 0.325f;
            YVoltsPerG = 0.325f;
            ZVoltsPerG = 0.550f;
            SupplyVoltage = 3.3f;
        }

        #endregion Constructors

        #region Methods

        ///// <summary>
        ///// Convenience method to get the current temperature. For frequent reads, use
        ///// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        ///// </summary>
        public AccelerationConditions Read()
        {
            Update();

            return Conditions;
        }

        ///// <summary>
        ///// Starts continuously sampling the sensor.
        /////
        ///// This method also starts raising `Changed` events and IObservable
        ///// subscribers getting notified.
        ///// </summary>
        public void StartUpdating(int standbyDuration = 1000)
        {
            // thread safety
            lock (_lock) {
                if (IsSampling) { return; }

                // state muh-cheen
                IsSampling = true;

                //SamplingTokenSource = new CancellationTokenSource();
                //CancellationToken ct = SamplingTokenSource.Token;

                AccelerationConditions oldConditions;
                AccelerationConditionChangeResult result;
                var task1 = new Thread(new ThreadStart ( () => {
                    while (true) {
                        if (!IsSampling) {
                            // do task clean up here
                            //_observers.ForEach(x => x.OnCompleted());
                            break;
                        }
                        // capture history
                        oldConditions = AccelerationConditions.From(Conditions);

                        // read
                        Update();

                        // build a new result with the old and new conditions
                        result = new AccelerationConditionChangeResult(oldConditions, Conditions);

                        // let everyone know
                        RaiseChangedAndNotify(result);

                        // sleep for the appropriate interval
                        Thread.Sleep(standbyDuration);
                    }
                }));
                task1.Start();
            }
        }

        protected void RaiseChangedAndNotify(AccelerationConditionChangeResult changeResult)
        {
            Updated?.Invoke(this, changeResult);
            //base.NotifyObservers(changeResult);
        }

        ///// <summary>
        ///// Stops sampling the temperature.
        ///// </summary>
        public void StopUpdating()
        {
            lock (_lock) {
                //if (!IsSampling) { return; }

                //SamplingTokenSource?.Cancel();

                // state muh-cheen
                IsSampling = false;
            }
        }

        /// <summary>
        ///     Read the sensor output and convert the sensor readings into acceleration values.
        /// </summary>
        public void Update()
        {
            Conditions.XAcceleration = (_xPort.ReadValue() - _zeroGVoltage) / XVoltsPerG;
            Conditions.YAcceleration = (_yPort.ReadValue() - _zeroGVoltage) / YVoltsPerG;
            Conditions.ZAcceleration = (_zPort.ReadValue() - _zeroGVoltage) / ZVoltsPerG;
        }

        /// <summary>
        ///     Get the raw analog input values from the sensor.
        /// </summary>
        /// <returns>Vector object containing the raw sensor data from the analog pins.</returns>
        public Vector GetRawSensorData()
        {
            return new Vector( _xPort.ReadValue(),  _yPort.ReadValue(),  _zPort.ReadValue());
        }

        #endregion Methods
    }
}