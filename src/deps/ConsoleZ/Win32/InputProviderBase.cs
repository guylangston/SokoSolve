using System;
using System.Linq;
using VectorInt;

namespace ConsoleZ.Win32
{
    public abstract class InputProviderBase : IInputProvider
    {
        private bool[] perFrameKeys;
        private bool[] perFrameMouseClick;

        protected InputProviderBase()
        {
            perFrameKeys       = new bool[256];
            perFrameMouseClick = new bool[3];
        }

        public virtual bool       IsMouseEnabled { get; set; }
        public         VectorInt2 MousePosition  { get; set; }
        public virtual bool       IsMouseClick   => perFrameMouseClick[0];
        
        public bool IsKeyDown(ConsoleKey key) => perFrameKeys[(byte) key];
        public bool IsKeyPressed()            => perFrameKeys.Any(x=>x);
        
        public bool IsKeyPressed(ConsoleKey key) => perFrameKeys[(byte) key];

        public void CaptureKeyDown(ConsoleKey key) => perFrameKeys[(byte) key] = true;
        public void CaptureMouseDown(int      key) => perFrameKeys[(byte) key] = true;
        
        public virtual void Step(float elapsed)
        {
            ArrayHelper.Fill(perFrameKeys, false);
            ArrayHelper.Fill(perFrameMouseClick, false);
        }

        public virtual void Dispose()
        {
            
        }
    }
}