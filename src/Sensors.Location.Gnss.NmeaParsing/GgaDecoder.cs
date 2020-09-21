using Meadow.Peripherals.Sensors.Location.Gnss;
using Meadow.TinyCLR.Core;
using System;

namespace Meadow.TinyCLR.Sensors.Location.Gnss.NmeaParsing
{
    public delegate void GnssPositionInfoHandler(object sender, GnssPositionInfo e);
    // TODO: Should this be a struct with fields?
    /// <summary>
    /// Decoder for GGA messages.
    /// </summary>
    public class GgaDecoder : INmeaDecoder
    {
        /// <summary>
        /// Position update received event.
        /// </summary>
        public event GnssPositionInfoHandler PositionReceived = delegate { };

        /// <summary>
        /// Prefix for the GGA decoder.
        /// </summary>
        public string Prefix {
            get { return "GGA"; }
        }

        /// <summary>
        /// Friendly name for the GGA messages.
        /// </summary>
        public string Name {
            get { return "Global Postioning System Fix Data"; }
        }

        /// <summary>
        /// Process the data from a GGA message.
        /// </summary>
        /// <param name="data">String array of the message components for a CGA message.</param>
        public void Process(NmeaSentence sentence)
        {
            // make sure all fields are present
            for (var index = 0; index <= 7; index++) {
                if (string.IsNullOrEmpty(sentence.DataElements[index].ToString())) {
                    //Debug.WriteLine("Not all elements present");
                    // TODO: should we throw an exception and have callers wrap in a try/catch?
                    // problem today is that it just quietly returns
                    return;
                }
            }

            var location = new GnssPositionInfo();
            location.TalkerID = sentence.TalkerID;
            location.TimeOfReading = NmeaUtilities.TimeOfReading(null, sentence.DataElements[0].ToString());
            location.Position.Latitude = NmeaUtilities.DegreesMinutesDecode(sentence.DataElements[1].ToString(), sentence.DataElements[2].ToString());
            location.Position.Longitude = NmeaUtilities.DegreesMinutesDecode(sentence.DataElements[3].ToString(), sentence.DataElements[4].ToString());
            location.FixQuality = (FixType)Converters.Integer(sentence.DataElements[5].ToString());

            int numberOfSatellites;
            if (int.TryParse(sentence.DataElements[6].ToString(), out numberOfSatellites)) {
                location.NumberOfSatellites = numberOfSatellites;
            }
            double horizontalDilutionOfPrecision;
            if (double.TryParse(sentence.DataElements[7].ToString(), out horizontalDilutionOfPrecision)) {
                location.HorizontalDilutionOfPrecision = horizontalDilutionOfPrecision;
            }
            double altitude;
            if (double.TryParse(sentence.DataElements[8].ToString(), out altitude)) {
                location.Position.Altitude = altitude;
            }
            PositionReceived(this, location);
        }
    }
}