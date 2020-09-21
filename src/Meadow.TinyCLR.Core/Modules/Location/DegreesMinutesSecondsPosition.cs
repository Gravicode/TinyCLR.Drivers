// Decompiled with JetBrains decompiler
// Type: Meadow.Peripherals.Sensors.Location.DegreesMinutesSecondsPosition
// Assembly: Meadow, Version=0.15.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D9B35635-6FC1-451D-8AC3-289CFF008676
// Assembly location: D:\experiment\TINYCLR\Meadow.Foundation\Source\Meadow.Foundation.Libraries_and_Frameworks\Sensors.GPS.NMEA\bin\Debug\net472\Meadow.dll

using System;


namespace Meadow.Peripherals.Sensors.Location
{
  public class DegreesMinutesSecondsPosition
  {
    public CardinalDirection Direction;

    public int Degrees { get; set; }

    public double Minutes { get; set; }

    public double seconds { get; set; }

    public override string ToString()
    {
      string str = string.Format("{0:f2}º {1:f2}' {2:f2}\"", (object) this.Degrees, (object) this.Minutes, (object) this.seconds);
      switch (this.Direction)
      {
        case CardinalDirection.North:
          str += "N";
          break;
        case CardinalDirection.South:
          str += "S";
          break;
        case CardinalDirection.East:
          str += "E";
          break;
        case CardinalDirection.West:
          str += "W";
          break;
        case CardinalDirection.Unknown:
          str += "Unknown";
          break;
      }
      return str;
    }
  }
}
