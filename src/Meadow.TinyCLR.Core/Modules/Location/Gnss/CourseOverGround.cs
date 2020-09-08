// Decompiled with JetBrains decompiler
// Type: Meadow.Peripherals.Sensors.Location.Gnss.CourseOverGround
// Assembly: Meadow, Version=0.15.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D9B35635-6FC1-451D-8AC3-289CFF008676
// Assembly location: D:\experiment\TINYCLR\Meadow.Foundation\Source\Meadow.Foundation.Libraries_and_Frameworks\Sensors.GPS.NMEA\bin\Debug\net472\Meadow.dll

using System;
using System.Text;



namespace Meadow.Peripherals.Sensors.Location.Gnss
{
  public class CourseOverGround : IGnssResult
  {
    public string TalkerID { get; set; } = "GP";

    public string TalkerSystemName => Lookups.KnownTalkerIDs.Contains(this.TalkerID) ? Lookups.KnownTalkerIDs[this.TalkerID].ToString() : "";

    public DateTime TimeOfReading { get; set; }

    public Decimal TrueHeading { get; set; }

    public Decimal MagneticHeading { get; set; }

    public Decimal Knots { get; set; }

    public Decimal Kph { get; set; }

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("CourseOverGround: {\r\n");
      stringBuilder.Append("\tTalker ID: " + this.TalkerID + ", talker name: " + this.TalkerSystemName + "\r\n");
      stringBuilder.Append(string.Format("\tTime of reading: {0}\r\n", (object) this.TimeOfReading));
      stringBuilder.Append(string.Format("\tTrue Heading: {0}\r\n", (object) this.TrueHeading));
      stringBuilder.Append(string.Format("\tMagentic Heading: {0}\r\n", (object) this.MagneticHeading));
      stringBuilder.Append(string.Format("\tKnots: {0:f2}\r\n", (object) this.Knots));
      stringBuilder.Append(string.Format("\tKph: {0:f2}\r\n", (object) this.Kph));
      stringBuilder.Append("}");
      return stringBuilder.ToString();
    }
  }
}
