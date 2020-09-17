using System;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Adc;
using Meadow.Peripherals.Sensors.Distance;

namespace Sensors.Distance
{
    public class Gp2D12 : IRangeFinder
    {
        #region Member variables / fields

        AdcChannel analogInputPort;

        float currentValue;

        public event DistanceEventHandler DistanceDetected;

        public float CurrentDistance { get; private set; } = -1;

        public float MinimumDistance => 0.098f;

        public float MaximumDistance => 0.79f;

        #endregion Member variables / fields
        
        #region Constructors

        public Gp2D12(string AnalogControllerName, int analogInputPin)
        {
            var adc = AdcController.FromName(AnalogControllerName);
            analogInputPort = adc.OpenChannel(analogInputPin);
            //analogInputPort = device.CreateAnalogInputPort(analogInputPin);
            Thread task1 = new Thread(new ThreadStart (ReadLoop));
            task1.Start();
        }


        void ReadLoop()
        {
            while (true)
            {
                var newVal = analogInputPort.ReadValue();
                if (currentValue != newVal)
                {
                    currentValue = newVal;
                    AnalogValueChanged(newVal);
                }
                Thread.Sleep(200);
            }
        }
        private void AnalogValueChanged(float newValue)
        {
            CurrentDistance = 26 / newValue;
            DistanceDetected?.Invoke(this, new DistanceEventArgs(CurrentDistance));
        }

        #endregion

        #region Methods

        public float ReadDistance()
        {
            var value = analogInputPort.ReadValue();

            CurrentDistance = 26 / value;

            CurrentDistance = (float)Math.Max(CurrentDistance, MinimumDistance);
            CurrentDistance = (float)Math.Min(CurrentDistance, MaximumDistance);

            return CurrentDistance;
        }

        #endregion Methods
    }
}