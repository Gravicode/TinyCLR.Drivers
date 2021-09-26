using System;
using System.Collections;
using System.Diagnostics;

namespace Meadow.Foundation.Displays.TextDisplayMenu
{
    public class MenuPage : IPage
    {
        public int ScrollPosition
        {
            get => scrollPosition; 
            set {
                if (value > MenuItems.Count - 1 || value < 0)
                {
                    Debug.WriteLine("Attempting to set a scroll position outside of item range: " + value.ToString());
                }
                scrollPosition = value;
            }
        }
        protected int scrollPosition = 0;

        //public List<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        public ArrayList MenuItems { get; set; } = new ArrayList();

        public bool Next()
        {
            // if outside of valid range return false
            if (scrollPosition >= MenuItems.Count - 1)
            {
                return false;
            }

            // increment scroll position
            scrollPosition++;

            return true;
        }

        public bool Previous()
        {
            // if outside of valid range return false
            if (scrollPosition <= 0) { return false; }

            // increment scroll position
            scrollPosition--;

            return true;
        }

        public bool Select()
        {   //gives us the ability to respond to select events
            return true;
        }
    }
}