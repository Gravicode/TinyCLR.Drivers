using GHIElectronics.TinyCLR.Devices.Gpio;
using System;
using System.Collections;
using System.Text;
using System.Threading;
using TinyCLR.Drivers.Interface;

namespace TinyCLR.Drivers.Modules
{
	/// <summary>
	/// Represents a simple LED
	/// </summary>
	public class Led : ILed
	{

		bool IsAnimating = false;
		/// <summary>
		/// Gets the port that is driving the LED
		/// </summary>
		/// <value>The port</value>
		public GpioPin Port { get; protected set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:Meadow.Foundation.Leds.Led"/> is on.
		/// </summary>
		/// <value><c>true</c> if is on; otherwise, <c>false</c>.</value>
		public bool IsOn
		{
			get { return isOn; }
			set
			{
				isOn = value;
				Port.Write(isOn? GpioPinValue.High : GpioPinValue.Low);
			}
		}
		protected bool isOn;

		/// <summary>
		/// Creates a LED through a pin directly from the Digital IO of the board
		/// </summary>
		/// <param name="pin"></param>
		public Led(int pin) 
		{
			var gpio = GpioController.GetDefault();
			this.Port = gpio.OpenPin(pin);
			this.Port.SetDriveMode(GpioPinDriveMode.Output);
			this.Port.Write(GpioPinValue.Low);
		}

		/// <summary>
		/// Creates a LED through a DigitalOutPutPort from an IO Expander
		/// </summary>
		/// <param name="port"></param>
		public Led(GpioPin port)
		{
			Port = port;
		}

		/// <summary>
		/// Stops the LED when its blinking and/or turns it off.
		/// </summary>
		public void Stop()
		{
			
			IsOn = false;
		}

		/// <summary>
		/// Blink animation that turns the LED on and off based on the OnDuration and offDuration values in ms
		/// </summary>
		/// <param name="onDuration"></param>
		/// <param name="offDuration"></param>
		public void StartBlink(uint onDuration = 200, uint offDuration = 200)
		{
			Stop();
			IsAnimating = true;
			Thread tsk = new Thread(new ThreadStart(()=> StartBlinkAsync(onDuration, offDuration)));
			tsk.Start();
		}

		protected void StartBlinkAsync(uint onDuration, uint offDuration)
		{
			while (IsAnimating)
			{
				IsOn = true;
				Thread.Sleep((int)onDuration);
				IsOn = false;
				Thread.Sleep((int)offDuration);
			}
		}
	}
}
