﻿// Decompiled with JetBrains decompiler
// Type: Meadow.Peripherals.Sensors.Atmospheric.IAtmosphericSensor
// Assembly: Meadow, Version=0.15.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D9B35635-6FC1-451D-8AC3-289CFF008676
// Assembly location: D:\experiment\TINYCLR\Meadow.Foundation\Source\Meadow.Foundation.Libraries_and_Frameworks\Sensors.GPS.NMEA\bin\Debug\net472\Meadow.dll

using System;


namespace Meadow.Peripherals.Sensors.Atmospheric
{
    public delegate void AtmosphericConditionChangeEventHandler(object sender, AtmosphericConditionChangeResult e);
    public interface IAtmosphericSensor 
  {
    AtmosphericConditions Conditions { get; }

    event AtmosphericConditionChangeEventHandler Updated;
  }
}
