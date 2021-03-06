﻿using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace Meadow.TinyCLR.Interface
{
    public enum RelayType
    {
        NormallyOpen = 0,
        NormallyClosed = 1
    }
    public interface IRelay
    {
        bool IsOn { get; set; }
        RelayType Type { get; }

        void Toggle();
    }
}
