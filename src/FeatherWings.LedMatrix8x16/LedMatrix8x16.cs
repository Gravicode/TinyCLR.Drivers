﻿using Meadow.TinyCLR.Core;
using Meadow.TinyCLR.Displays;
using Meadow.TinyCLR.ICs.IOExpanders;
using System;

namespace Meadow.TinyCLR.FeatherWings
{
    /// <summary>
    /// Represents an Adafruit Led Matrix 8x16 feather wing (HT16K33)
    /// </summary>
    public class LedMatrix8x16Wing : DisplayBase
    {
        private Ht16K33 ht16k33;
        private Color pen;

        public LedMatrix8x16Wing(string i2cBus, byte address = 0x70)
        {
            ht16k33 = new Ht16K33(i2cBus, address);
        }

        public override DisplayColorMode ColorMode => DisplayColorMode.Format1bpp;

        public override uint Width => 8;

        public override uint Height => 16;

        public override void Clear(bool updateDisplay = false)
        {
            ht16k33.ClearDisplay();
        }

        public override void DrawPixel(int x, int y, Color color)
        {
            DrawPixel(x, y, color == Color.Black ? false : true);
        }

        public override void DrawPixel(int x, int y, bool colored)
        {
           if(x >= Width || x < 0 || y >= Height || y < 0)
            {
                return; 
            }

            if (y < 8)
            {
                y *= 2;
            }
            else
            {
                y = (y - 8) * 2 + 1;
            }
            ht16k33.ToggleLed((byte)(y * Width + x), colored);
        }

        public override void DrawPixel(int x, int y)
        {
            DrawPixel(x, y, pen == Color.Black ? false : true);
        }

        public override void SetPenColor(Color pen)
        {
            this.pen = pen;
        }

        public override void Show()
        {
            ht16k33.UpdateDisplay();
        }
        public override void InvertPixel(int x, int y)
        {
            if (y < 8)
            {
                y *= 2;
            }
            else
            {
                y = (y - 8) * 2 + 1;
            }

            ht16k33.ToggleLed((byte)(y * Width + x));
        }

    }
}