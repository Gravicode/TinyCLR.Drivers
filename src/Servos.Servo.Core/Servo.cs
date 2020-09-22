using GHIElectronics.TinyCLR.Devices.Pwm;

namespace Meadow.Foundation.Servos
{
    public class Servo : ServoBase
    {
        public Servo(PwmChannel pwm, ServoConfig config) : base(pwm, config)
        {

        }
    }
}