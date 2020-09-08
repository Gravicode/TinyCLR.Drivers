// Decompiled with JetBrains decompiler
// Type: Meadow.Utilities.ChecksumCalculator
// Assembly: Meadow, Version=0.15.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D9B35635-6FC1-451D-8AC3-289CFF008676
// Assembly location: D:\experiment\TINYCLR\Meadow.Foundation\Source\Meadow.Foundation.Libraries_and_Frameworks\Sensors.GPS.NMEA\bin\Debug\net472\Meadow.dll

using System;
using System.Text;

namespace Meadow.Utilities
{
  public static class ChecksumCalculator
  {
    private static byte[] _lookupTable;
    private static byte _polynomial;

    public static byte XOR(string data) => ChecksumCalculator.XOR(Encoding.UTF8.GetBytes(data));

    public static byte XOR(byte[] data)
    {
      byte num = 0;
      for (int index = 0; index < data.Length; ++index)
        num ^= data[index];
      return num;
    }

    private static void PopulateLookupTable(byte polynomial)
    {
      ChecksumCalculator._lookupTable = new byte[256];
      for (int index1 = 0; index1 < 256; ++index1)
      {
        int num = index1;
        for (int index2 = 0; index2 < 8; ++index2)
        {
          if ((num & 128) != 0)
            num = num << 1 ^ (int) polynomial;
          else
            num <<= 1;
        }
        ChecksumCalculator._lookupTable[index1] = (byte) num;
      }
      ChecksumCalculator._polynomial = polynomial;
    }

    public static byte PolynomialCRC(byte[] data, byte polynomial)
    {
      if (data == null || data.Length == 0)
        throw new ArgumentException(nameof (data), "PolynomialCRC: Data to CRC is invalid.");
      if (ChecksumCalculator._lookupTable == null || ChecksumCalculator._lookupTable != null && (int) ChecksumCalculator._polynomial != (int) polynomial)
        ChecksumCalculator.PopulateLookupTable(polynomial);
      byte num1 = 0;
      foreach (byte num2 in data)
        num1 = ChecksumCalculator._lookupTable[(int) num1 ^ (int) num2];
      return num1;
    }
  }
}
