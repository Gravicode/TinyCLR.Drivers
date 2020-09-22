// Decompiled with JetBrains decompiler
// Type: Meadow.Peripherals.Sensors.Motion.AccelerationConditions
// Assembly: Meadow, Version=0.15.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D9B35635-6FC1-451D-8AC3-289CFF008676
// Assembly location: D:\experiment\TINYCLR\Meadow.Foundation\Source\Meadow.Foundation.Libraries_and_Frameworks\Sensors.GPS.NMEA\bin\Debug\net472\Meadow.dll


namespace Meadow.Peripherals.Sensors.Motion
{
  public class AccelerationConditions
  {
    public float XAcceleration { get; set; }

    public float YAcceleration { get; set; }

    public float ZAcceleration { get; set; }

    public float XGyroscopicAcceleration { get; set; }

    public float YGyroscopicAcceleration { get; set; }

    public float ZGyroscopicAcceleration { get; set; }

    public AccelerationConditions()
    {
    }

    public AccelerationConditions(
      float xAcceleration,
      float yAcceleration,
      float zAcceleration,
      float xGyroAcceleration,
      float yGyroAcceleration,
      float zGyroAcceleration)
    {
      this.XAcceleration = xAcceleration;
      this.YAcceleration = yAcceleration;
      this.ZAcceleration = zAcceleration;
      this.XGyroscopicAcceleration = xGyroAcceleration;
      this.YGyroscopicAcceleration = yGyroAcceleration;
      this.ZGyroscopicAcceleration = zGyroAcceleration;
    }

    public static AccelerationConditions From(
      AccelerationConditions conditions) => new AccelerationConditions(conditions.XAcceleration, conditions.YAcceleration, conditions.ZAcceleration, conditions.XGyroscopicAcceleration, conditions.YGyroscopicAcceleration, conditions.ZGyroscopicAcceleration);
  }
}
