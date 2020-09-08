using GHIElectronics.TinyCLR.Devices.Adc;
using System;
using System.Threading;
using Meadow.TinyCLR.Core;

namespace Meadow.TinyCLR.Modules.Hid
{
    public delegate void JoystickPositionChangedHandler(object sender, JoystickPosition e);
    /// <summary>
    /// 2-axis analog joystick
    /// </summary>
    public class AnalogJoystick
        
    {
        AnalogSamplingSetting samplingSettingHorizontal;
        public bool IsSamplingHorizontal { get; set; }
        AnalogSamplingSetting samplingSettingVertical;
        public bool IsSamplingVertical { get; set; }
        /// <summary>
        /// Raised when the value of the reading changes.
        /// </summary>
        public event JoystickPositionChangedHandler Updated = delegate { };

        #region Properties

        public AdcChannel HorizontalInputPort { get; protected set; }

        public AdcChannel VerticalInputPort { get; protected set; }

        public DigitalJoystickPosition Position { get; }

        public JoystickCalibration Calibration { get; protected set; }

        public bool IsInverted { get; protected set; }

        public float HorizontalValue { get; protected set; }

        public float VerticalValue { get; protected set; }

        #endregion

        #region Enums

        public enum DigitalJoystickPosition
        {
            Center,
            Up,
            Down,
            Left,
            Right,
            UpRight,
            UpLeft,
            DownRight,
            DownLeft,
        }

        #endregion Enums

        #region Member variables / fields

        #endregion Member variables / fields

        #region Constructors

        public AnalogJoystick(string AdcControllerName, int horizontalPin, int verticalPin, JoystickCalibration calibration = null, bool isInverted = false) 
           
        {
            // this(device.CreateAnalogInputPort(horizontalPin), device.CreateAnalogInputPort(verticalPin), calibration, isInverted)
            var adc = AdcController.FromName(AdcControllerName);
            var horizonAnalog = adc.OpenChannel(horizontalPin);
            var verticalAnalog = adc.OpenChannel(verticalPin);

            SetAnalogPort(horizonAnalog,verticalAnalog,calibration,isInverted);
        }
        void SetAnalogPort(AdcChannel horizontalInputPort, AdcChannel verticalInputPort,
            JoystickCalibration calibration = null, bool isInverted = false)
        {
            HorizontalInputPort = horizontalInputPort;
            VerticalInputPort = verticalInputPort;
            IsInverted = isInverted;

            if (calibration == null)
            {
                Calibration = new JoystickCalibration(3.3f);
            }
            else
            {
                Calibration = calibration;
            }

            InitSubscriptions();
        }
        public AnalogJoystick(AdcChannel horizontalInputPort, AdcChannel verticalInputPort,
            JoystickCalibration calibration = null, bool isInverted = false)
        {
            SetAnalogPort( horizontalInputPort,  verticalInputPort,
             calibration ,  isInverted );
        }

        void InitSubscriptions()
        {
            /*
            HorizontalInputPort.Subscribe
            (
                new FilterableChangeObserver<FloatChangeResult, float>(
                    h => {
                        HorizontalValue = h.New;
                        if ((Math.Abs(h.Old - Calibration.HorizontalCenter) < Calibration.DeadZone) &&
                            (Math.Abs(h.New - Calibration.HorizontalCenter) < Calibration.DeadZone))
                        {
                            return;
                        }

                        var oldH = GetNormalizedPosition(h.Old, true);
                        var newH = GetNormalizedPosition(h.New, true);
                        var v = GetNormalizedPosition(VerticalValue, false);

                        RaiseEventsAndNotify
                        (
                            new JoystickPositionChangeResult(
                                new JoystickPosition(newH, v),
                                new JoystickPosition(oldH, v)
                            )
                        );
                    }
                )
            );

            VerticalInputPort.Subscribe
            (
                new FilterableChangeObserver<FloatChangeResult, float>(
                    v => {
                        VerticalValue = v.New;
                        if ((Math.Abs(v.Old - Calibration.VerticalCenter) < Calibration.DeadZone) &&
                            (Math.Abs(v.New - Calibration.VerticalCenter) < Calibration.DeadZone))
                        {
                            return;
                        }

                        var oldV = GetNormalizedPosition(v.Old, false);
                        var newV = GetNormalizedPosition(v.New, false);
                        var h = GetNormalizedPosition(HorizontalValue, true);

                        RaiseEventsAndNotify
                        (
                            new JoystickPositionChangeResult(
                                new JoystickPosition(h, newV),
                                new JoystickPosition(h, oldV)
                            )
                        );
                    }
                )
           );*/
        }

        #endregion Constructors

