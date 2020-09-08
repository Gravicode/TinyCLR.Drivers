using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace Meadow.TinyCLR.Core.Interface
{
    public class TextDisplayConfig
    {
        public ushort Height;
        public ushort Width;

        public TextDisplayConfig()
        {

        }
    }

    public interface ITextDisplay
    {
        TextDisplayConfig DisplayConfig { get; }

        void ClearLine(byte lineNumber);
        void ClearLines();
        void SaveCustomCharacter(byte[] characterMap, byte address);
        void SetCursorPosition(byte column, byte line);
        void Write(string text);
        void WriteLine(string text, byte lineNumber);
    }
}
