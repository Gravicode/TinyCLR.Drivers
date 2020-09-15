// Decompiled with JetBrains decompiler
// Type: Meadow.Utilities.BitHelpers
// Assembly: Meadow, Version=0.15.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D9B35635-6FC1-451D-8AC3-289CFF008676
// Assembly location: D:\experiment\TINYCLR\Meadow.Foundation\Source\Meadow.Foundation.Libraries_and_Frameworks\Sensors.GPS.NMEA\bin\Debug\net472\Meadow.dll

namespace Meadow.Utilities
{
  public static class BitHelpers
  {
    public static byte SetBit(byte mask, byte bitIndex, byte value) => BitHelpers.SetBit(mask, bitIndex, value != (byte) 0);

    public static byte SetBit(byte mask, byte bitIndex, bool value)
    {
      byte num = mask;
      return !value ? (byte) ((uint) num & (uint) (byte) ~(1 << (int) bitIndex)) : (byte) ((uint) num | (uint) (byte) (1U << (int) bitIndex));
    }

    public static bool GetBitValue(byte mask, byte bitIndex) => ((int) mask & (int) (byte) (1U << (int) bitIndex)) != 0;
  }
}
