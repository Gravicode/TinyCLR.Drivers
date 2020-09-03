﻿using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace TinyCLR.Drivers.Modules.Rotary
{
    /// <summary>
    /// Represents a 2 bit version of GrayCode (https://en.wikipedia.org/wiki/Gray_code) used to 
    /// encode the current state of a rotary encoder. 
    /// </summary>
    public struct TwoBitGrayCode
    {
        public bool APhase;
        public bool BPhase;
    }
}
