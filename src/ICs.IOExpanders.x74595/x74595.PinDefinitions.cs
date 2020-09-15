//using System.Collections.Generic;
//using Meadow.Hardware;

using System.Collections;

namespace Meadow.TinyCLR.ICs.IOExpanders
{
	public partial class x74595
	{
		public class PinDefinitions //: intDefinitions
		{
            public IList AllPins { get; } = new ArrayList();

            /// <summary>
            /// GP0
            /// </summary>
            public int GP0 { get; } = 0x00;

            /// <summary>
            /// GP1
            /// </summary>
            public int GP1 { get; } = 0x01;

            /// <summary>
            /// GP2
            /// </summary>
            public int GP2 { get; } = 0x02;

            /// <summary>
            /// GP3
            /// </summary>
            public int GP3 { get; } =  0x03;

            /// <summary>
            /// GP4
            /// </summary>
            public int GP4 { get; } =  0x04;

            /// <summary>
            /// GP5
            /// </summary>
            public int GP5 { get; } = 0x05;

            /// <summary>
            /// GP6
            /// </summary>
            public int GP6 { get; } = 0x06;

            /// <summary>
            /// GP7
            /// </summary>
            public int GP7 { get; } =  0x07;

            /*
            private static int GetPin(string name, byte key)
            {
                return new Pin(
                    name, key,
                    new List<IChannelInfo> {
                        new DigitalChannelInfo(
                            name: name,
                            inputCapable: false,
                            outputCapable: true,
                            pullDownCapable: false,
                            pullUpCapable: false)
                    });
            }*/
            /*
              /// <summary>
            /// GP0
            /// </summary>
            public int GP0 { get; } = GetPin("GP0", 0x00);

            /// <summary>
            /// GP1
            /// </summary>
            public int GP1 { get; } = GetPin("GP1", 0x01);

            /// <summary>
            /// GP2
            /// </summary>
            public int GP2 { get; } = GetPin("GP2", 0x02);

            /// <summary>
            /// GP3
            /// </summary>
            public int GP3 { get; } = GetPin("GP3", 0x03);

            /// <summary>
            /// GP4
            /// </summary>
            public int GP4 { get; } = GetPin("GP4", 0x04);

            /// <summary>
            /// GP5
            /// </summary>
            public int GP5 { get; } = GetPin("GP5", 0x05);

            /// <summary>
            /// GP6
            /// </summary>
            public int GP6 { get; } = GetPin("GP6", 0x06);

            /// <summary>
            /// GP7
            /// </summary>
            public int GP7 { get; } = GetPin("GP7", 0x07);

            private static int GetPin(string name, byte key)
            {
                return new Pin(
                    name, key,
                    new List<IChannelInfo> {
                        new DigitalChannelInfo(
                            name: name,
                            inputCapable: false,
                            outputCapable: true,
                            pullDownCapable: false,
                            pullUpCapable: false)
                    });
            }
             */
            public PinDefinitions()
            {
                InitAllPins();
            }

            protected void InitAllPins()
            {
                // add all our pins to the collection
                AllPins.Add(GP0);
                AllPins.Add(GP1);
                AllPins.Add(GP2);
                AllPins.Add(GP3);
                AllPins.Add(GP4);
                AllPins.Add(GP5);
                AllPins.Add(GP6);
                AllPins.Add(GP7);
            }
		}
	}
}