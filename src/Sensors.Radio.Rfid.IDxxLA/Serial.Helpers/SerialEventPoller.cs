using System;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Uart;

namespace Meadow.TinyCLR.Sensors.Radio.Rfid.Serial.Helpers
{
    /// <summary>
    /// Helper class to fake events for a serial port by using polling behind the scenes.
    /// Useful until events are fully supported for <see cref="UartController" />.
    /// </summary>
    public class SerialEventPoller : IDisposable
    {
        public bool IsRunning { get; set; }
        //private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Event for if there is data in the serial port buffer to read.
        /// </summary>
        public event DataReceivedEventHandler DataReceived = delegate { };

        /// <summary>
        /// Creates a new event poller for the provided <see cref="UartController" />.
        /// Call <see cref="Start" /> to begin poling.
        /// </summary>
        /// <param name="serialPort">The serial port to poll.</param>
        public SerialEventPoller(UartController serialPort)
        {
            SerialPort = serialPort;
        }

        /// <summary>
        /// The currently used <see cref="UartController" />.
        /// </summary>
        public UartController SerialPort { get; }

        public void Dispose()
        {
            Stop();
            //_cancellationTokenSource?.Dispose();
        }

        /// <summary>
        /// Start polling the <see cref="UartController" /> buffer.
        /// </summary>
        /// <param name="pollingIntervalMs">The interval between polling calls. Defaults to 100ms.</param>
        public void Start(int pollingIntervalMs = 100)
        {
            Stop();

            //_cancellationTokenSource = new CancellationTokenSource();
            //var token = _cancellationTokenSource.Token;
            IsRunning = true;
            var task1 =new Thread(new ThreadStart (
            () => PollForData(SerialPort, pollingIntervalMs)));
            task1.Start();
        }

        /// <summary>
        /// Stop polling the <see cref="UartController" /> buffer.
        /// </summary>
        public void Stop()
        {
            IsRunning = false;
            //_cancellationTokenSource?.Cancel();
        }

        private void PollForData(UartController port, int intervalMs)
        {
            while (true)
            {
                if (port.BytesToRead > 0)
                {
                    //var handler = Volatile.Read(ref DataReceived);
                    DataReceived?.Invoke(this, new PolledSerialDataReceivedEventArgs { SerialPort = port });
                }

                Thread.Sleep(intervalMs);
                if (!IsRunning)
                {
                    break;
                }
            }
        }
    }

    public class PolledSerialDataReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// The serial port with data in it's buffer.
        /// You should check there is still data in the buffer before consuming.
        /// </summary>
        public UartController SerialPort { get; set; }
    }

    public delegate void DataReceivedEventHandler(object sender, PolledSerialDataReceivedEventArgs e);
}
