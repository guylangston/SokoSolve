using System;
using ConsoleZ.Win32;

namespace ConsoleZ.Drawing.Game
{
    public abstract class GameScene<TParent, TPixel> : IRenderingGameLoop<TPixel>, IDisposable where TParent:IRenderingGameLoop<TPixel>
    {
        protected GameScene(TParent parent)
        {
            Parent = parent;
        }

        protected TParent           Parent            { get; }
        public    bool              IsActive          => Parent.IsActive;
        public    int               FrameCount        => Parent.FrameCount;
        public    float             Elapsed           => Parent.Elapsed;
        public    float             FrameIntervalGoal => Parent.FrameIntervalGoal;
        public    float             FramesPerSecond   => Parent.FramesPerSecond;
        public    IInputProvider    Input             => Parent.Input;
        public    IRenderer<TPixel> Renderer          => Parent.Renderer;

        public abstract void Init();
        public virtual void Reset() { }
        public abstract void Step(float elapsedSec);
        public abstract void Draw();
        public abstract void Dispose();
    }
    
    
}