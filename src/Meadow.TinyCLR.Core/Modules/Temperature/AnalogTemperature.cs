﻿using GHIElectronics.TinyCLR.Devices.Adc;
using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Meadow.TinyCLR.Core;
using Meadow.TinyCLR.Interface;
using Meadow.Peripherals.Sensors.Temperature;
using Meadow.Peripherals.Sensors.Atmospheric;

namespace Meadow.TinyCLR.Modules.Temperature
{
    /// <summary>
    /// Provide the ability to read the temperature from the following sensors:
    /// - TMP35 / 36 / 37
    /// - LM35 / 45
    /// </summary>
    /// <remarks>
    /// <i>AnalogTemperature</i> provides a method of reading the temperature from
    /// linear analog temperature sensors.  There are a number of these sensors available
    /// including the commonly available TMP and LM series.
    /// Sensors of this type obey the following equation:
    /// y = mx + c
    /// where y is the reading in millivolts, m is the gradient (number of millivolts per
    /// degree centigrade and C is the point where the line would intercept the y axis.
    /// The <i>SensorType</i> enum defines the list of sensors with default settings in the
    /// library.  Unsupported sensors that use the same linear algorithm can be constructed
    /// by setting the sensor type to <i>SensorType.Custom</i> and providing the settings for
    /// the linear calculations.
    /// The default sensors have the following settings:
    /// Sensor              Millivolts at 25C    Millivolts per degree C
    /// TMP35, LM35, LM45       250                     10
    /// TMP36, LM50             750                     10
    /// TMP37                   500                     20
    /// </remarks>
    public class AnalogTemperature : ITemperatureSensor

    {
        AnalogSamplingSetting samplingSetting;
        public bool IsSampling { get; set; }
        /// <summary>
        /// Raised when the value of the reading changes.
        /// </summary>
        //public event TempChangedHandler Updated = delegate { };
        public event AtmosphericConditionChangeEventHandler Updated = delegate { };
        #region Local classes

        /// <summary>
        ///     Calibration class for new sensor types.  This allows new sensors
        ///     to be used with this class.
        /// </summary>
        /// <remarks>
        ///     The default settings for this object are correct for the TMP35.
        /// </remarks>
        public class Calibration
        {
            /// <summary>
            ///     Sample reading as specified in the product data sheet.
            ///     Measured in degrees Centigrade.
            /// </summary>
            public int SampleReading { get; protected set; } = 25;

            /// <summary>
            ///     Millivolt reading the sensor will generate when the sensor
            ///     is at the Samplereading temperature.  This value can be
            ///     obtained from the data sheet. 
            /// </summary>
            public int MillivoltsAtSampleReading { get; protected set; } = 250;

            /// <summary>
            ///     Linear change in the sensor output (in millivolts) per 1 degree C
            ///     change in temperature.
            /// </summary>
            public int MillivoltsPerDegreeCentigrade { get; protected set; } = 10;

            /// <summary>
            ///     Default constructor.  Create a new Calibration object with default values
            ///     for the properties.
            /// </summary>
            public Calibration()
            {
            }

            /// <summary>
            ///     Create a new Calibration object using the specified values.
            /// </summary>
            /// <param name="sampleReading">Sample reading from the data sheet.</param>
            /// <param name="millivoltsAtSampleReading">Millivolts output at the sample reading (from the data sheet).</param>
            /// <param name="millivoltsPerDegreeCentigrade">Millivolt change per degree centigrade (from the data sheet).</param>
            public Calibration(int sampleReading, int millivoltsAtSampleReading, int millivoltsPerDegreeCentigrade)
            {
                SampleReading = sampleReading;
                MillivoltsAtSampleReading = millivoltsAtSampleReading;
                MillivoltsPerDegreeCentigrade = millivoltsPerDegreeCentigrade;
            }
        }

        #endregion Local classes

        #region Enums

