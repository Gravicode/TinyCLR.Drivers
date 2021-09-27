/*
Driver ported from http://wiki.sunfounder.cc/images/b/bb/CharacterDisplay_for_Raspberry_Pi.zip
For reference: http://wiki.sunfounder.cc/index.php?title=CharacterDisplay_Module
Brian Kim 5/5/2018
*/

using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Pwm;
using Meadow.TinyCLR.Core.Interface;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;


namespace Meadow.TinyCLR.Displays.Lcd
{
    public class CharacterDisplay : ITextDisplay
    {
        protected ICharacterDisplay characterDisplay;

        public TextDisplayConfig DisplayConfig => characterDisplay?.DisplayConfig;
        public CharacterDisplay(
            
            int pinRS,
            int pinE,
            int pinD4,
            int pinD5,
            int pinD6,
            int pinD7,
            byte rows = 4, byte columns = 20)
        {
            characterDisplay = new GpioCharacterDisplay(pinRS, pinE, pinD4, pinD5, pinD6, pinD7, rows, columns);
        }

       

        public CharacterDisplay(
            
            string PwmControllerName,
            int pinRS,
            int pinE,
            int pinD4,
            int pinD5,
            int pinD6,
            int pinD7,
            byte rows = 4, byte columns = 20)
        {
            characterDisplay = new GpioCharacterDisplay(PwmControllerName, pinRS, pinE, pinD4, pinD5, pinD6, pinD7, rows, columns);
        }

        //public CharacterDisplay(
        //    PwmChannel portV0,
        //    int portRS,
        //    int portE,
        //    int portD4,
        //    int portD5,
        //    int portD6,
        //    int portD7,
        //    ushort rows = 4, ushort columns = 20)
        //{
        //    characterDisplay = new GpioCharacterDisplay(portV0, portRS, portE, portD4, portD5, portD6, portD7, rows, columns);
        //}
        /*
        public CharacterDisplay(II2cBus i2cBus, byte address = I2cCharacterDisplay.DefaultI2cAddress, byte rows = 4, byte columns = 20)
        {
            characterDisplay = new I2cCharacterDisplay(i2cBus, address, rows, columns);
        }*/

        public void ClearLine(byte lineNumber)
        {
            characterDisplay?.ClearLine(lineNumber);
        }

        public void ClearLines()
        {
            characterDisplay?.ClearLines();
        }

        public void SaveCustomCharacter(byte[] characterMap, byte address)
        {
            characterDisplay?.SaveCustomCharacter(characterMap, address);
        }

        public void SetCursorPosition(byte column, byte line)
        {
            characterDisplay?.SetCursorPosition(column, line);
        }

        public void Write(string text)
        {
            characterDisplay?.Write(text);
        }

        public void WriteLine(string text, byte lineNumber, bool showCursor = false)
        {
            characterDisplay?.WriteLine(text, lineNumber);
        }

        public void Show()
        {
            characterDisplay?.Show();
        }
    }
}