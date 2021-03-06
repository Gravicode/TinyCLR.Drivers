﻿using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Pwm;
using Meadow.TinyCLR.Interface;
using System;

namespace Meadow.TinyCLR.ICs.IOExpanders
{
    public class PwmPortPCA9685 : IPwmPort
    {
        readonly byte _address;
        readonly I2cDevice _i2cBus;
        readonly byte _portNumber;
        readonly byte _led0OnL;

        float _dutyCycle;
        float _frequency;

        //public IPwmChannelInfo Channel => throw new NotImplementedException();

        public float Duration { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float Period { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// Gets the overall PWM Frequency set for the PCA9685. Can't be changed per port.
        /// </summary>
        public float Frequency { get => _frequency; set { } }
        public TimeScale TimeScale { get; set; }
        public bool State => throw new NotImplementedException();
        public IPwmChannelInfo Channel => throw new NotImplementedException();
        public int Pin => throw new NotImplementedException();

        public float DutyCycle { 
            get => _dutyCycle;
            set 
            {
                _dutyCycle = value;
                Start();
            } 
        }
        
        public bool Inverted { get; set; }

        //public TimeScale TimeScale { get; set; }

       //IDigitalChannelInfo IPort<IDigitalChannelInfo>.Channel => throw new NotImplementedException();

        public PwmPortPCA9685(I2cDevice i2cBus, byte address, byte led0OnL, float frequency, byte portNumber, float dutyCycle)
        {
            _i2cBus = i2cBus;
            _address = address;
            _dutyCycle = dutyCycle;
            _portNumber = portNumber;
            _frequency = frequency;
            _led0OnL = led0OnL;
        }

        public void Dispose()
        {
            
        }

        public void Start()
        {
            if (Inverted)
            {
                SetPwm(_portNumber, (int)(_dutyCycle * 4096), 0);
            }
            else
            {
                SetPwm(_portNumber, 0, (int)(_dutyCycle * 4096));
            }
        }

        public void Stop()
        {
            SetPwm(_portNumber, 0, 0);
        }

        private void SetPwm(byte port, int on, int off)
        {
            Write((byte)(_led0OnL + (4 * port)), (byte)(on & 0xFF), (byte)(on >> 8), (byte)(off & 0xFF), (byte)(off >> 8));
        }

        private void Write(byte register, byte ledXOnL, byte ledXOnH, byte ledXOffL, byte ledXOffH)
        {
            _i2cBus.Write(new byte[] { _address, register, ledXOnL, ledXOnH, ledXOffL, ledXOffH });
        }

    }
}
