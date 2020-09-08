using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace Meadow.TinyCLR.Interface
{
    public interface IPidController
    {
        float OutputMin { get; set; }
        float OutputMax { get; set; }
        float ActualInput { get; set; }
        float TargetInput { get; set; }
        float IntegralComponent { get; set; }
        float DerivativeComponent { get; set; }
        float ProportionalComponent { get; set; }
        bool OutputTuningInformation { get; set; }

        float CalculateControlOutput();
        void ResetIntegrator();
    }
}
