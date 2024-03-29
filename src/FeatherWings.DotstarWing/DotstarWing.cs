﻿using GHIElectronics.TinyCLR.Devices.Gpio;
using Meadow.TinyCLR.Core;
using Meadow.TinyCLR.Displays;
using Meadow.TinyCLR.Leds;
using System;
using static Meadow.TinyCLR.Leds.Apa102;

namespace Meadow.TinyCLR.FeatherWings
{
    /// <summary>
    /// Represents Adafruit's Dotstar feather wing 12x6
    /// </summary>
    public class DotstarWing : DisplayBase
    {
        Color penColor;
        Apa102 ledMatrix;

        public float Brightness
        {
            get => ledMatrix.Brightness;
            set => ledMatrix.Brightness = value;  
        }

        public DotstarWing(string spiBus, GpioPin chipSelect) : this(spiBus,chipSelect,72)
        {
        }

        public DotstarWing(string spiBus, GpioPin chipSelect, uint numberOfLeds, PixelOrder pixelOrder = PixelOrder.BGR, bool autoWrite = false)
        {
            penColor = Color.White;
            ledMatrix = new Apa102(spiBus, chipSelect, numberOfLeds, pixelOrder, autoWrite);
        }

        public override DisplayColorMode ColorMode => DisplayColorMode.Format12bppRgb444;

        public override uint Width => 12;

        public override uint Height => 6;

        public override void Clear(bool updateDisplay = false)
        {
            ledMatrix.Clear(updateDisplay);
        }

        public override void DrawPixel(int x, int y, Color color)
        {
            uint minor = (uint)x;
            uint major = (uint)y;
            uint majorScale;

            major = Height - 1 - major;
            majorScale = Width;

            uint pixelOffset = (major * majorScale) + minor;

            if (pixelOffset >= 0 && pixelOffset < Height * Width)
            {
                ledMatrix.SetLed(pixelOffset, color);
            }
        } 

        public override void DrawPixel(int x, int y, bool colored)
        {
            if (colored)
            {
                DrawPixel(x, y, penColor);
            }
            else
            {
                DrawPixel(x, y, Color.Black);
            }
        }

        public override void DrawPixel(int x, int y)
        {
            DrawPixel(x, y, penColor);
        }

        public override void SetPenColor(Color pen)
        {
            penColor = pen;
        }

        public override void Show()
        {
            ledMatrix.Show();
        }
        public override void InvertPixel(int x, int y)
        {
            throw new NotImplementedException();
        }
    }
}