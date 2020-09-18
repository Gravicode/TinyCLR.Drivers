
using GHIElectronics.TinyCLR.Devices.Adc;

namespace Meadow.TinyCLR.Sensors.Light
{
    public class Alspt19315C
    {
        #region Member variables / fields

        /// <summary>
        ///     Analog port connected to the sensor.
        /// </summary>
        private readonly AdcChannel sensor;

        /// <summary>
        ///     Voltage being output by the sensor.
        /// </summary>
        public float GetVoltage()
        {
            return sensor.ReadValue();
        }

        #endregion Member variables / fields

        #region Constructors

        /// <summary>
        ///     Create a new light sensor object using a static reference voltage.
        /// </summary>
        /// <param name="pin">AnalogChannel connected to the sensor.</param>
        public Alspt19315C( string AnalogControllerName, int pin)
        {
            var adc = AdcController.FromName(AnalogControllerName);
            sensor= adc.OpenChannel(pin);

            //sensor = device.CreateAnalogInputPort(pin);
        }

        #endregion Constructors
    }
}