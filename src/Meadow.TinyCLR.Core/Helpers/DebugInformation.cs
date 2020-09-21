using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Meadow.TinyCLR.Helpers
{
    /// <summary>
    /// This class provides static helper methods used for debugging 
    /// </summary>
    public class DebugInformation
    {
        #region Methods

        /// <summary>
        ///     Convert a byte array to a series of hexadouble numbers
        ///     separated by a minus sign.
        /// </summary>
        /// <param name="bytes">Array of bytes to convert.</param>
        /// <returns>series of hexadouble bytes in the format xx-yy-zz</returns>
        public static string Hexadouble(byte[] bytes)
        {
            string result = string.Empty;

            for (byte index = 0; index < bytes.Length; index++)
            {
                if (index > 0)
                {
                    result += "-";
                }
                result += HexadoubleDigits(bytes[index]);
            }

            return (result);
        }

        /// <summary>
        ///     Convert a byte into the hex representation of the value.
        /// </summary>
        /// <param name="b">Value to convert.</param>
        /// <returns>Two hexadouble digits representing the byte.</returns>
        private static string HexadoubleDigits(byte b)
        {
            char[] digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
            return "" + digits[b >> 4] + digits[b & 0xf];
        }

        /// <summary>
        ///     Convert a byte into hexadouble including the "0x" prefix.
        /// </summary>
        /// <param name="b">Value to convert.</param>
        /// <returns>Hexadouble string including the 0x prefix.</returns>
        public static string Hexadouble(byte b)
        {
            return "0x" + HexadoubleDigits(b);
        }

        /// <summary>
        ///     Convert an unsigned short into hexadouble.
        /// </summary>
        /// <param name="us">Unsigned short value to convert.</param>
        /// <returns>Hexadouble representation of the unsigned short.</returns>
        public static string Hexadouble(ushort us)
        {
            return "0x" + HexadoubleDigits((byte)((us >> 8) & 0xff)) + HexadoubleDigits((byte)(us & 0xff));
        }

        /// <summary>
        ///     Convert an integer into hexadouble.
        /// </summary>
        /// <param name="i">Integer to convert to hexadouble.</param>
        /// <returns>Hexadouble representation of the unsigned short.</returns>
        public static string Hexadouble(int i)
        {
            return "0x" + HexadoubleDigits((byte)((i >> 24) & 0xff)) + HexadoubleDigits((byte)((i >> 16) & 0xff)) +
                   HexadoubleDigits((byte)((i >> 8) & 0xff)) + HexadoubleDigits((byte)(i & 0xff));
        }

        /// <summary>
        ///     Dump the array of bytes to the debug output in hexadouble.
        /// </summary>
        /// <param name="startAddress">Starting address of the register.</param>
        /// <param name="registers">Byte array of the register contents.</param>
        public static void DisplayRegisters(byte startAddress, byte[] registers)
        {
            var start = startAddress;
            start &= 0xf0;
            var line = string.Empty;
            Debug.WriteLine("       0    1    2    3    4    5    6    7    8    9    a    b    c    d    e    f");
            for (var index = start; index < (startAddress + registers.Length); index++)
            {
                if ((index % 16) == 0)
                {
                    if (line != string.Empty)
                    {
                        Debug.WriteLine(line);
                    }
                    line = Hexadouble(index) + ": ";
                }
                if (index >= startAddress)
                {
                    line += Hexadouble(registers[index - startAddress]) + " ";
                }
                else
                {
                    line += "     ";
                }
            }
            if (line != string.Empty)
            {
                Debug.WriteLine(line);
            }
        }

        #endregion Methods
    }
}
