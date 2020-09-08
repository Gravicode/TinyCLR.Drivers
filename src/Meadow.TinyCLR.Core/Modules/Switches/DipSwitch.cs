using GHIElectronics.TinyCLR.Devices.Gpio;
using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Meadow.TinyCLR.Core;
using Meadow.TinyCLR.Interface;

namespace Meadow.TinyCLR.Modules.Switches
{
    /// <summary>
    /// Represents a DIP-switch wired in a bus configuration, in which all switches 
    /// are terminated to the same ground/common or high pin.
    ///     
    /// Note: This class is not yet implemented.
    /// </summary>
    public class DipSwitch
    {
        #region Properties

        /// <summary>
        /// Returns the ISwitch at the specified index.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public ISwitch this[int i] => Switches[i];

        /// <summary>
        /// Returns the switch array.
        /// </summary>
        public ISwitch[] Switches = null;

        /// <summary>
        /// Raised when one of the switches is switched on or off.
        /// </summary>
        public event ArrayEventHandler Changed = delegate { };

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new DipSwitch connected to the specified switchPins, with the InterruptMode and ResisterMode specified by the type parameters.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="switchPins"></param>
        /// <param name="interruptMode"></param>
        /// <param name="resistorMode"></param>
        /// <param name="debounceDuration"></param>
        /// <param name="glitchFilterCycleCount"></param>
        public DipSwitch(int[] switchPins, GpioPinEdge interruptMode, GpioPinDriveMode resistorMode, int debounceDuration = 20, int glitchFilterCycleCount = 0)
        {
            Switches = new ISwitch[switchPins.Length];

            for (int i = 0; i < switchPins.Length; i++)
            {
                Switches[i] = new SpstSwitch(switchPins[i], interruptMode, resistorMode, debounceDuration, glitchFilterCycleCount);
                int index = i;
                Switches[i].Changed += (s, e) => HandleSwitchChanged(index);
            }
        }

        /// <summary>
        /// Creates a new DipSwitch connected to an array of Interrupt Ports
        /// </summary>
        /// <param name="interruptPorts"></param>
        public DipSwitch(GpioPin[] interruptPorts)
        {
            Switches = new ISwitch[interruptPorts.Length];

            for (int i = 0; i < interruptPorts.Length; i++)
            {
                Switches[i] = new SpstSwitch(interruptPorts[i]);
                int index = i;
                Switches[i].Changed += (s, e) => HandleSwitchChanged(index);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Event handler when switch value has been changed
        /// </summary>
        /// <param name="switchNumber"></param>
        protected void HandleSwitchChanged(int switchNumber)
        {
            Debug.WriteLine($"HandleSwitchChange: {switchNumber} total switches: {Switches.Length}");
            Changed(this, new ArrayEventArgs(switchNumber, Switches[switchNumber]));
        }

        #endregion
    }
}
