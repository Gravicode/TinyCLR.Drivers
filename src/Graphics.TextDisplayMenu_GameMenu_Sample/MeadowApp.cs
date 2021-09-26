//using Meadow;
//using Meadow.Devices;
//using Meadow.Foundation.Displays.Ssd130x;
//using Meadow.Foundation.Displays.TextDisplayMenu;
//using Meadow.Foundation.Graphics;
//using Meadow.Foundation.Leds;
//using Meadow.Foundation.Sensors.Buttons;
//using Meadow.Hardware;
//using Meadow.Peripherals.Displays;
//using Meadow.Peripherals.Sensors.Buttons;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Pins;
using Graphics.TextDisplayMenu_GameMenu_Sample.Properties;
using Meadow.Foundation.Displays.TextDisplayMenu;
using Meadow.Foundation.Graphics;
using Meadow.TinyCLR.Core.Interface;
using Meadow.TinyCLR.Displays;
using Meadow.TinyCLR.Interface;
using Meadow.TinyCLR.Modules.Buttons;
using Meadow.TinyCLR.Modules.Led;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
//using System.Threading.Tasks;

namespace MeadowApp
{
    public class MeadowApp
    {
        RgbPwmLed onboardLed;

        Menu menu;

        GraphicsLibrary graphics;
        Ssd1309 ssd1309;

        IButton up = null;
        IButton down = null;
        IButton left = null;
        IButton right = null;

        public MeadowApp()
        {
            Initialize();
        }

        void Initialize()
        {
            Debug.WriteLine("Initialize hardware...");
            var controller = PwmController.FromName(SC20260.Timer.Pwm.Controller3.Id);
            var led1 = controller.OpenChannel(SC20260.Timer.Pwm.Controller3.PB0);
            var led2= controller.OpenChannel(SC20260.Timer.Pwm.Controller3.PB1);
            var led3 = controller.OpenChannel(SC20260.Timer.Pwm.Controller3.PC6);

            onboardLed = new RgbPwmLed(led1,led2,led3, 
                3.3f, 3.3f, 3.3f,
                CommonType.CommonAnode);
            //onboardLed = new RgbPwmLed(device: Device,
            //    redPwmPin: Device.Pins.OnboardLedRed,
            //    greenPwmPin: Device.Pins.OnboardLedGreen,
            //    bluePwmPin: Device.Pins.OnboardLedBlue,
            //    3.3f, 3.3f, 3.3f,
            //    Meadow.Peripherals.Leds.IRgbLed.CommonType.CommonAnode);

            Debug.WriteLine("Create Display with SPI...");
            var cs = GpioController.GetDefault().OpenPin(SC20260.GpioPin.PE4);

            var settings = new SpiConnectionSettings()
            {
                ChipSelectType = SpiChipSelectType.Gpio,
                ChipSelectLine = cs,
                Mode = SpiMode.Mode1,
                ClockFrequency = 4_000_000,
            };

            var controllerSpi = SpiController.FromName(SC20100.SpiBus.Spi4);
            var deviceSpi = controllerSpi.GetDevice(settings);


            ssd1309 = new Ssd1309
            (
                SC20260.SpiBus.Spi2,
                SC20260.GpioPin.PE4,
                SC20260.GpioPin.PC2,
                SC20260.GpioPin.PC3
            );

            Debug.WriteLine("Create GraphicsLibrary...");

            graphics = new GraphicsLibrary(ssd1309)
            {
                CurrentFont = new Font8x12(),
            };

            graphics.Clear();
            graphics.DrawText(0, 0, "Loading Menu");
            graphics.Show();

            CreateMenu(graphics);

            Debug.WriteLine("Create buttons...");

            up = new PushButton(SC20260.GpioPin.PC6,GpioPinDriveMode.InputPullDown);
            up.Clicked += Up_Clicked;

            left = new PushButton(SC20260.GpioPin.PC7, GpioPinDriveMode.InputPullDown);
            left.Clicked += Left_Clicked;

            right = new PushButton(SC20260.GpioPin.PC8, GpioPinDriveMode.InputPullDown);
            right.Clicked += Right_Clicked;

            down = new PushButton(SC20260.GpioPin.PC9, GpioPinDriveMode.InputPullDown);
            down.Clicked += Down_Clicked;

            menu.Enable();
        }

        private void Down_Clicked(object sender, EventArgs e)
        {
            if (menu.IsEnabled) { menu.Next(); }
        }

        private void Right_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Right_Clicked");
            if (menu.IsEnabled)
            {
                menu.Select();
            }
        }

        private void Left_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Left_Clicked");
            if (menu.IsEnabled == false)
            {
                playGame = false;
            }
        }

        private void Up_Clicked(object sender, EventArgs e)
        {
            if (menu.IsEnabled)
            {
                menu.Previous();
            }
        }

        bool playGame = false;
        Thread StartGame(string command)
        {
            Debug.WriteLine($"******** {command}");

            playGame = true;
            EnableMenu();
            this.command = command;
            Thread th1 = new Thread(new ThreadStart(Loop));
            th1.Start();
            return th1;
            
        }
        public string command { get; set; }
        void Loop()
        {
            int count = 0;
            int x = 0, y = 0;
            int xD = 1, yD = 1;

            while (count < 150 && playGame == true)
            {
                graphics.Clear();
                graphics.DrawText(0, 0, $"{command}:");
                graphics.DrawText(0, 20, $"{count++}");
                graphics.DrawPixel(x += xD, y += yD);
                if (x == graphics.Width || x == 0) { xD *= -1; };
                if (y == graphics.Height || y == 0) { yD *= -1; };
                graphics.Show();
            }
        }

        void EnableMenu()
        {
            Debug.WriteLine("Enable menu...");

            menu?.Enable();
        }

        void DisableMenu()
        {
            menu?.Disable();
        }

        void CreateMenu(ITextDisplay display)
        {
            Debug.WriteLine("Load menu data...");

            var menuData = LoadResource("menu.json");

            Debug.WriteLine($"Data length: {menuData.Length}...");

            Debug.WriteLine("Create menu...");

            menu = new Menu(display, menuData, false);

            menu.Selected += Menu_Selected;
        }

        private void Menu_Selected(object sender, MenuSelectedEventArgs e)
        {
            Debug.WriteLine($"******** Selected: {e.Command}");

            DisableMenu();

            var t = StartGame(e.Command);
        }

        byte[] LoadResource(string filename)
        {
            //var assembly = Assembly.GetExecutingAssembly();
            //var resourceName = $"Graphics.TextDisplayMenu_GameMenu_Sample.{filename}";
            var bytes = Resources.GetBytes(Resources.BinaryResources.menu);
            return bytes;
            /*
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }*/
        }
    }
}