        /// <summary>
        ///     Type of temperature sensor.
        /// </summary>
        public enum KnownSensorType
        {
            Custom,
            TMP35,
            TMP36,
            TMP37,
            LM35,
            LM45,
            LM50
        }

        #endregion Enums

        #region Member variables /fields
       
        public AdcChannel AnalogInputPort { get; protected set; }

        /// <summary>
        ///     Millivolts per degree centigrade for the sensor attached to the analog port.
        /// </summary>
        /// <remarks>
        ///     This will be the gradient of the line y = mx + c.
        /// </remarks>
        private int millivoltsPerDegreeCentigrade;

        /// <summary>
        ///     Point where the line y = mx +c would intercept the y-axis.
        /// </summary>
        private float yIntercept;

        #endregion Member variables / fields

        #region Properties

        /// <summary>
        ///     Analog port that the temperature sensor is attached to.
        /// </summary>
        /// <value>Analog port connected to the temperature sensor.</value>
        private AdcChannel AnalogPort { get; set; }

        /// <summary>
        ///     Temperature in degrees centigrade.
        /// </summary>
        /// <remarks>
        ///     The temperature is given by the following calculation:
        ///     temperature = (reading in millivolts - yIntercept) / millivolts per degree centigrade
        /// </remarks>
        public float Temperature { get; protected set; }

        float ITemperatureSensor.Temperature => throw new NotImplementedException();

        #endregion Properties


        #region Constructor(s)

        /// <summary>
        ///     Default constructor, private to prevent this being used.
        /// </summary>
        private AnalogTemperature()
        {
        }

        /// <summary>
        ///     New instance of the AnalogTemperatureSensor class.
        /// </summary>
        /// <param name="analogPin">Analog pin the temperature sensor is connected to.</param>
        /// <param name="sensorType">Type of sensor attached to the analog port.</param>
        /// <param name="calibration">Calibration for the analog temperature sensor. Only used if sensorType is set to Custom.</param>
        public AnalogTemperature(
            string AdcControllerName,
            int analogPin,
            KnownSensorType sensorType,
            Calibration calibration = null
            )
        {
            samplingSetting = new AnalogSamplingSetting();
            // : this(device.CreateAnalogInputPort(analogPin), sensorType, calibration)
            var adc = AdcController.FromName(AdcControllerName);
            var analog = adc.OpenChannel(analogPin);
            SetAnalogPort(analog, sensorType, calibration);
        }

        void SetAnalogPort(AdcChannel analogInputPort,
                                 KnownSensorType sensorType,
                                 Calibration calibration = null)
        {
            AnalogInputPort = analogInputPort;

            //
            //  If the calibration object is null use the defaults for TMP35.
            //
            if (calibration == null) { calibration = new Calibration(); }

            switch (sensorType)
            {
                case KnownSensorType.TMP35:
                case KnownSensorType.LM35:
                case KnownSensorType.LM45:
                    yIntercept = 0;
                    millivoltsPerDegreeCentigrade = 10;
                    break;
                case KnownSensorType.LM50:
                case KnownSensorType.TMP36:
                    yIntercept = 500;
                    millivoltsPerDegreeCentigrade = 10;
                    break;
                case KnownSensorType.TMP37:
                    yIntercept = 0;
                    millivoltsPerDegreeCentigrade = 20;
                    break;
                case KnownSensorType.Custom:
                    yIntercept = calibration.MillivoltsAtSampleReading - (calibration.SampleReading * calibration.MillivoltsPerDegreeCentigrade);
                    millivoltsPerDegreeCentigrade = calibration.MillivoltsPerDegreeCentigrade;
                    break;
                default:
                    throw new ArgumentException("Unknown sensor type", nameof(sensorType));
            }

            // wire up our observable
            // have to convert from voltage to temp units for our consumers
            // this is where the magic is: this allows us to extend the IObservable
            // pattern through the sensor driver
            /*
            AnalogInputPort.Subscribe
            (
                new FilterableChangeObserver<FloatChangeResult, float>(
                    h => {
                        var newTemp = VoltageToTemperature(h.New);
                        var oldTemp = VoltageToTemperature(h.Old);
                        Temperature = newTemp; // save state
                        RaiseEventsAndNotify
                        (
                            new AtmosphericConditionChangeResult(
                                new AtmosphericConditions(newTemp, null, null),
                                new AtmosphericConditions(oldTemp, null, null)
                            )
                        );
                    }
                )
           );*/
        }
        public AnalogTemperature(AdcChannel analogInputPort,
                                 KnownSensorType sensorType,
                                 Calibration calibration = null)
        {
            SetAnalogPort(analogInputPort,
                                  sensorType,
                                  calibration);
        }

        

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Convenience method to get the current temperature. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        /// <param name="sampleCount">The number of sample readings to take. 
        /// Must be greater than 0. These samples are automatically averaged.</param>
        /// <param name="sampleIntervalDuration">The time, in milliseconds,
        /// to wait in between samples during a reading.</param>
        /// <returns>A float value that's ann average value of all the samples taken.</returns>
        public AtmosphericConditions Read(int sampleCount = 10, int sampleIntervalDuration = 40)
        {
            // read the voltage
            float voltage = AnalogInputPort.ReadValue();// (sampleCount, sampleIntervalDuration);
            // convert and save to our temp property for later retreival
            Temperature = VoltageToTemperature(voltage);
            // return
            return new AtmosphericConditions(Temperature, 0, 0);
            //return Temperature;
        }

