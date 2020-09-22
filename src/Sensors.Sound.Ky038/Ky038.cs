using GHIElectronics.TinyCLR.Devices.Adc;
using GHIElectronics.TinyCLR.Devices.Gpio;
using System;
using System.Diagnostics;
using System.Threading;

namespace Meadow.TinyCLR.Sensors.Sound
{
    //WIP
   public class Ky038
    {
        protected AdcChannel analogPort;
        protected GpioPin digitalInputPort;

        public Ky038(string AnalogControllerName, int A0, int D0) 
            
        {
            var adc = AdcController.FromName(AnalogControllerName);
            var analog = adc.OpenChannel(A0);
            var gpio = GpioController.GetDefault();
            var d1 = gpio.OpenPin(D0);
            d1.SetDriveMode(GpioPinDriveMode.Input);
            Setup(analog, d1);
            //this (device.CreateAnalogInputPort(A0), device.CreateDigitalInputPort(D0))
        }

        public Ky038(AdcChannel analogPort, GpioPin digitalInputPort)
        {
            Setup( analogPort,  digitalInputPort);
        }
        void Setup(AdcChannel analogPort, GpioPin digitalInputPort)
        {
            this.analogPort = analogPort;
            this.digitalInputPort = digitalInputPort;

            digitalInputPort.ValueChanged += DigitalInputPort_ValueChanged;

            //analogPort.StartSampling();

            while (true)
            {
                Debug.WriteLine($"Analog: {analogPort.ReadValue()}");
                Thread.Sleep(250);
            }
        }

        private void DigitalInputPort_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {

        }

        //private void DigitalInputPort_Changed(object sender, DigitalInputPortEventArgs e)
        //{
           
        //}
    }
}