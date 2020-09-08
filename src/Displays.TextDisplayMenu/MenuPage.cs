﻿using System;
using System.Collections;
using System.Diagnostics;

namespace Meadow.TinyCLR.Displays.TextDisplayMenu
{
    public class MenuPage
    {
        public int ScrollPosition
        {
            get { return _scrollPosition; }
            set {
                if (value > MenuItems.Count - 1 || value < 0)
                {
                    Debug.WriteLine("Attempting to set a scroll position outside of item range: " + value.ToString());
                }
                _scrollPosition = value;
            }
        }
        protected int _scrollPosition = 0;

        public ArrayList MenuItems
        { get; set; } = new ArrayList();
    }
}
