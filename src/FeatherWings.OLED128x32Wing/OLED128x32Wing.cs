using GHIElectronics.TinyCLR.Devices.Gpio;
using Meadow.TinyCLR.Displays;
using Meadow.TinyCLR.Interface;
using Meadow.TinyCLR.Modules.Buttons;

namespace Meadow.TinyCLR.FeatherWings
{
    /// <summary>
    /// Represents Adafruits OLED Feather Wing
    /// </summary>
    public class OLED128x32Wing
    {
        string i2cBus;

        public Ssd1306 Display { get; protected set; }

        public IButton ButtonA { get; protected set; }

        public IButton ButtonB { get; protected set; }

        public IButton ButtonC { get; protected set; }

        public OLED128x32Wing(string i2cBus,  int pinA, int pinB, int pinC) {
            /*
             : 
            this(i2cBus, 
                device.CreateDigitalInputPort(pinA, InterruptMode.EdgeBoth, ResistorMode.PullUp),
                device.CreateDigitalInputPort(pinB, InterruptMode.EdgeBoth, ResistorMode.PullUp),
                device.CreateDigitalInputPort(pinC, InterruptMode.EdgeBoth, ResistorMode.PullUp))
             */
            var gpio = GpioController.GetDefault();

            var DpinA = gpio.OpenPin(pinA);
            DpinA.SetDriveMode(GpioPinDriveMode.InputPullUp);

            var DpinB = gpio.OpenPin(pinB);
            DpinB.SetDriveMode(GpioPinDriveMode.InputPullUp);

            var DpinC = gpio.OpenPin(pinC);
            DpinC.SetDriveMode(GpioPinDriveMode.InputPullUp);

            Setup( i2cBus, DpinA, DpinB, DpinC);
        }

        public OLED128x32Wing(string i2cBus, GpioPin portA, GpioPin portB, GpioPin portC)
        {
            Setup( i2cBus,  portA,  portB,  portC);
        }

        void Setup(string i2cBus, GpioPin portA, GpioPin portB, GpioPin portC) {
            this.i2cBus = i2cBus;
            Display = new Ssd1306(this.i2cBus, 0x3C, Ssd1306.DisplayType.OLED128x32);

            //Bug? Resistor Mode is being set properly from the above constructor but unless it is set again it doesn't work.
            //portA.Resistor = portA.Resistor;
            //portC.Resistor = portC.Resistor;

            ButtonA = new PushButton(portA);
            ButtonB = new PushButton(portB);
            ButtonC = new PushButton(portC);
        }
    }
}