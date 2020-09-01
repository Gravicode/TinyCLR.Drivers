﻿using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace TinyCLR.Drivers.Modules.Led
{
    public static class LedHelpers
    {
        /// <summary>
        /// Calculates the maximum duty cycle based on the voltage drop/Forward Voltage/Vf
        /// of the LED.
        /// </summary>
        /// <param name="Vf"></param>
        /// <returns></returns>
        public static float CalculateMaximumDutyCycle(float forwardVoltage)
        {
            // clamp to our maximum output voltage
            float Vf = forwardVoltage;
            if (Vf > 3.3) { Vf = 3.3F; }

            // 1.8V / 3.3V = .55 = 55%
            float maxDutyPercent = Vf / 3.3F;

            return maxDutyPercent;
        }
    }

}
