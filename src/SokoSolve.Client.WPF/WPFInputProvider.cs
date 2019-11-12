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
                
                Key.A => ConsoleKey.A,
                Key.B => ConsoleKey.B,
                Key.C => ConsoleKey.C,
                Key.D => ConsoleKey.D,
                Key.E => ConsoleKey.E,
                Key.F => ConsoleKey.F,
                Key.G => ConsoleKey.G,
                Key.H => ConsoleKey.H,
                Key.I => ConsoleKey.I,
                Key.J => ConsoleKey.J,
                Key.K => ConsoleKey.K,
                Key.L => ConsoleKey.L,
                Key.M => ConsoleKey.M,
                Key.N => ConsoleKey.N,
                Key.O => ConsoleKey.O,
                Key.P => ConsoleKey.P,
                Key.Q => ConsoleKey.Q,
                Key.R => ConsoleKey.R,
                Key.S => ConsoleKey.S,
                Key.T => ConsoleKey.T,
                Key.U => ConsoleKey.U,
                Key.V => ConsoleKey.V,
                Key.W => ConsoleKey.W,
                Key.X => ConsoleKey.X,
                Key.Y => ConsoleKey.Y,
                Key.Z => ConsoleKey.Z,

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