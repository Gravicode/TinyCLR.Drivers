using GHIElectronics.TinyCLR.Devices.Pwm;
using Meadow.TinyCLR.Interface;

namespace Meadow.Foundation.Servos
{
    public class Servo : ServoBase
    {
        public Servo(IPwmPort pwm, ServoConfig config) : base(pwm, config)
        {

        }

       
    }
}