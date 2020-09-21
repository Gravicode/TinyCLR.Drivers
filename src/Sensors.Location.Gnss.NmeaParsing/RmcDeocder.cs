using System;
using Meadow.Peripherals.Sensors.Location;
using Meadow.Peripherals.Sensors.Location.Gnss;

namespace Meadow.TinyCLR.Sensors.Location.Gnss.NmeaParsing
{
    /// <summary>
    /// Decode RMC - Recommended Minimum Specific GPS messages.
    /// </summary>
    public class RmcDecoder : INmeaDecoder
    {
        /// <summary>
        /// Position update received event.
        /// </summary>
        public event GnssPositionInfoHandler PositionCourseAndTimeReceived = delegate { };

        /// <summary>
        /// Prefix for the RMBC decoder.
        /// </summary>
        public string Prefix {
            get => "RMC";
        }

        /// <summary>
        /// Friendly name for the RMC messages.
        /// </summary>
        public string Name {
            get => "Recommended Minimum";
        }

        /// <summary>
        /// Process the data from a RMC
        /// </summary>
        /// <param name="data">String array of the message components for a RMC message.</param>
        public void Process(NmeaSentence sentence)
        {
            var position = new GnssPositionInfo();

            position.TalkerID = sentence.TalkerID;

            position.TimeOfReading = NmeaUtilities.TimeOfReading(sentence.DataElements[8].ToString(), sentence.DataElements[0].ToString());
            //Debug.WriteLine($"Time of Reading:{position.TimeOfReading}UTC");

            if (sentence.DataElements[1].ToString().ToLower() == "a") {
                position.Valid = true;
            } else {
                position.Valid = false;
            }
            //Debug.WriteLine($"valid:{position.Valid}");

            //if (position.Valid) {
                //Debug.WriteLine($"will attempt to parse latitude; element[2]:{sentence.DataElements[2]}, element[3]:{sentence.DataElements[3]}");
                position.Position.Latitude = NmeaUtilities.DegreesMinutesDecode(sentence.DataElements[2].ToString(), sentence.DataElements[3].ToString());
                //Debug.WriteLine($"will attempt to parse longitude; element[4]:{sentence.DataElements[4]}, element[5]:{sentence.DataElements[5]}");
                position.Position.Longitude = NmeaUtilities.DegreesMinutesDecode(sentence.DataElements[4].ToString(), sentence.DataElements[5].ToString());
                //Debug.WriteLine("40");

                double speedInKnots;
                if(double.TryParse(sentence.DataElements[6].ToString(), out speedInKnots)) {
                    position.SpeedInKnots = speedInKnots;
                }
                double courseHeading;
                if (double.TryParse(sentence.DataElements[7].ToString(), out courseHeading)) {
                    position.CourseHeading = courseHeading;
                }

                if (sentence.DataElements[10].ToString().ToLower() == "e") {
                    position.MagneticVariation = CardinalDirection.East;
                } else if (sentence.DataElements[10].ToString().ToLower() == "w") {
                    position.MagneticVariation = CardinalDirection.West;
                } else {
                    position.MagneticVariation = CardinalDirection.Unknown;
                }
            //}
            //Debug.WriteLine($"RMC Message Parsed, raising event");
            PositionCourseAndTimeReceived(this, position);
        }

    }
}