// Decompiled with JetBrains decompiler
// Type: Meadow.Peripherals.Sensors.Location.Gnss.Satellite
// Assembly: Meadow, Version=0.15.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D9B35635-6FC1-451D-8AC3-289CFF008676
// Assembly location: D:\experiment\TINYCLR\Meadow.Foundation\Source\Meadow.Foundation.Libraries_and_Frameworks\Sensors.GPS.NMEA\bin\Debug\net472\Meadow.dll

using System.Text;


namespace Meadow.Peripherals.Sensors.Location.Gnss
{
  public class Satellite
  {
    public int SignalTolNoiseRatio;

    public int ID { get; set; }

    public int Elevation { get; set; }

    public int Azimuth { get; set; }

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(string.Format("ID: {0}, Azimuth: {1}, Elevation: {2}, Signal to Noise Ratio: {3}", (object) this.ID, (object) this.Azimuth, (object) this.Elevation, (object) this.SignalTolNoiseRatio));
      return stringBuilder.ToString();
    }
  }
}
