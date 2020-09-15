using GHIElectronics.TinyCLR.Devices.I2c;
using System;


namespace Meadow.TinyCLR.ICs.IOExpanders
{
    public partial class Mcp23x08
    {
        internal interface IMcpDeviceComms
        {
            byte ReadRegister(byte address);
            byte[] ReadRegisters(byte address, ushort length);
            void WriteRegister(byte address, byte value);
            void WriteRegisters(byte address, byte[] data);
        }

        internal class I2cMcpDeviceComms : IMcpDeviceComms
        {
            I2cDevice device { get; set; }
            public I2cMcpDeviceComms(string bus, byte peripheralAddress)
               
            {
                var settings = new I2cConnectionSettings(peripheralAddress, 100_000); //The slave's address and the bus speed.
                var controller = I2cController.FromName(bus);
                device = controller.GetDevice(settings);
            }

            public byte ReadRegister(byte address)
            {
                var readBytes = new byte[1];
                device.WriteRead(new byte[] { address },readBytes);
                return readBytes[0];
            }

            public byte[] ReadRegisters(byte address, ushort length)
            {
                var readBytes = new byte[length];
                device.WriteRead(new byte[] { address },0,1,readBytes,0,length);
                return readBytes;
            }

            public void WriteRegister(byte address, byte value)
            {
                var writeBytes = new byte[] {address, value };
                device.Write(writeBytes);
                
            }

            public void WriteRegisters(byte address, byte[] data)
            {
                byte[] ret = new byte[1 + data.Length];
                Array.Copy(new byte[] { address }, 0, ret, 0, 1);
                Array.Copy(data, 0, ret, 1, data.Length);
                device.Write(ret);
            }
        }
    }
}
