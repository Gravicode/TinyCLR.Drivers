using System;
using Meadow.Peripherals.Sensors.Location.Gnss;

namespace Meadow.TinyCLR.Sensors.Location.Gnss.NmeaParsing
{
    public delegate void ActiveSatellitesHandler(object sender, ActiveSatellites e);
    public class GsaDecoder : INmeaDecoder
    {
        // Note this is commented out so we don't pay the (trivial) price of the if() check. :)
        //protected bool DebugMode { get; set; } = false;

        /// <summary>
        /// Event raised when valid GSA data is received.
        /// </summary>
        public event ActiveSatellitesHandler ActiveSatellitesReceived = delegate { };

        /// <summary>
        /// Prefix for the GSA decoder.
        /// </summary>
        public string Prefix {
            get => "GSA";
        }

        /// <summary>
        /// Friendly name for the GSA messages.
        /// </summary>
        public string Name {
            get => "GSA - DOP and number of active satellites.";
        }

        /// <summary>
        /// Process the data from a GSA message.
        /// </summary>
        /// <param name="data">String array of the message components for a GSA message.</param>
        public void Process(NmeaSentence sentence)
        {
            //if (DebugMode) { Debug.WriteLine($"GSADecoder.Process"); }

            var satellites = new ActiveSatellites();

            satellites.TalkerID = sentence.TalkerID;

            switch (sentence.DataElements[0].ToString().ToLower()) {
                case "a":
                    satellites.SatelliteSelection = ActiveSatelliteSelection.Automatic;
                    break;
                case "m":
                    satellites.SatelliteSelection = ActiveSatelliteSelection.Manual;
                    break;
                default:
                    satellites.SatelliteSelection = ActiveSatelliteSelection.Unknown;
                    break;
            }

            //if (DebugMode) { Debug.WriteLine($"satellite seletion:{satellites.SatelliteSelection}"); };

            int dimensionalFixType;
            if (int.TryParse(sentence.DataElements[1].ToString(), out dimensionalFixType)) {
                satellites.Dimensions = (DimensionalFixType)dimensionalFixType;
            }
            //if (DebugMode) { Debug.WriteLine($"dimensional fix type:{satellites.Dimensions}"); };

            var satelliteCount = 0;
            for (var index = 2; index < 14; index++) {
                if (!string.IsNullOrEmpty(sentence.DataElements[index].ToString())) {
                    satelliteCount++;
                }
            }
            if (satelliteCount > 0) {
                satellites.SatellitesUsedForFix = new string[satelliteCount];
                var currentSatellite = 0;
                for (var index = 2; index < 14; index++) {
                    if (!string.IsNullOrEmpty(sentence.DataElements[index].ToString())) {
                        satellites.SatellitesUsedForFix[currentSatellite] = sentence.DataElements[index].ToString();
                        currentSatellite++;
                    }
                }
            } else {
                satellites.SatellitesUsedForFix = null;
            }
            double dilutionOfPrecision;
            if (double.TryParse(sentence.DataElements[14].ToString(), out dilutionOfPrecision)) {
                satellites.DilutionOfPrecision = dilutionOfPrecision;
            }
            double horizontalDilutionOfPrecision;
            if (double.TryParse(sentence.DataElements[15].ToString(), out horizontalDilutionOfPrecision)) {
                satellites.HorizontalDilutionOfPrecision = horizontalDilutionOfPrecision;
            }
            double verticalDilutionOfPrecision;
            if (double.TryParse(sentence.DataElements[16].ToString(), out verticalDilutionOfPrecision)) {
                satellites.VerticalDilutionOfPrecision = verticalDilutionOfPrecision;
            }
            //Debug.WriteLine($"GSADecoder.Process complete; satelliteCount:{satelliteCount}");
            ActiveSatellitesReceived(this, satellites);
        }
    }
}