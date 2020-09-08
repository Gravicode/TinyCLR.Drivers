using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace Meadow.TinyCLR.Helpers.Measurements
{
    public class Weight : WeightMeasurement
    {
        private float _weightInGrams;

        public WeightUnits StandardUnits { get => WeightUnits.Grams; }

        public Weight(float value, WeightUnits units)
        {
            StandardValue = Convert(value, units, WeightUnits.Grams);
        }

        public override float ConvertTo(WeightUnits units)
        {
            return Convert(StandardValue, WeightUnits.Grams, units);
        }

        public static float Convert(float val, WeightUnits fromUnits, WeightUnits toUnits)
        {
            var factor = 1.0f;

            switch (fromUnits)
            {
                case WeightUnits.Carats:
                    switch (toUnits)
                    {
                        case WeightUnits.Grains: factor = 3.08647167f; break;
                        case WeightUnits.Grams: factor = 0.20f; break;
                        case WeightUnits.Kilograms: factor = 0.0002f; break;
                        case WeightUnits.Ounces: factor = 0.00705479f; break;
                        case WeightUnits.Pounds: factor = 0.00044092f; break;
                    }
                    break;
                case WeightUnits.Grains:
                    switch (toUnits)
                    {
                        case WeightUnits.Carats: factor = 0.32399455f; break;
                        case WeightUnits.Grams: factor = 0.06479891f; break;
                        case WeightUnits.Kilograms: factor = 0.0000648f; break;
                        case WeightUnits.Ounces: factor = 0.00228571f; break;
                        case WeightUnits.Pounds: factor = 0.00014286f; break;
                    }
                    break;
                case WeightUnits.Grams:
                    switch (toUnits)
                    {
                        case WeightUnits.Carats: factor = 5.0f; break;
                        case WeightUnits.Grains: factor = 15.4323584f; break;
                        case WeightUnits.Kilograms: factor = 0.001f; break;
                        case WeightUnits.Ounces: factor = 0.03527396f; break;
                        case WeightUnits.Pounds: factor = 0.00220462f; break;
                    }
                    break;
                case WeightUnits.Kilograms:
                    switch (toUnits)
                    {
                        case WeightUnits.Carats: factor = 5000f; break;
                        case WeightUnits.Grains: factor = 15432.3584f; break;
                        case WeightUnits.Grams: factor = 1000f; break;
                        case WeightUnits.Ounces: factor = 35.27396f; break;
                        case WeightUnits.Pounds: factor = 2.20462f; break;
                    }
                    break;
                case WeightUnits.Ounces:
                    switch (toUnits)
                    {
                        case WeightUnits.Carats: factor = 141.747615f; break;
                        case WeightUnits.Grains: factor = 437.5f; break;
                        case WeightUnits.Grams: factor = 28.3495231f; break;
                        case WeightUnits.Kilograms: factor = 0.2834952f; break;
                        case WeightUnits.Pounds: factor = 0.0625f; break;
                    }
                    break;
                case WeightUnits.Pounds:
                    switch (toUnits)
                    {
                        case WeightUnits.Carats: factor = 2267.96185f; break;
                        case WeightUnits.Grains: factor = 6999.99999f; break;
                        case WeightUnits.Grams: factor = 453.59237f; break;
                        case WeightUnits.Kilograms: factor = 0.45359237f; break;
                        case WeightUnits.Ounces: factor = 16f; break;
                    }
                    break;
            }

            return val * factor;
        }
    }
}