        /// <summary>
        /// Starts continuously sampling the sensor.
        ///
        /// This method also starts raising `Changed` events and IObservable
        /// subscribers getting notified. Use the `readIntervalDuration` parameter
        /// to specify how often events and notifications are raised/sent.
        /// </summary>
        /// <param name="sampleCount">How many samples to take during a given
        /// reading. These are automatically averaged to reduce noise.</param>
        /// <param name="sampleIntervalDuration">The time, in milliseconds,
        /// to wait in between samples during a reading.</param>
        /// <param name="standbyDuration">The time, in milliseconds, to wait
        /// between sets of sample readings. This value determines how often
        /// `Changed` events are raised and `IObservable` consumers are notified.</param>
        public void StartUpdating(
            int sampleCount = 10,
            int sampleIntervalDuration = 40,
            int standbyDuration = 100)
        {
            samplingSetting.sampleCount = sampleCount;
            samplingSetting.sampleIntervalDuration = sampleIntervalDuration;
            samplingSetting.standbyDuration = standbyDuration;
            IsSampling = true;
            var task = new Thread(new ThreadStart(StartSampling));
            task.Start();
            
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
                float voltage = AnalogInputPort.ReadValue();
                // (sampleCount, sampleIntervalDuration);
                // convert and save to our temp property for later retreival
                var temp = VoltageToTemperature(voltage);
                total += temp;
                counter++;
                average = total / counter;
                if (!IsSampling) break;
                Thread.Sleep(sampleIntervalDuration);
            }
            IsSampling = false;
            RaiseEventsAndNotify
            (
                new AtmosphericConditionChangeResult(new AtmosphericConditions(average, 0, 0), null)
            );
        }
        /// <summary>
        /// Stops sampling the temperature.
        /// </summary>
        public void StopUpdating()
        {
            IsSampling = false;
        }

        protected void RaiseEventsAndNotify(AtmosphericConditionChangeResult changeResult)
        {
            Updated.Invoke(this,changeResult);
            //base.NotifyObservers(changeResult);
        }

        /// <summary>
        /// Converts a voltage value to a celsius temp, based on the current
        /// calibration values.
        /// </summary>
        /// <param name="voltage"></param>
        /// <returns></returns>
        protected float VoltageToTemperature(float voltage)
        {
            var yAdjusted = voltage * 1000;
            return (yAdjusted - yIntercept) / millivoltsPerDegreeCentigrade;
        }

        #endregion Methods
    }
    
}
