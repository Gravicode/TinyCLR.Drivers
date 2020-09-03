using GHIElectronics.TinyCLR.Devices.Gpio;
using System;
using System.Collections;
using System.Text;
using System.Threading;
using TinyCLR.Drivers.Interface;

namespace TinyCLR.Drivers.Modules.Rotary
{
    /// <summary>
    /// Digital rotary encoder that uses two-bit Gray Code to encode rotation.
    /// </summary>
    public class RotaryEncoder : IRotaryEncoder
    {
        #region Properties

        /// <summary>
        /// Returns the pin connected to the A-phase output on the rotary encoder.
        /// </summary>
        public GpioPin APhasePort { get; set; }

        /// <summary>
        /// Returns the pin connected to the B-phase output on the rotary encoder.
        /// </summary>
        public GpioPin BPhasePort { get; set; }

        /// <summary>
        /// Raised when the rotary encoder is rotated and returns a RotaryTurnedEventArgs object which describes the direction of rotation.
        /// </summary>
        public event RotaryTurnedEventHandler Rotated = delegate { };

        #endregion

        #region Member variables / fields

        /// <summary>
        /// Contains the previous offset used to find direction information
        /// </summary>
        private uint _dynamicOffset = 0;

        /// <summary>
        /// The rotary encoder has 2 inputs, called A and B. Because of its design
        /// either A or B changes but not both on each notification when the encoder
        /// is rotated. Because of this the direction to be determined. If A goes
        /// High before B then we are rotating one direction if B goes high before A
        /// we are rotating the other direction. For each change we must consider the
        /// previous state of A and B and the current state of A and B. This can be
        /// represented as 4-bit number.
        ///  3 2 1 0
        /// |old|new|
        /// |A|B|A|B|
        ///
        /// Bits 0 and 1 represent the current state of A and B while bits 2 and 3
        /// represent previous states of A and B. This 4-bit number yields 16 possible
        /// combination, however, there are combination that for which not change is
        /// represent. For example, the if bits 0-3 are all 0, this would mean that A
        /// was Low and is Low, and that B was Low and is Low (nothing changed.)
        /// </summary>
        private readonly int[] RotEncLookup = { 0, -1, 1, 0, 1, 0, 0, -1, -1, 0, 0, 1, 0, 1, -1, 0 };

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiate a new RotaryEncoder on the specified pins.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="aPhasePin"></param>
        /// <param name="bPhasePin"></param>
        public RotaryEncoder(int aPhasePin, int bPhasePin) 
           
        {
            /*
              this(device.CreateDigitalInputPort(aPhasePin, InterruptMode.EdgeBoth, ResistorMode.PullUp, 0, .5),
                 device.CreateDigitalInputPort(bPhasePin, InterruptMode.EdgeBoth, ResistorMode.PullUp, 0, .5))
             */
            var gpio = GpioController.GetDefault();
            var pinA = gpio.OpenPin(aPhasePin);
            pinA.SetDriveMode( GpioPinDriveMode.InputPullUp);
            pinA.DebounceTimeout = new TimeSpan(0, 0, 0, 500);

            var pinB = gpio.OpenPin(bPhasePin);
            pinB.SetDriveMode(GpioPinDriveMode.InputPullUp);
            pinB.DebounceTimeout = new TimeSpan(0, 0, 0, 500);
            Setup(pinA,pinB);
        }

        /// <summary>
        /// Instantiate a new RotaryEncoder on the specified ports
        /// </summary>
        /// <param name="aPhasePort"></param>
        /// <param name="bPhasePort"></param>
        public RotaryEncoder(GpioPin aPhasePort, GpioPin bPhasePort)
        {
            Setup(aPhasePort, bPhasePort);
        }

        void Setup(GpioPin aPhasePort, GpioPin bPhasePort)
        {
            APhasePort = aPhasePort;
            BPhasePort = bPhasePort;

            APhasePort.ValueChanged += APhasePort_ValueChanged;
            BPhasePort.ValueChanged += BPhasePort_ValueChanged;
        }



        #endregion

        #region Methods

        private void BPhasePort_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            // Clear bit B bit
            uint new2LsBits = _dynamicOffset & 0x01;    // Save bit 1 (A)
            if (BPhasePort.Read()==GpioPinValue.High)
                new2LsBits |= 0x02;                     // Set bit 2 (B)

            FindDirection(new2LsBits);
        }

        private void APhasePort_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            // Clear bit A bit
            uint new2LsBits = _dynamicOffset & 0x02;    // Save bit 2 (B)
            if (APhasePort.Read() == GpioPinValue.High)
                new2LsBits |= 0x01;                     // Set bit 1 (A)

            FindDirection(new2LsBits);
        }

        private void FindDirection(uint new2LsBits)
        {
            _dynamicOffset <<= 2;          // Move previous A & B to bits 2 & 3
            _dynamicOffset |= new2LsBits;  // Set the current A & B states in bits 0 & 1
            _dynamicOffset &= 0x0000000f;  // Save only lowest 4 bits

            int direction = RotEncLookup[_dynamicOffset];
            if (direction == 0)
                return;                 // nothing changed

            if (direction == 1)
                Rotated?.Invoke(this, new RotaryTurnedEventArgs(RotationDirection.Clockwise));
            else
                Rotated?.Invoke(this, new RotaryTurnedEventArgs(RotationDirection.CounterClockwise));
        }

        #endregion
    }
}
