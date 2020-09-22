using GHIElectronics.TinyCLR.Devices.I2c;
using Meadow.Peripherals.Sensors.Motion;
using System;
using System.Threading;

namespace Meadow.Foundation.Sensors.Motion
{
    public class Mpu6050 : 
        IAccelerometer, IDisposable
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x68,
            Address1 = 0x69,
            Default = Address0
        }

        private enum Register : byte
        {
            Config = 0x1a,
            GyroConfig = 0x1b,
            AccelConfig = 0x1c,
            InterruptConfig = 0x37,
            InterruptEnable = 0x38,
            InterruptStatus = 0x3a,
            PowerManagement = 0x6b,
            AccelerometerX = 0x3b,
            AccelerometerY = 0x3d,
            AccelerometerZ = 0x3f,
            Temperature = 0x41,
            GyroX = 0x43,
            GyroY = 0x45,
            GyroZ = 0x47
        }

        public event AccelerationConditionChangeResultHandler Updated;

        /// <summary>
        ///     Acceleration along the X-axis.
        /// </summary>
        /// <remarks>
        ///     This property will only contain valid data after a call to Read or after
        ///     an interrupt has been generated.
        /// </remarks>
        public float AccelerationX {
            get {
                if (IsSampling) { return Conditions.XAcceleration; } else { return ReadRegisterInt16(Register.AccelerometerX) * (1 << AccelerometerScale) / AccelScaleBase; }
            }
        }

        /// <summary>
        ///     Acceleration along the Y-axis.
        /// </summary>
        /// <remarks>
        ///     This property will only contain valid data after a call to Read or after
        ///     an interrupt has been generated.
        /// </remarks>
        public float AccelerationY {
            get {
                if (IsSampling) { return Conditions.YAcceleration; } else { return ReadRegisterInt16(Register.AccelerometerY) * (1 << AccelerometerScale) / AccelScaleBase; }
            }
        }

        /// <summary>
        ///     Acceleration along the Z-axis.
        /// </summary>
        /// <remarks>
        ///     This property will only contain valid data after a call to Read or after
        ///     an interrupt has been generated.
        /// </remarks>
        public float AccelerationZ {
            get {
                if (IsSampling) { return Conditions.ZAcceleration; } else { return ReadRegisterInt16(Register.AccelerometerZ) * (1 << AccelerometerScale) / AccelScaleBase; }
            }
        }

        public AccelerationConditions Conditions { get; protected set; } = new AccelerationConditions();

        /// <summary>
        /// Gets a value indicating whether the analog input port is currently
        /// sampling the ADC. Call StartSampling() to spin up the sampling process.
        /// </summary>
        /// <value><c>true</c> if sampling; otherwise, <c>false</c>.</value>
        public bool IsSampling { get; protected set; } = false;

        private const float GyroScaleBase = 131f;
        private const float AccelScaleBase = 16384f;

        // internal thread lock
        private object _lock = new object();
        //private CancellationTokenSource SamplingTokenSource;

        private float _temp;

        private float _lastTemp;

        private int GyroScale { get; set; }
        private int AccelerometerScale { get; set; }
        private I2cDevice Device { get; set; }

        public float GyroChangeThreshold { get; set; }
        public float AccelerationChangeThreshold { get; set; }
        public byte Address { get; private set; }

        public Mpu6050(string I2cbus, byte address = 0x68)
        {
            if (I2cbus == null) throw new ArgumentNullException(nameof(I2cbus));
            var settings = new I2cConnectionSettings(address, 100_000); //The slave's address and the bus speed.
            var controller = I2cController.FromName(I2cbus);
            Device = controller.GetDevice(settings);
            //Device = bus;

            Initialize(address);
        }

        public Mpu6050(string I2cBus, Addresses address)
            : this(I2cBus, (byte)address)
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) {
                StopUpdating();
            }
        }

        /// <summary>
        /// Dispose managed resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        ///// <summary>
        ///// Starts continuously sampling the sensor.
        /////
        ///// This method also starts raising `Changed` events and IObservable
        ///// subscribers getting notified.
        ///// </summary>
        public void StartUpdating(int standbyDuration = 1000)
        {
            // thread safety
            lock (_lock) {
                if (IsSampling) { return; }

                // state muh-cheen
                IsSampling = true;

                //SamplingTokenSource = new CancellationTokenSource();
                //CancellationToken ct = SamplingTokenSource.Token;

                AccelerationConditions oldConditions;
                AccelerationConditionChangeResult result;
               var task1 = new Thread(new ThreadStart ( () => {
                    while (true) {
                        if (!IsSampling) {
                            // do task clean up here
                            //_observers.ForEach(x => x.OnCompleted());
                            break;
                        }
                        // capture history
                        oldConditions = AccelerationConditions.From(Conditions);

                        // read
                        Update();

                        // build a new result with the old and new conditions
                        result = new AccelerationConditionChangeResult(oldConditions, Conditions);

                        // let everyone know
                        RaiseChangedAndNotify(result);

                        // sleep for the appropriate interval
                        Thread.Sleep(standbyDuration);
                    }
                }));
                task1.Start();
            }
        }

        protected void RaiseChangedAndNotify(AccelerationConditionChangeResult changeResult)
        {
            Updated?.Invoke(this, changeResult);
            //base.NotifyObservers(changeResult);
        }

        ///// <summary>
        ///// Stops sampling the temperature.
        ///// </summary>
        public void StopUpdating()
        {
            lock (_lock) {
                //if (!IsSampling) { return; }

                //SamplingTokenSource?.Cancel();

                // state muh-cheen
                IsSampling = false;
            }
        }

        private void Initialize(byte address)
        {
            switch (address) {
                case 0x68:
                case 0x69:
                    // valid;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("MPU6050 device address must be either 0x68 or 0x69");
            }

            Address = address;

            Wake();
        }

        /// <summary>
        /// Gyroscope X measurement, in degrees per second
        /// </summary>
        public float XGyroscopicAcceleration {
            get {
                if (IsSampling) {
                    return Conditions.XGyroscopicAcceleration;
                }
                return ReadRegisterInt16(Register.GyroX) * (1 << GyroScale) / GyroScaleBase;
            }
        }

        /// <summary>
        /// Gyroscope Y measurement, in degrees per second
        /// </summary>
        public float YGyroscopicAcceleration {
            get {
                if (IsSampling) {
                    return Conditions.YGyroscopicAcceleration;
                }
                return ReadRegisterInt16(Register.GyroY) * (1 << GyroScale) / GyroScaleBase;
            }
        }

        /// <summary>
        /// Gyroscope Z measurement, in degrees per second
        /// </summary>
        public float ZGyroscopicAcceleration {
            get {
                if (IsSampling) {
                    return Conditions.ZGyroscopicAcceleration;
                }
                return ReadRegisterInt16(Register.GyroZ) * (1 << GyroScale) / GyroScaleBase;
            }
        }

        /// <summary>
        /// Temperature of sensor
        /// </summary>
        public float TemperatureC {
            get {
                if (IsSampling) {
                    return _temp;
                }
                return ReadRegisterInt16(Register.Temperature) * (1 << GyroScale) / GyroScaleBase;
            }
        }

        public void Wake()
        {
            Device.Write(new byte[] { Address, (byte)Register.PowerManagement, 0x00 });

            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            // read all 3 config bytes
            var data = new byte[3];
            Device.WriteRead(new byte[] { Address , (byte)Register.Config }, data);
    
            //var data = Device.WriteReadData(Address, 3, (byte)Register.Config);

            GyroScale = (data[1] & 0b00011000) >> 3;
            AccelerometerScale = (data[2] & 0b00011000) >> 3;
        }

        private short ReadRegisterInt16(Register register)
        {
            return ReadRegisterInt16((byte)register);
        }

        private short ReadRegisterInt16(byte register)
        {
            var data = new byte[2];
            Device.WriteRead(new byte[] { Address, register }, data);
            //var data = Device.WriteReadData(Address, 2, register);
            unchecked {
                return (short)(data[0] << 8 | data[1]); ;
            }
        }

        private void Update()
        {
            lock (_lock) {
                // we'll just read 14 bytes (7 registers), starting at 0x3b
                var data = new byte[14];
                Device.WriteRead(new byte[] { Address, (byte)Register.AccelerometerX }, data);
                //var data = Device.WriteReadData(Address, 14, (byte)Register.AccelerometerX);

                var a_scale = (1 << AccelerometerScale) / AccelScaleBase;
                var g_scale = (1 << GyroScale) / GyroScaleBase;
                Conditions.XAcceleration = ScaleAndOffset(data, 0, a_scale);
                Conditions.YAcceleration = ScaleAndOffset(data, 2, a_scale);
                Conditions.ZAcceleration = ScaleAndOffset(data, 4, a_scale);
                _temp = ScaleAndOffset(data, 6, 1 / 340f, 36.53f);
                Conditions.XGyroscopicAcceleration = ScaleAndOffset(data, 8, g_scale);
                Conditions.YGyroscopicAcceleration = ScaleAndOffset(data, 10, g_scale);
                Conditions.ZGyroscopicAcceleration = ScaleAndOffset(data, 12, g_scale);
            }
        }

        private float ScaleAndOffset(byte[] data, int index, float scale, float offset = 0)
        {
            // convert to a signed number
            unchecked {
                var s = (short)(data[index] << 8 | data[index + 1]);
                return (s * scale) + offset;
            }
        }
    }
}
