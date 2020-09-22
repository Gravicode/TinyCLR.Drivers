// Decompiled with JetBrains decompiler
// Type: Meadow.Peripherals.Sensors.Motion.AccelerationConditionChangeResult
// Assembly: Meadow, Version=0.15.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D9B35635-6FC1-451D-8AC3-289CFF008676
// Assembly location: D:\experiment\TINYCLR\Meadow.Foundation\Source\Meadow.Foundation.Libraries_and_Frameworks\Sensors.GPS.NMEA\bin\Debug\net472\Meadow.dll


namespace Meadow.Peripherals.Sensors.Motion
{
  public class AccelerationConditionChangeResult 
  {
    protected AccelerationConditions _newValue = new AccelerationConditions();
    protected AccelerationConditions _oldValue = new AccelerationConditions();

    public AccelerationConditions New
    {
      get => this._newValue;
      set
      {
        this._newValue = value;
        this.RecalcDelta();
      }
    }

    public AccelerationConditions Old
    {
      get => this._oldValue;
      set
      {
        this._oldValue = value;
        this.RecalcDelta();
      }
    }

    public AccelerationConditions Delta { get; protected set; } = new AccelerationConditions();

    public AccelerationConditionChangeResult(
      AccelerationConditions newValue,
      AccelerationConditions oldValue)
    {
      this.New = newValue;
      this.Old = oldValue;
    }

    protected void RecalcDelta()
    {
      AccelerationConditions accelerationConditions1 = new AccelerationConditions();
      AccelerationConditions accelerationConditions2 = accelerationConditions1;
      float xacceleration = this.New.XAcceleration;
      float nullable1 = this.Old.XAcceleration;
      float nullable2 = xacceleration!= 0 & nullable1!= 0 ? (xacceleration - nullable1) : 0;
      accelerationConditions2.XAcceleration = nullable2;
      AccelerationConditions accelerationConditions3 = accelerationConditions1;
      nullable1 = this.New.YAcceleration;
      float nullable3 = this.Old.YAcceleration;
      float nullable4 = nullable1!= 0 & nullable3!= 0 ? (nullable1 - nullable3) : 0;
      accelerationConditions3.YAcceleration = nullable4;
      AccelerationConditions accelerationConditions4 = accelerationConditions1;
      nullable3 = this.New.ZAcceleration;
      nullable1 = this.Old.ZAcceleration;
      float nullable5 = nullable3!= 0 & nullable1!= 0 ? (nullable3 - nullable1) : 0;
      accelerationConditions4.ZAcceleration = nullable5;
      AccelerationConditions accelerationConditions5 = accelerationConditions1;
      nullable1 = this.New.XGyroscopicAcceleration;
      nullable3 = this.Old.XGyroscopicAcceleration;
      float nullable6 = nullable1!= 0 & nullable3!= 0 ? (nullable1 - nullable3) : 0;
      accelerationConditions5.XGyroscopicAcceleration = nullable6;
      AccelerationConditions accelerationConditions6 = accelerationConditions1;
      nullable3 = this.New.YGyroscopicAcceleration;
      nullable1 = this.Old.YGyroscopicAcceleration;
      float nullable7 = nullable3!= 0 & nullable1!= 0 ? (nullable3 - nullable1) : 0;
      accelerationConditions6.YGyroscopicAcceleration = nullable7;
      AccelerationConditions accelerationConditions7 = accelerationConditions1;
      nullable1 = this.New.ZGyroscopicAcceleration;
      nullable3 = this.Old.ZGyroscopicAcceleration;
      float nullable8 = nullable1!= 0 & nullable3!= 0 ? (nullable1 - nullable3) : 0;
      accelerationConditions7.ZGyroscopicAcceleration = nullable8;
      this.Delta = accelerationConditions1;
    }
  }
}
