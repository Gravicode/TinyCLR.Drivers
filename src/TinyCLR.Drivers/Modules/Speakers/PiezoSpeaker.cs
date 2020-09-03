using GHIElectronics.TinyCLR.Devices.Pwm;
using System;
using System.Collections;
using System.Text;
using System.Threading;
using TinyCLR.Drivers.Interface;

namespace TinyCLR.Drivers.Modules.Speakers
{
    /// <summary>
    /// Represents a 2 pin piezo-electric speaker capable of generating tones
    /// </summary>
    public class PiezoSpeaker : IToneGenerator
    {
        /// <summary>
        /// Gets the port that is driving the Piezo Speaker
        /// </summary>
        public PwmChannel Port { get; protected set; }

        private bool isPlaying = false;

        /// <summary>
        /// Create a new PiezoSpeaker instance
        /// </summary>
        /// <param name="pin">PWM Pin connected to the PiezoSpeaker</param>
        public PiezoSpeaker(string PwmControllerName, int pin, float frequency = 100, float dutyCycle = 0) 
            
        {
            var controller = PwmController.FromName(PwmControllerName);
            var pwmPort = controller.OpenChannel(pin);
            Port.Controller.SetDesiredFrequency(frequency);
            Port.SetActiveDutyCyclePercentage(dutyCycle);
        }

        /// <summary>
        /// Create a new PiezoSpeaker instance
        /// </summary>
        /// <param name="port"></param>
        public PiezoSpeaker(PwmChannel port)
        {
            Port = port;
        }

        /// <summary>
        /// Play a frequency for a specified duration
        /// </summary>
        /// <param name="frequency">The frequency in hertz of the tone to be played</param>
        /// <param name="duration">How long the note is played in milliseconds, if durration is 0, tone plays indefinitely</param>
        public void PlayTone(float frequency, int duration = 0)
        {
            if (frequency <= 1)
            {
                throw new Exception("Piezo frequency must be greater than 1");
            }

            if (!isPlaying)
            {
                isPlaying = true;

             
                Port.Controller.SetDesiredFrequency(frequency);
                Port.SetActiveDutyCyclePercentage(0.5f);

                Port.Start();

                if (duration > 0)
                {
                    Thread.Sleep(duration);
                    Port.Stop();
                }

                isPlaying = false;
            }
        }

        /// <summary>
        /// Stops a tone playing
        /// </summary>
        public void StopTone()
        {
            Port.Stop();
        }
    }
}
