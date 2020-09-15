// Decompiled with JetBrains decompiler
// Type: Meadow.Peripherals.Sensors.Atmospheric.AtmosphericConditionChangeResult
// Assembly: Meadow, Version=0.15.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D9B35635-6FC1-451D-8AC3-289CFF008676
// Assembly location: D:\experiment\TINYCLR\Meadow.Foundation\Source\Meadow.Foundation.Libraries_and_Frameworks\Sensors.GPS.NMEA\bin\Debug\net472\Meadow.dll



namespace Meadow.Peripherals.Sensors.Atmospheric
{
  public class AtmosphericConditionChangeResult 
  {
    protected AtmosphericConditions _newValue = new AtmosphericConditions();
    protected AtmosphericConditions _oldValue = new AtmosphericConditions();

    public AtmosphericConditions New
    {
      get => this._newValue;
      set
      {
        this._newValue = value;
        this.RecalcDelta();
      }
    }

    public AtmosphericConditions Old
    {
      get => this._oldValue;
      set
      {
        this._oldValue = value;
        this.RecalcDelta();
      }
    }

    public AtmosphericConditions Delta { get; protected set; } = new AtmosphericConditions();

    public AtmosphericConditionChangeResult(
      AtmosphericConditions newValue,
      AtmosphericConditions oldValue)
    {
      this.New = newValue;
      this.Old = oldValue;
    }

    protected void RecalcDelta()
    {
      AtmosphericConditions atmosphericConditions1 = new AtmosphericConditions();
      AtmosphericConditions atmosphericConditions2 = atmosphericConditions1;
      float temperature = this.New.Temperature;
      float nullable1 = this.Old.Temperature;
      float nullable2 =  temperature - nullable1 ;
      atmosphericConditions2.Temperature = nullable2;
      AtmosphericConditions atmosphericConditions3 = atmosphericConditions1;
      nullable1 = this.New.Pressure;
      float nullable3 = this.Old.Pressure;
      float nullable4 = nullable1 - nullable3;
      atmosphericConditions3.Pressure = nullable4;
      AtmosphericConditions atmosphericConditions4 = atmosphericConditions1;
      nullable3 = this.New.Humidity;
      nullable1 = this.Old.Humidity;
      float nullable5 = nullable3 - nullable1;
      atmosphericConditions4.Humidity = nullable5;
      this.Delta = atmosphericConditions1;
    }
  }
}
