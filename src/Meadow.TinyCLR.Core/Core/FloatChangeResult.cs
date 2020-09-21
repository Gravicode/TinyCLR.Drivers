// Decompiled with JetBrains decompiler
// Type: Meadow.FloatChangeResult
// Assembly: Meadow, Version=0.15.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D9B35635-6FC1-451D-8AC3-289CFF008676
// Assembly location: D:\experiment\TINYCLR\Meadow.Foundation\Source\Meadow.Foundation.Libraries_and_Frameworks\Sensors.GPS.NMEA\bin\Debug\net472\Meadow.dll

namespace Meadow
{
    public abstract class NumericChangeResultBase
    {
        public NumericChangeResultBase(float newValue, float oldValue) {
            this.New = newValue;
            this.Old = oldValue;
        }

        public float New { get; set; }
        public float Old { get; set; }
        public abstract float Delta { get; }
        public abstract float DeltaPercent { get; }
    }
    public class FloatChangeResult : NumericChangeResultBase
  {
    public override float Delta => this.New - this.Old;

    public override float DeltaPercent => (float) ((double) this.Delta / (double) this.Old * 100.0);

    public FloatChangeResult(float newValue, float oldValue)
      : base(newValue, oldValue)
    {
    }
  }
}
