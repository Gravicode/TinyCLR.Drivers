

using Meadow.TinyCLR.Core.Interface;
using Meadow.TinyCLR.Interface;

namespace Meadow.TinyCLR.Displays.TextDisplayMenu
{
    public interface IMenuInputItem
    {
        void Init(ITextDisplay display, IRotaryEncoder encoder, IButton buttonSelect);
        void Init(ITextDisplay display, IButton buttonNext, IButton buttonPrevious, IButton buttonSelect);
        void GetInput(string itemID, object currentValue);
        event ValueChangedHandler ValueChanged;
    }
}