        #region Methods

        //call to set the joystick center position
        public void SetCenterPosition()
        {
            var hCenter = HorizontalInputPort.ReadValue();
            var vCenter = VerticalInputPort.ReadValue();

            Calibration = new JoystickCalibration(
                hCenter, Calibration.HorizontalMin, Calibration.HorizontalMax,
                vCenter, Calibration.VerticalMin, Calibration.VerticalMax,
                Calibration.DeadZone);
        }

        public void SetRange(int duration)
        {

            var timeoutTask = DateTime.Now.AddMilliseconds(duration);
            
            float h, v;
            var tick = DateTime.Now;
            while (tick<timeoutTask)
            {
                h = HorizontalInputPort.ReadValue();
                v = VerticalInputPort.ReadValue();
                Thread.Sleep(20);
                tick = DateTime.Now;
            }
        }

        // helper method to check joystick position
        public bool IsJoystickInPosition(DigitalJoystickPosition position)
        {
            if (position == Position)
                return true;

            return false;
        }

        public DigitalJoystickPosition GetPosition()
        {
            var h = GetHorizontalValue();
            var v = GetVerticalValue();

            if (h > Calibration.HorizontalCenter + Calibration.DeadZone)
            {
                if (v > Calibration.VerticalCenter + Calibration.DeadZone)
                {
                    return IsInverted ? DigitalJoystickPosition.DownLeft : DigitalJoystickPosition.UpRight;
                }
                if (v < Calibration.VerticalCenter - Calibration.DeadZone)
                {
                    return IsInverted ? DigitalJoystickPosition.UpLeft : DigitalJoystickPosition.DownRight;
                }
                return IsInverted ? DigitalJoystickPosition.Left : DigitalJoystickPosition.Right;
            }
            else if (h < Calibration.HorizontalCenter - Calibration.DeadZone)
            {
                if (v > Calibration.VerticalCenter + Calibration.DeadZone)
                {
                    return IsInverted ? DigitalJoystickPosition.DownRight : DigitalJoystickPosition.UpLeft;
                }
                if (v < Calibration.VerticalCenter - Calibration.DeadZone)
                {
                    return IsInverted ? DigitalJoystickPosition.UpRight : DigitalJoystickPosition.DownLeft;
                }
                return IsInverted ? DigitalJoystickPosition.Right : DigitalJoystickPosition.Left;
            }
            else if (v > Calibration.VerticalCenter + Calibration.DeadZone)
            {
                return IsInverted ? DigitalJoystickPosition.Down : DigitalJoystickPosition.Up;
            }
            else if (v < Calibration.VerticalCenter - Calibration.DeadZone)
            {
                return IsInverted ? DigitalJoystickPosition.Up : DigitalJoystickPosition.Down;
            }

            return DigitalJoystickPosition.Center;
        }

        public float GetHorizontalValue()
        {
            return HorizontalInputPort.ReadValue();
        }

        public float GetVerticalValue()
        {
            return VerticalInputPort.ReadValue();
        }

