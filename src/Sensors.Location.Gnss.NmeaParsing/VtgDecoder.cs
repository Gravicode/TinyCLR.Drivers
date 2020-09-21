using System;
using Meadow.Peripherals.Sensors.Location.Gnss;

namespace Meadow.TinyCLR.Sensors.Location.Gnss.NmeaParsing
{
    public delegate void CourseOverGroundHandler(object sender, CourseOverGround e);
    /// <summary>
    /// Parses VTG (Velocity Made Good) messages from a GPS/GNSS receiver.
    /// </summary>
    public class VtgDecoder : INmeaDecoder
    {
        /// <summary>
        /// Event to be raised when a course and velocity message is received and decoded.
        /// </summary>
        public event CourseOverGroundHandler CourseAndVelocityReceived = delegate { };

        /// <summary>
        /// Prefix for the VTG decoder.
        /// </summary>
        public string Prefix
        {
            get => "VTG";
        }

        /// <summary>
        /// Friendly name for the VTG messages.
        /// </summary>
        public string Name
        {
            get => "Velocity made good";
        }

        /// <summary>
        /// Process the data from a VTG message.
        /// </summary>
        /// <param name="sentence">String array of the message components for a VTG message.</param>
        public void Process(NmeaSentence sentence)
        {
            //Debug.WriteLine($"VTGDecoder.Process");

            var course = new CourseOverGround();

            course.TalkerID = sentence.TalkerID;

            double trueHeading;
            if (double.TryParse(sentence.DataElements[0].ToString(), out trueHeading)) {
                course.TrueHeading = trueHeading;
            }
            double magneticHeading;
            if (double.TryParse(sentence.DataElements[2].ToString(), out magneticHeading)) {
                course.MagneticHeading = magneticHeading;
            }
            double knots;
            if (double.TryParse(sentence.DataElements[4].ToString(), out knots)) {
                course.Knots = knots;
            }
            double kph;
            if (double.TryParse(sentence.DataElements[6].ToString(), out kph)) {
                course.Kph = kph;
            }
            //Debug.WriteLine($"VTG process finished: trueHeading:{course.TrueHeading}, magneticHeading:{course.MagneticHeading}, knots:{course.Knots}, kph:{course.Kph}");
            CourseAndVelocityReceived(this, course);
        }

        
    }
}