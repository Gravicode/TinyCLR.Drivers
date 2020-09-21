// Decompiled with JetBrains decompiler
// Type: Meadow.Peripherals.Sensors.Location.Gnss.GnssPositionInfo
// Assembly: Meadow, Version=0.15.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D9B35635-6FC1-451D-8AC3-289CFF008676
// Assembly location: D:\experiment\TINYCLR\Meadow.Foundation\Source\Meadow.Foundation.Libraries_and_Frameworks\Sensors.GPS.NMEA\bin\Debug\net472\Meadow.dll

using System;
using System.Text;


namespace Meadow.Peripherals.Sensors.Location.Gnss
{
  public class GnssPositionInfo : IGnssResult
  {
    public string TalkerID { get; set; } = "GP";

    public string TalkerSystemName => Lookups.KnownTalkerIDs.Contains(this.TalkerID) ? Lookups.KnownTalkerIDs[this.TalkerID].ToString() : "";

    public DateTime TimeOfReading { get; set; }

    public bool Valid { get; set; }

    public double SpeedInKnots { get; set; }

    public double CourseHeading { get; set; }

    public CardinalDirection MagneticVariation { get; set; } = CardinalDirection.Unknown;

    public SphericalPositionInfo Position { get; set; } = new SphericalPositionInfo();

    public FixType FixQuality { get; set; }

    public int NumberOfSatellites { get; set; }

    public double HorizontalDilutionOfPrecision { get; set; }

    public override string ToString()
    {
      StringBuilder stringBuilder1 = new StringBuilder();
      stringBuilder1.Append("GnssPositionInfo: {\r\n");
      stringBuilder1.Append("\tTalker ID: " + this.TalkerID + ", talker name: " + this.TalkerSystemName + "\r\n");
      StringBuilder stringBuilder2 = stringBuilder1;
      DateTime timeOfReading = this.TimeOfReading;
      ref DateTime local1 = ref timeOfReading;
      string str1 = "\tTime of reading: " + (local1.ToString()) + "\r\n";
      stringBuilder2.Append(str1);
      stringBuilder1.Append(string.Format("\tValid: {0}\r\n", (object) this.Valid));
      stringBuilder1.Append("\tLatitude: " + (this.Position?.Latitude?.ToString() ?? "null") + "\r\n");
      stringBuilder1.Append("\tLongitude: " + (this.Position?.Longitude?.ToString() ?? "null") + "\r\n");
      StringBuilder stringBuilder3 = stringBuilder1;
      SphericalPositionInfo position = this.Position;
      double valueOrDefault;
      string str2;
      if (position == null)
      {
        str2 = (string) null;
      }
      else
      {
        var altitude = position.Altitude;
        ref double local2 = ref altitude;
       
          valueOrDefault = local2;
          str2 = valueOrDefault.ToString("f2");
        
      }
      if (str2 == null)
        str2 = "null";
      string str3 = "\tAltitude: " + str2 + "\r\n";
      stringBuilder3.Append(str3);
      StringBuilder stringBuilder4 = stringBuilder1;
      double speedInKnots = this.SpeedInKnots;
      ref double local3 = ref speedInKnots;
      string str4;
     
        valueOrDefault = local3;
        str4 = valueOrDefault.ToString("f2");
      
      if (str4 == null)
        str4 = "null";
      string str5 = "\tSpeed in Knots: " + str4 + "\r\n";
      stringBuilder4.Append(str5);
      StringBuilder stringBuilder5 = stringBuilder1;
      double courseHeading = this.CourseHeading;
      ref double local4 = ref courseHeading;
      string str6;
      
        valueOrDefault = local4;
        str6 = valueOrDefault.ToString("f2");
      
      if (str6 == null)
        str6 = "null";
      string str7 = "\tCourse Heading: " + str6 + "\r\n";
      stringBuilder5.Append(str7);
      stringBuilder1.Append(string.Format("\tMagnetic Variation: {0}\r\n", (object) this.MagneticVariation));
      StringBuilder stringBuilder6 = stringBuilder1;
      int numberOfSatellites = this.NumberOfSatellites;
      ref int local5 = ref numberOfSatellites;
      string str8 = "\tNumber of satellites: " + ( local5.ToString() ) + "\r\n";
      stringBuilder6.Append(str8);
      StringBuilder stringBuilder7 = stringBuilder1;
      FixType fixQuality = this.FixQuality;
      ref FixType local6 = ref fixQuality;
      string str9 = "\tFix quality: " + (local6.ToString()) + "\r\n";
      stringBuilder7.Append(str9);
      StringBuilder stringBuilder8 = stringBuilder1;
      double dilutionOfPrecision = this.HorizontalDilutionOfPrecision;
      ref double local7 = ref dilutionOfPrecision;
      string str10;
     
        valueOrDefault = local7;
        str10 = valueOrDefault.ToString("f2");
      
      if (str10 == null)
        str10 = "null";
      string str11 = "\tHDOP: " + str10 + "\r\n";
      stringBuilder8.Append(str11);
      stringBuilder1.Append("}");
      return stringBuilder1.ToString();
    }
  }
}
