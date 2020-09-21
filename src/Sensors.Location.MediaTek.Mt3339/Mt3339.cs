using Meadow.TinyCLR.Sensors.Location.Gnss.NmeaParsing;
using Meadow.Peripherals.Sensors.Location.Gnss;
using System;
using System.Text;
using System.Diagnostics;
using GHIElectronics.TinyCLR.Devices.Uart;
using System.Text.RegularExpressions;

namespace Meadow.TinyCLR.Sensors.Location.MediaTek
{
    public class NmeaEventArgs
    {
        public string NmeaSentence { get; set; }
    }

    public class Mt3339
    {
        //public int BaudRate {
        //    get => serialPort.BaudRate;
        //    set => serialPort.BaudRate = value;
        //}

        UartController serialPort;
        NmeaSentenceProcessor nmeaProcessor;

        //public event EventHandler<NmeaEventArgs> NmeaSentenceArrived = delegate { };

        public event GnssPositionInfoHandler GgaReceived = delegate { };
        public event GnssPositionInfoHandler GllReceived = delegate { };
        public event ActiveSatellitesHandler GsaReceived = delegate { };
        public event GnssPositionInfoHandler RmcReceived = delegate { };
        public event CourseOverGroundHandler VtgReceived = delegate { };
        public event SatellitesInViewHandler GsvReceived = delegate { };
        byte[] rxBuffer = new byte[512];
        string ReadBuffer = string.Empty;

        // TODO: if we want to make this public then we're going to have to add
        // a bunch of checks around baud rate, 8n1, etc.
        protected Mt3339(string serialPortControllerName)
        {
            serialPort = UartController.FromName(serialPortControllerName);

            var uartSetting = new UartSetting()
            {
                BaudRate = 115200,
                DataBits = 8,
                Parity = UartParity.None,
                StopBits = UartStopBitCount.One,
                Handshaking = UartHandshake.None,
            };

            serialPort.SetActiveSettings(uartSetting);
            
            serialPort.Enable();

            serialPort.DataReceived += SerialPort_DataReceived;

            //this.serialPort = serialPort;

            //this.serialPort.MessageReceived += SerialPort_MessageReceived;

            Init();
        }

        private void SerialPort_DataReceived(UartController sender, DataReceivedEventArgs e)
        {
        
            var bytesReceived = serialPort.Read(rxBuffer, 0, e.Count);
            var msg = Encoding.UTF8.GetString(rxBuffer, 0, bytesReceived);
            if (msg.IndexOf("\r\n") > -1)
            {
                var msgs = Regex.Split(msg, "\r\n", RegexOptions.IgnoreCase);
                ReadBuffer += msgs[0];
                nmeaProcessor?.ProcessNmeaMessage(ReadBuffer.Trim());
                ReadBuffer = msgs[1];
            }
            else
            {
                ReadBuffer += msg;
            }
            Debug.WriteLine($"Message arrived:{msg}");

           
        }

        protected void Init()
        {
            serialPort.DataReceived += SerialPort_DataReceived;
            InitDecoders();
            Debug.WriteLine("Finish Mt3339 initialization.");
        }

        public void StartUpdataing()
        {
            // open the serial connection
            serialPort.Enable();
            Debug.WriteLine("serial port opened.");

            //==== setup commands

            // get release and version
            Debug.WriteLine("Asking for release and version.");
            this.serialPort.Write(Encoding.UTF8.GetBytes(Commands.PMTK_Q_RELEASE));

            // get atntenna info
            Debug.WriteLine("Start output antenna info");
            this.serialPort.Write(Encoding.UTF8.GetBytes(Commands.PGCMD_ANTENNA));

            // turn on all data
            Debug.WriteLine("Turning on all data");
            this.serialPort.Write(Encoding.UTF8.GetBytes(Commands.PMTK_SET_NMEA_OUTPUT_ALLDATA));
        }

        protected void InitDecoders()
        {
            Debug.WriteLine("Create NMEA");
            nmeaProcessor = new NmeaSentenceProcessor();

            Debug.WriteLine("Add decoders");

            // MTK
            var mtkDecoder = new MtkDecoder();
            Debug.WriteLine("Created MTK");
            nmeaProcessor.RegisterDecoder(mtkDecoder);
            mtkDecoder.MessageReceived += (object sender, string message) => {
                Debug.WriteLine($"MTK Message:{message}");
            };

            // GGA
            var ggaDecoder = new GgaDecoder();
            Debug.WriteLine("Created GGA");
            nmeaProcessor.RegisterDecoder(ggaDecoder);
            ggaDecoder.PositionReceived += (object sender, GnssPositionInfo location) => {
                this.GgaReceived(this, location);
            };

            // GLL
            var gllDecoder = new GllDecoder();
            nmeaProcessor.RegisterDecoder(gllDecoder);
            gllDecoder.GeographicLatitudeLongitudeReceived += (object sender, GnssPositionInfo location) => {
                this.GllReceived(this, location);
            };

            // GSA
            var gsaDecoder = new GsaDecoder();
            nmeaProcessor.RegisterDecoder(gsaDecoder);
            gsaDecoder.ActiveSatellitesReceived += (object sender, ActiveSatellites activeSatellites) => {
                this.GsaReceived(this, activeSatellites);
            };

            // RMC (recommended minimum)
            var rmcDecoder = new RmcDecoder();
            nmeaProcessor.RegisterDecoder(rmcDecoder);
            rmcDecoder.PositionCourseAndTimeReceived += (object sender, GnssPositionInfo positionCourseAndTime) => {
                this.RmcReceived(this, positionCourseAndTime);
            };

            // VTG (course made good)
            var vtgDecoder = new VtgDecoder();
            nmeaProcessor.RegisterDecoder(vtgDecoder);
            vtgDecoder.CourseAndVelocityReceived += (object sender, CourseOverGround courseAndVelocity) => {
                this.VtgReceived(this, courseAndVelocity);
            };

            // GSV (satellites in view)
            var gsvDecoder = new GsvDecoder();
            nmeaProcessor.RegisterDecoder(gsvDecoder);
            gsvDecoder.SatellitesInViewReceived += (object sender, SatellitesInView satellites) => {
                this.GsvReceived(this, satellites);
            };

        }

        //private void SerialPort_MessageReceived(object sender, SerialMessageData e)
        //{
            
        //}
    }
}