// Decompiled with JetBrains decompiler
// Type: Meadow.Peripherals.Sensors.Atmospheric.AtmosphericConditions
// Assembly: Meadow, Version=0.15.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D9B35635-6FC1-451D-8AC3-289CFF008676
// Assembly location: D:\experiment\TINYCLR\Meadow.Foundation\Source\Meadow.Foundation.Libraries_and_Frameworks\Sensors.GPS.NMEA\bin\Debug\net472\Meadow.dll


namespace Meadow.Peripherals.Sensors.Atmospheric
{
  public class AtmosphericConditions
  {
    public float Temperature { get; set; }

    public float Pressure { get; set; }

    public float Humidity { get; set; }

    public AtmosphericConditions()
    {
    }

    public AtmosphericConditions(float temperature, float pressure, float humidity)
    {
      this.Temperature = temperature;
      this.Pressure = pressure;
      this.Humidity = humidity;
    }

    public static AtmosphericConditions From(AtmosphericConditions conditions) => new AtmosphericConditions(conditions.Temperature, conditions.Pressure, conditions.Humidity);
  }
}
