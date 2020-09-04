using System;
using System.Collections;
using System.Text;
using System.Threading;
using TinyCLR.Drivers.System;

namespace TinyCLR.Drivers.Interface
{
    public class AtmosphericConditions
    {
        
        public AtmosphericConditions(float temperature, float pressure, float humidity)
        {
            this.Temperature = temperature;
            this.Pressure = pressure;
            this.Humidity = humidity;
        }

        public float Temperature { get; set; }
        public float Pressure { get; set; }
        public float Humidity { get; set; }

      
    }
    public interface ITemperatureSensor
    {
        float Temperature { get; }

        event TempChangedHandler Updated;
    }

    public delegate void TempChangedHandler(AtmosphericConditions e);
}
