using Meadow.TinyCLR.Sensors.Location.MediaTek;

namespace Meadow.TinyCLR.FeatherWings
{
    public class GPSWing : Mt3339
    {
        public GPSWing(string serialMessagePort)
            : base(serialMessagePort)
        {
        }
    }
}