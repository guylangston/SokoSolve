using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using ConsoleZ.Drawing;
using ConsoleZ.Drawing.Game;
using VectorInt;

namespace ConsoleZ.Samples
{
    public class SampleScene : GameScene<IRenderingGameLoop<ConsolePixel>, ConsolePixel>
    {
        Random        random   = new Random();
        List<Element> elements = new List<Element>();
        
        class Element
        {
            public Vector2   Position { get; set; }
            public Vector2     Speed    { get; set; }
            public ConsolePixel Pixel    { get; set; }
            public SampleScene Parent { get; set; }

            public void Step(in float elapsedSec)
            {
                Position += Speed * elapsedSec;
                if (Position.X < 0)
                {
                    Position = new Vector2(0, Position.Y);
                    Speed = new Vector2(Speed.X * -1, Speed.Y);
                }
                if (Position.X > Parent.Renderer.Width)
                {
                    Position = new Vector2(Parent.Renderer.Width, Position.Y);
                    Speed    = new Vector2(Speed.X * -1, Speed.Y);
                }
                if (Position.Y < 0)
                {
                    Position = new Vector2(Position.X, 0);
                    Speed    = new Vector2(Speed.X, Speed.Y * -1);
                }
                if (Position.Y > Parent.Renderer.Height)
                {
                    Position = new Vector2(Position.X, Parent.Renderer.Height);
                    Speed    = new Vector2(Speed.X, Speed.Y * -1);
                }
                
            }
        }
        
        public SampleScene(IRenderingGameLoop<ConsolePixel> parent) : base(parent)
        {
        }
        
        public override void Init()
        {
            elements.Clear();
            foreach (var i in Enumerable.Range(0, 50))
            {
                elements.Add(new Element()
                {
                    Parent = this,
                    Position = RandomInside(Renderer.Geometry),
                    Speed = new Vector2(random.Next(-100, 100) / 100f, random.Next(-100, 100) / 100f),
                    Pixel = new ConsolePixel((char)('A' + i), RandomColour(), RandomColour())
                });
            }
        }

        private Color RandomColour() => Color.FromArgb(
            255,
              0, //random.Next(0, 255),
            random.Next(0, 255), 
             0 //random.Next(0, 255)
            );
        
        private VectorInt2 RandomInside(RectInt rect) => new VectorInt2(random.Next(0, rect.W), random.Next(0, rect.H));

        public override void Step(float elapsedSec)
        {
            foreach (var element in elements)
            {
                element.Step(elapsedSec);
            }
        }

        public override void Draw()
        {
            foreach (var element in elements)
            {
                Renderer[element.Position] = element.Pixel;
            }
        }

        public override void Dispose()
        {
            
        }
    }
}