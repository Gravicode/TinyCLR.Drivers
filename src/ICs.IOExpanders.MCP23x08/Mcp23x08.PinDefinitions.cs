using System;
using System.Collections;

namespace Meadow.TinyCLR.ICs.IOExpanders
{
    public partial class Mcp23x08
    {
        public class PinDefinitions //: intDefinitions
        {

            public IList AllPins { get; } = new ArrayList();

            /// <summary>
            /// GP0
            /// </summary>
            public readonly int GP0 = (byte)0x00;


            /// <summary>
            /// GP1
            /// </summary>
            public readonly int GP1 = (byte)0x01;

            /// <summary>
            /// GP2
            /// </summary>
            public readonly int GP2 = (byte)0x02;

            /// <summary>
            /// GP3
            /// </summary>
            public readonly int GP3 = (byte)0x03;

            /// <summary>
            /// GP4
            /// </summary>
            public readonly int GP4 = (byte)0x04;

            /// <summary>
            /// GP5
            /// </summary>
            public readonly int GP5 = (byte)0x05;

            /// <summary>
            /// GP6
            /// </summary>
            public readonly int GP6 = (byte)0x06;

            /// <summary>
            /// GP7
            /// </summary>
            public readonly int GP7 = (byte)0x07;
            /*
            /// <summary>
            /// GP0
            /// </summary>
            public readonly int GP0 = new Pin(
                "GP0", (byte)0x00,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GP0", pullDownCapable:false),
                }
            );

            /// <summary>
            /// GP1
            /// </summary>
            public readonly int GP1 = new Pin(
                "GP1", (byte)0x01,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GP1", pullDownCapable:false),
                }
            );

            /// <summary>
            /// GP2
            /// </summary>
            public readonly int GP2 = new Pin(
                "GP2", (byte)0x02,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GP2", pullDownCapable:false),
                }
            );

            /// <summary>
            /// GP3
            /// </summary>
            public readonly int GP3 = new Pin(
                "GP3", (byte)0x03,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GP3", pullDownCapable:false),
                }
            );

            /// <summary>
            /// GP4
            /// </summary>
            public readonly int GP4 = new Pin(
                "GP4", (byte)0x04,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GP4", pullDownCapable:false),
                }
            );

            /// <summary>
            /// GP5
            /// </summary>
            public readonly int GP5 = new Pin(
                "GP5", (byte)0x05,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GP5", pullDownCapable:false),
                }
            );

            /// <summary>
            /// GP6
            /// </summary>
            public readonly int GP6 = new Pin(
                "GP6", (byte)0x06,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GP6", pullDownCapable:false),
                }
            );

            /// <summary>
            /// GP7
            /// </summary>
            public readonly int GP7 = new Pin(
                "GP7", (byte)0x07,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GP7", pullDownCapable:false),
                }
            );
            */

            public PinDefinitions()
            {
                InitAllPins();
            }

            protected void InitAllPins()
            {
                // add all our pins to the collection
                AllPins.Add(this.GP0);
                AllPins.Add(this.GP1);
                AllPins.Add(this.GP2);
                AllPins.Add(this.GP3);
                AllPins.Add(this.GP4);
                AllPins.Add(this.GP5);
                AllPins.Add(this.GP6);
                AllPins.Add(this.GP7);
            }

        }
    }
}
