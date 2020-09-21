using System;
using Meadow.TinyCLR.Sensors.Location.Gnss.NmeaParsing;
using Meadow.Peripherals.Sensors.Location.Gnss;
using System.Diagnostics;

namespace Meadow.TinyCLR.Sensors.Location.MediaTek
{
    public delegate void MessageReceivedHandler(object sender, string e);
    public class MtkDecoder : INmeaDecoder
    {
        public event MessageReceivedHandler MessageReceived = delegate { };

        /// <summary>
        /// Friendly name for the MTK messages.
        /// </summary>
        public string Name {
            get => "MediaTek";
        }

        /// <summary>
        /// Prefix for the GLL (Geographic position Latitude / Longitude) decoder.
        /// </summary>
        public string Prefix {
            get => "MTK";
        }


        /// <summary>
        /// Process the data from a RMC
        /// </summary>
        /// <param name="data">String array of the message components for a RMC message.</param>
        public void Process(NmeaSentence sentence)
        {
            // get the packet type (command number)
            var packetType = sentence.DataElements[0];
            Debug.WriteLine($"Packet Type:{packetType}, {Lookups.KnownPacketTypes[packetType]}");

            for (int i = 0; i < sentence.DataElements.Count; i++) {
                Debug.WriteLine($"index [{i}], value{sentence.DataElements[i]}");
            }
            

        }
    }
}
