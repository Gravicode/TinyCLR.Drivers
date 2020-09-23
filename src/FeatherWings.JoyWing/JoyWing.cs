using GHIElectronics.TinyCLR.Devices.Gpio;
using Meadow.TinyCLR.Interface;
using Meadow.TinyCLR.System;

namespace Meadow.Foundation.FeatherWings
{
    /// <summary>
    /// This driver is not complete
    /// </summary>
    public class JoyWing
    {
        public event EventHandler OnA;
        public event EventHandler OnB;
        public event EventHandler OnX;
        public event EventHandler OnY;
        public event EventHandler OnSelect;

        public IButton ButtonX { get; private set; }
        public IButton ButtonY { get; private set; }
        public IButton ButtonA { get; private set; }
        public IButton ButtonB { get; private set; }
        public IButton ButtonSelect { get; private set; }

        public JoyWing(int pinX, int pinY, int pinA, int pinB, int pinSelect,
            int pinJoyHorizontal, int pinJoyVertical) 
            
        {
            /*
             this(device.CreateDigitalInputPort(pinX, InterruptMode.EdgeRising),
                device.CreateDigitalInputPort(pinY, InterruptMode.EdgeRising),
                device.CreateDigitalInputPort(pinA, InterruptMode.EdgeRising),
                device.CreateDigitalInputPort(pinB, InterruptMode.EdgeRising),
                device.CreateDigitalInputPort(pinSelect, InterruptMode.EdgeRising),
                device.CreateDigitalInputPort(pinJoyHorizontal, InterruptMode.EdgeRising),
                device.CreateDigitalInputPort(pinJoyVertical, InterruptMode.EdgeRising))
             */
            var gpio = GpioController.GetDefault();

            var DpinX = gpio.OpenPin(pinX);
            DpinX.SetDriveMode(GpioPinDriveMode.InputPullUp);

            var DpinY = gpio.OpenPin(pinY);
            DpinY.SetDriveMode(GpioPinDriveMode.InputPullUp);

            var DpinA = gpio.OpenPin(pinA);
            DpinA.SetDriveMode(GpioPinDriveMode.InputPullUp);

            var DpinB = gpio.OpenPin(pinB);
            DpinB.SetDriveMode(GpioPinDriveMode.InputPullUp);

            var DpinSelect = gpio.OpenPin(pinSelect);
            DpinSelect.SetDriveMode(GpioPinDriveMode.InputPullUp);

            var DpinJoyHorizontal = gpio.OpenPin(pinJoyHorizontal);
            DpinJoyHorizontal.SetDriveMode(GpioPinDriveMode.InputPullUp);

            var DpinJoyVertical = gpio.OpenPin(pinJoyVertical);
            DpinJoyVertical.SetDriveMode(GpioPinDriveMode.InputPullUp);

            Setup(DpinX, DpinY, DpinA, DpinB, DpinSelect,
            DpinJoyHorizontal, DpinJoyVertical);
        }

        public JoyWing(GpioPin portX, GpioPin portY, GpioPin portA, GpioPin portB,
            GpioPin portSelect, GpioPin portJoyHorizontal, GpioPin portJoyVertical)
        {
            /*  ButtonA = new PushButton(portA);
              ButtonB = new PushButton(portB);
              ButtonX = new PushButton(portX);
              ButtonY = new PushButton(portY);
              ButtonSelect = new PushButton(portSelect);*/

            Setup( portX,  portY,  portA,  portB,
              portSelect,  portJoyHorizontal,  portJoyVertical);
        }

        void Setup(GpioPin portX, GpioPin portY, GpioPin portA, GpioPin portB,
            GpioPin portSelect, GpioPin portJoyHorizontal, GpioPin portJoyVertical)
        {
            ButtonA.PressEnded += (s, e) => OnA?.Invoke(s, e);
            ButtonB.PressEnded += (s, e) => OnB?.Invoke(s, e);
            ButtonX.PressEnded += (s, e) => OnX?.Invoke(s, e);
            ButtonY.PressEnded += (s, e) => OnY?.Invoke(s, e);
            ButtonSelect.PressEnded += (s, e) => OnSelect?.Invoke(s, e);
        }
    }
}