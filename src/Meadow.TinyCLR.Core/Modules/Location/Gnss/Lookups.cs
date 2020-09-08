// Decompiled with JetBrains decompiler
// Type: Meadow.Peripherals.Sensors.Location.Gnss.Lookups
// Assembly: Meadow, Version=0.15.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D9B35635-6FC1-451D-8AC3-289CFF008676
// Assembly location: D:\experiment\TINYCLR\Meadow.Foundation\Source\Meadow.Foundation.Libraries_and_Frameworks\Sensors.GPS.NMEA\bin\Debug\net472\Meadow.dll


using System.Collections;

namespace Meadow.Peripherals.Sensors.Location.Gnss
{
  public static class Lookups
  {
    public static Hashtable KnownTalkerIDs { get; } = new Hashtable();

    static Lookups() => Lookups.PopulateKnownTalkerIDs();

    private static void PopulateKnownTalkerIDs()
    {
      Lookups.KnownTalkerIDs.Add("BD", "BeiDou");
      Lookups.KnownTalkerIDs.Add("CD", "Digital Selective Calling (DSC)");
      Lookups.KnownTalkerIDs.Add("EC", "Electronic Chart Display & Information System (ECDIS)");
      Lookups.KnownTalkerIDs.Add("GA", "Galileo Positioning System");
      Lookups.KnownTalkerIDs.Add("GB", "BeiDou");
      Lookups.KnownTalkerIDs.Add("GL", "GLONASS");
      Lookups.KnownTalkerIDs.Add("GN", "Combination of multiple satellite systems.");
      Lookups.KnownTalkerIDs.Add("GP", "Global Positioning System receiver");
      Lookups.KnownTalkerIDs.Add("II", "Integrated Instrumentation");
      Lookups.KnownTalkerIDs.Add("IN", "Integrated Navigation");
      Lookups.KnownTalkerIDs.Add("LC", "Loran-C receiver (obsolete)");
      Lookups.KnownTalkerIDs.Add("QZ", "QZSS regional GPS augmentation system");
      Lookups.KnownTalkerIDs.Add("GI", "NavIC (IRNSS)");
    }
  }
}
