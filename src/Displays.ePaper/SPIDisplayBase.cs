using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
using Meadow;
using Meadow.TinyCLR.Core;
//using Meadow.Hardware;
using System.Threading;

namespace Meadow.TinyCLR.Displays
{
    public abstract class SpiDisplayBase : DisplayBase
    {
        protected readonly byte[] spiBOneByteBuffer = new byte[1];

        protected GpioPin dataCommandPort; //out
        protected GpioPin resetPort; //out
        protected GpioPin busyPort; //in
        protected SpiDevice spi;

        protected Color currentPen = Color.White;

        protected const bool Data = true;
        protected const bool Command = false;
        
        protected void Write(byte value)
        {
            spiBOneByteBuffer[0] = value;
            spi.Write(spiBOneByteBuffer);
        }

        public override void SetPenColor(Color pen)
        {
            currentPen = pen;
        }

        protected void Reset()
        {
            resetPort.Write(GpioPinValue.Low);// = (false);
            DelayMs(200);
            resetPort.Write(GpioPinValue.High);// = (true);
            DelayMs(200);
        }

        protected void DelayMs(int millseconds)
        {
            Thread.Sleep(millseconds);
        }

        protected void SendCommand(byte command)
        {
            dataCommandPort.Write( Command ? GpioPinValue.High: GpioPinValue.Low);
            Write(command);
        }

        protected void SendData(int data)
        {
            SendData((byte)data);
        }

        protected void SendData(byte data)
        {
            dataCommandPort.Write(Data? GpioPinValue.High : GpioPinValue.Low);
            Write(data);
        }

        protected void SendData(byte[] data)
        {
            dataCommandPort.Write(Data ? GpioPinValue.High: GpioPinValue.Low);
            spi.Write(data);
        }

        protected virtual void WaitUntilIdle()
        {
            int count = 0;
            while (busyPort.Read() == GpioPinValue.Low && count < 20)
            {
                DelayMs(50);
                count++;
            }
        }
    }
}