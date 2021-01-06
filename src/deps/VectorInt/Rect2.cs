using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace VectorInt
{
   public interface IRect2 : IEnumerable<Vector2>
    {
        float X { get; set; }
        float Y { get; set; }
        float W { get; set; }
        float H { get; set; }
        
        float X2 { get; }
        float Y2 { get; }
        float XC { get; }
        float YC { get; }

        // Top
        Vector2 TL { get; }
        Vector2 TM { get; }
        Vector2 TR { get; }
        
        // Middle
        Vector2 ML { get; }
        Vector2 C { get; }
        Vector2 MR { get; }
        
        // Bottom
        Vector2 BL { get; }
        Vector2 BM { get; }
        Vector2 BR { get; }
        
        // Logic
        bool Contains(Vector2 p);
    }

    public class Rect2 : IRect2
    {
        public Rect2(float x, float y, float w, float h)
        {
            X = x;
            Y = y;
            W = w;
            H = h;
        }
        
        public Rect2(Vector2 pos, Vector2 size)
        {
            X = pos.X;
            Y = pos.Y;
            W = size.X;
            H = size.Y;
        }

        public Rect2(Vector2 size)
        {
            X = 0;
            Y = 0;
            W = size.X;
            H = size.Y;
        }

        public static Rect2 FromTwoPoints(Vector2 a, Vector2 b)
        {
            var x = Math.Min(a.X, b.X);
            var y = Math.Min(a.Y, b.Y);
            var x2 = Math.Max(a.X, b.X);
            var y2 = Math.Max(a.Y, b.Y);
            
            return new Rect2(x, y, x2-x, y2 - y);
            
        }
        

        public float X { get; set; }
        public float Y { get; set; }
        public float W { get; set; }
        public float H { get; set; }

        public float X2 => X + W - 1;
        public float Y2 => Y + H - 1;
        
        public float XC => X + (W - 1) / 2;
        public float YC => Y + (H - 1) / 2;
        
        public Vector2 Size => new Vector2(W, H);
        
        public Vector2 TL => new Vector2(X,  Y);
        public Vector2 TM => new Vector2(XC, Y);
        public Vector2 TR => new Vector2(X2, Y);
        
        public Vector2 ML => new Vector2(X,  YC);
        public Vector2 C  => new Vector2(XC, YC);
        public Vector2 MR => new Vector2(X2, YC);
        
        public Vector2 BL => new Vector2(X,  Y2);
        public Vector2 BM => new Vector2(XC, Y2);
        public Vector2 BR => new Vector2(X2, Y2);


        public override string ToString() => $"({X}, {Y}, {X2}, {Y2} [W:{W}, H:{H}])";


        public bool Contains(Vector2 p) =>
            p.X >= X && p.X <= X2 &&
            p.Y >= Y && p.Y <= Y2;


        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<Vector2> GetEnumerator()
        {
            for (var xx = X; xx < X + W; xx++)
            for (var yy = Y; yy < Y + H; yy++)
                yield return new Vector2(xx, yy);
        }

        public static Rect2 CenterAt(Vector2 at, Vector2 size)
        {
            var half = new Vector2(size.X / 2, size.Y / 2);
            return new Rect2(at.X - half.X, at.Y - half.Y, size.X, size.Y);
        }
       

        public IEnumerable<(Vector2 inner, Vector2 outer)> InnerVsOuter()
        {
            for (var xx = X; xx < X + W; xx++)
            for (var yy = Y; yy < Y + H; yy++)
                yield return (new Vector2(xx-X, yy-Y), new Vector2(xx, yy) );
        }

        public IRectInt Move(Vector2 offset) => new RectInt(TL + offset, Size);
    }
}