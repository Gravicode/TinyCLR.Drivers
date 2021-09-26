//using Meadow.Peripherals.Displays;
using Meadow.TinyCLR.Core.Interface;

namespace Meadow.Foundation.Displays.TextDisplayMenu
{
    public interface IMenuInputItem : IPage
    {
        void Init(ITextDisplay display);

        void GetInput(string itemID, object currentValue);

        event ValueChangedHandler ValueChanged;
    }
}