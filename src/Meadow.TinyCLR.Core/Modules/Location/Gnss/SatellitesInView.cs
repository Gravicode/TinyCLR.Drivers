// Decompiled with JetBrains decompiler
// Type: Meadow.Peripherals.Sensors.Location.Gnss.SatellitesInView
// Assembly: Meadow, Version=0.15.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D9B35635-6FC1-451D-8AC3-289CFF008676
// Assembly location: D:\experiment\TINYCLR\Meadow.Foundation\Source\Meadow.Foundation.Libraries_and_Frameworks\Sensors.GPS.NMEA\bin\Debug\net472\Meadow.dll

using System.Text;


namespace Meadow.Peripherals.Sensors.Location.Gnss
{
  public class SatellitesInView : IGnssResult
  {
    public string TalkerID { get; set; } = "GP";

    public string TalkerSystemName => Lookups.KnownTalkerIDs.Contains(this.TalkerID) ? Lookups.KnownTalkerIDs[this.TalkerID].ToString() : "";

    public Satellite[] Satellites { get; protected set; }

    public SatellitesInView(Satellite[] satellites) => this.Satellites = satellites;

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("SatellitesInView: {\r\n");
      stringBuilder.Append("\tTalker ID: " + this.TalkerID + ", talker name: " + this.TalkerSystemName + "\r\n");
      stringBuilder.Append("\tSatellites:\r\n");
      foreach (Satellite satellite in this.Satellites)
        stringBuilder.Append(string.Format("\t{0}\r\n", (object) satellite));
      stringBuilder.Append("}");
      return stringBuilder.ToString();
    }
  }
}
