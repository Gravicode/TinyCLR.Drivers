// Decompiled with JetBrains decompiler
// Type: Meadow.Peripherals.Sensors.Location.Gnss.ActiveSatellites
// Assembly: Meadow, Version=0.15.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D9B35635-6FC1-451D-8AC3-289CFF008676
// Assembly location: D:\experiment\TINYCLR\Meadow.Foundation\Source\Meadow.Foundation.Libraries_and_Frameworks\Sensors.GPS.NMEA\bin\Debug\net472\Meadow.dll

using System;
using System.Text;



namespace Meadow.Peripherals.Sensors.Location.Gnss
{
  public class ActiveSatellites : IGnssResult
  {
    public string TalkerID { get; set; } = "GP";

    public string TalkerSystemName => Lookups.KnownTalkerIDs.Contains(this.TalkerID)? Lookups.KnownTalkerIDs[this.TalkerID].ToString() : "";

    public DateTime TimeOfReading { get; set; }

    public DimensionalFixType Dimensions { get; set; }

    public ActiveSatelliteSelection SatelliteSelection { get; set; }

    public string[] SatellitesUsedForFix { get; set; }

    public double DilutionOfPrecision { get; set; }

    public double HorizontalDilutionOfPrecision { get; set; }

    public double VerticalDilutionOfPrecision { get; set; }

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("Active Satellites: {\r\n");
      stringBuilder.Append("\tTalker ID: " + this.TalkerID + ", talker name: " + this.TalkerSystemName + "\r\n");
      stringBuilder.Append(string.Format("\tTime of reading: {0}\r\n", (object) this.TimeOfReading));
      stringBuilder.Append(string.Format("\tNumber of satellites involved in fix: {0}\r\n", (object) this.SatellitesUsedForFix.Length));
      stringBuilder.Append(string.Format("\tDilution of precision: {0:f2}\r\n", (object) this.DilutionOfPrecision));
      stringBuilder.Append(string.Format("\tHDOP: {0:f2}\r\n", (object) this.HorizontalDilutionOfPrecision));
      stringBuilder.Append(string.Format("\tVDOP: {0:f2}\r\n", (object) this.VerticalDilutionOfPrecision));
      if (this.SatellitesUsedForFix != null)
      {
        stringBuilder.Append("\tSatellites used for fix:\r\n");
        foreach (string str in this.SatellitesUsedForFix)
          stringBuilder.Append("\t" + str + "\r\n");
      }
      stringBuilder.Append("}");
      return stringBuilder.ToString();
    }
  }
}
