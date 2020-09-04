using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace TinyCLR.Drivers.Helpers.Measurements
{
    public enum WeightUnits
    {
        Grams,
        Kilograms,
        Pounds,
        Ounces,
        Grains,
        Carats
    }
    public abstract class WeightMeasurement
    {
        public float StandardValue { get; protected set; }

        public abstract float ConvertTo(WeightUnits units);
    }
}
