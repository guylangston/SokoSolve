using System;
using System.Threading;
using ConsoleZ.Win32;

namespace ConsoleZ.Drawing.Game
{
    public class ConsoleGameLoop<TPixel> : RenderingGameLoopBase<TPixel>
    {
        public ConsoleGameLoop(InputProvider inputProvider, IRenderer<TPixel> renderer) : base(inputProvider, renderer)
        {
            
        }

        public  IRenderingGameLoop<TPixel> Scene { get; set; }
       
        public virtual void Start()
        {
            //GameStarted = DateTime.Now;
            IsActive = true;
            while (IsActive)
            {
                var startFrame = DateTime.Now;
                Draw();
                var endFrame = DateTime.Now;
                var elapse = (float)(endFrame - startFrame).TotalSeconds;
                if (elapse < FrameIntervalGoal)
                {
                    Thread.Sleep((int)((FrameIntervalGoal - elapse)*1000f));
                    elapse = FrameIntervalGoal;
                }

                Step(elapse);
                FrameCount++;
            }
            // Dispose should be called next
        }
        
        public override void Init()
        {
            Scene?.Init();
        }

        public override void Step(float elapsedSec)
        {
            Scene?.Step(elapsedSec);
            
            Input.Step(elapsedSec); // this clears input; so do this last
        }

        public override void Draw()
        {
            Scene?.Draw();
        }

        public override void Dispose()
        {
            if (Scene != null && Scene is IDisposable sd) sd.Dispose();
            Input.Dispose();   
        }
    }
}