        public void StartUpdating(int sampleCount = 3,
            int sampleIntervalDuration = 40,
            int standbyDuration = 100)
        {
            //HorizontalInputPort.StartSampling(sampleCount, sampleIntervalDuration, standbyDuration);
            samplingSettingHorizontal.sampleCount = sampleCount;
            samplingSettingHorizontal.sampleIntervalDuration = sampleIntervalDuration;
            samplingSettingHorizontal.standbyDuration = standbyDuration;
            IsSamplingHorizontal = true;

            var task = new Thread(new ThreadStart(StartSamplingHorizontal));
            task.Start();


            //VerticalInputPort.StartSampling(sampleCount, sampleIntervalDuration, standbyDuration);
            samplingSettingVertical.sampleCount = sampleCount;
            samplingSettingVertical.sampleIntervalDuration = sampleIntervalDuration;
            samplingSettingVertical.standbyDuration = standbyDuration;
            IsSamplingVertical = true;

            var task2 = new Thread(new ThreadStart(StartSamplingVertical));
            task2.Start();
        }
        void StartSamplingHorizontal()
        {
            int sampleCount = samplingSettingHorizontal.sampleCount;
            int sampleIntervalDuration = samplingSettingHorizontal.sampleIntervalDuration;
            int standbyDuration = samplingSettingHorizontal.standbyDuration;
            var counter = 0;
            var total = 0f;
            var average = 0f;
            for (int i = 0; i < sampleCount; i++)
            {
                // read the voltage
                var temp = HorizontalInputPort.ReadValue();

                
                /*
                if ((Math.Abs(h.Old - Calibration.HorizontalCenter) < Calibration.DeadZone) &&
                    (Math.Abs(h.New - Calibration.HorizontalCenter) < Calibration.DeadZone))
                {
                    return;
                }
                */
                total += temp;
                counter++;
                average = total / counter;
                if (!IsSamplingHorizontal) break;
                Thread.Sleep(sampleIntervalDuration);
            }
            IsSamplingHorizontal = false;
            HorizontalValue = average;
            var newH = GetNormalizedPosition(HorizontalValue, true);
            var v = GetNormalizedPosition(VerticalValue, false);

            RaiseEventsAndNotify
            (

                    new JoystickPosition(newH, v)

            );
        }
        void StartSamplingVertical()
        {
            int sampleCount = samplingSettingVertical.sampleCount;
            int sampleIntervalDuration = samplingSettingVertical.sampleIntervalDuration;
            int standbyDuration = samplingSettingVertical.standbyDuration;
            var counter = 0;
            var total = 0f;
            var average = 0f;
            for (int i = 0; i < sampleCount; i++)
            {
                // read the voltage
                var temp = VerticalInputPort.ReadValue();


                /*
                if ((Math.Abs(h.Old - Calibration.VerticalCenter) < Calibration.DeadZone) &&
                    (Math.Abs(h.New - Calibration.VerticalCenter) < Calibration.DeadZone))
                {
                    return;
                }
                */
                total += temp;
                counter++;
                average = total / counter;
                if (!IsSamplingVertical) break;
                Thread.Sleep(sampleIntervalDuration);
            }
            IsSamplingVertical = false;
            VerticalValue = average;
            var newV = GetNormalizedPosition(VerticalValue, true);
            var h = GetNormalizedPosition(HorizontalValue, false);

            RaiseEventsAndNotify
            (

                    new JoystickPosition(h,newV)

            );
        }
        /// <summary>
        /// Stops sampling the temperature.
        /// </summary>
        public void StopUpdatingHorizontal()
        {
            IsSamplingHorizontal = false;
        }
        public void StopUpdatingVertical()
        {
            IsSamplingVertical = false;
        }

        protected void RaiseEventsAndNotify(JoystickPosition changeResult)
        {
            Updated?.Invoke(this, changeResult);
            //base.NotifyObservers(changeResult);
        }

        float GetNormalizedPosition(float value, bool isHorizontal)
        {
            float normalized;

            if (isHorizontal)
            {
                if (value <= Calibration.HorizontalCenter)
                {
                    normalized = (value - Calibration.HorizontalCenter) / (Calibration.HorizontalCenter - Calibration.HorizontalMin);
                }
                else
                {
                    normalized = (value - Calibration.HorizontalCenter) / (Calibration.HorizontalMax - Calibration.HorizontalCenter);
                }
            }
            else
            {
                if (value <= Calibration.VerticalCenter)
                {
                    normalized = (value - Calibration.VerticalCenter) / (Calibration.VerticalCenter - Calibration.VerticalMin);
                }
                else
                {
                    normalized = (value - Calibration.VerticalCenter) / (Calibration.VerticalMax - Calibration.VerticalCenter);
                }
            }

            return IsInverted ? -1 * normalized : normalized;
        }

        #endregion Methods

        #region Local classes

        /// <summary>
        ///     Calibration class for new sensor types.  This allows new sensors
        ///     to be used with this class.
        /// </summary>
        /// <remarks>
        ///     
        /// </remarks>
        public class JoystickCalibration
        {
            public float HorizontalCenter { get; protected set; }
            public float HorizontalMin { get; protected set; }
            public float HorizontalMax { get; protected set; }

            public float VerticalCenter { get; protected set; }
            public float VerticalMin { get; protected set; }
            public float VerticalMax { get; protected set; }

            public float DeadZone { get; protected set; }

            public JoystickCalibration(float analogVoltage)
            {
                HorizontalCenter = analogVoltage / 2;
                HorizontalMin = 0;
                HorizontalMax = analogVoltage;

                VerticalCenter = analogVoltage / 2;
                VerticalMin = 0;
                VerticalMax = analogVoltage;

                DeadZone = 0.2f;
            }

            public JoystickCalibration(float horizontalCenter, float horizontalMin, float horizontalMax,
                float verticalCenter, float verticalMin, float verticalMax, float deadZone)
            {
                HorizontalCenter = horizontalCenter;
                HorizontalMin = horizontalMin;
                HorizontalMax = horizontalMax;

                VerticalCenter = verticalCenter;
                VerticalMin = verticalMin;
                VerticalMax = verticalMax;

                DeadZone = deadZone;
            }
        }

        #endregion Local classes
    }
}
