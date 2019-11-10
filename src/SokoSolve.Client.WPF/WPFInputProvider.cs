using System;
using System.Windows.Input;
using ConsoleZ.Win32;

namespace SokoSolve.Client.WPF
{
    public  class WPFInputProvider : InputProviderBase
    {
        public void CaptureKeyDown(Key key) => base.CaptureKeyDown(ToConsoleKey(key));
            
        private ConsoleKey ToConsoleKey(Key eKey)
        {
            return eKey switch
            {
                Key.D1 => ConsoleKey.D1,
                Key.D2 => ConsoleKey.D2,
                Key.D3 => ConsoleKey.D3,
                Key.D4 => ConsoleKey.D4,
                Key.D5 => ConsoleKey.D5,
                Key.D6 => ConsoleKey.D6,
                Key.D7 => ConsoleKey.D7,
                Key.D8 => ConsoleKey.D8,
                Key.D9 => ConsoleKey.D9,
                Key.D0 => ConsoleKey.D0,

                Key.Left  => ConsoleKey.LeftArrow,
                Key.Right => ConsoleKey.RightArrow,
                Key.Up    => ConsoleKey.UpArrow,
                Key.Down  => ConsoleKey.DownArrow,

                Key.Enter  => ConsoleKey.Enter,
                Key.Escape => ConsoleKey.Escape,

                _ => ConsoleKey.F24
            };
        }
    }
}