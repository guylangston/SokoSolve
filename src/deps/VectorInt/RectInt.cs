using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace VectorInt
{

    public interface IRectInt : IEnumerable<VectorInt2>
    {
        int X { get; set; }
        int Y { get; set; }
        int W { get; set; }
        int H { get; set; }
        
        int X2 { get; }
        int Y2 { get; }
        int XC { get; }
        int YC { get; }

        // Top
        VectorInt2 TL { get; }
        VectorInt2 TM { get; }
        VectorInt2 TR { get; }
        
        // Middle
        VectorInt2 ML { get; }
        VectorInt2 C { get; }
        VectorInt2 MR { get; }
        
        // Bottom
        VectorInt2 BL { get; }
        VectorInt2 BM { get; }
        VectorInt2 BR { get; }
        
        // Logic
        bool Contains(VectorInt2 p);
    }

    public class RectInt : IRectInt
    {
        public RectInt(int x, int y, int w, int h)
        {
            X = x;
            Y = y;
            W = w;
            H = h;
        }
        
        public RectInt(VectorInt2 pos, VectorInt2 size)
        {
            X = pos.X;
            Y = pos.Y;
            W = size.X;
            H = size.Y;
        }

        public RectInt(VectorInt2 size)
        {
            X = 0;
            Y = 0;
            W = size.X;
            H = size.Y;
        }

        public static RectInt FromTwoPoints(VectorInt2 a, VectorInt2 b)
        {
            var x = Math.Min(a.X, b.X);
            var y = Math.Min(a.Y, b.Y);
            var x2 = Math.Max(a.X, b.X);
            var y2 = Math.Max(a.Y, b.Y);
            
            return new RectInt(x, y, x2-x, y2 - y);
            
        }
        

        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }

        public int X2 => X + W - 1;
        public int Y2 => Y + H - 1;
        
        public int XC => X + (W - 1) / 2;
        public int YC => Y + (H - 1) / 2;
        
        public VectorInt2 Size => new VectorInt2(W, H);
        
        public VectorInt2 TL => (X,  Y);
        public VectorInt2 TM => (XC, Y);
        public VectorInt2 TR => (X2, Y);
        
        public VectorInt2 ML => (X,  YC);
        public VectorInt2 C  => (XC, YC);
        public VectorInt2 MR => (X2, YC);
        
        public VectorInt2 BL => (X,  Y2);
        public VectorInt2 BM => (XC, Y2);
        public VectorInt2 BR => (X2, Y2);


        public override string ToString() => $"({X}, {Y}, {X2}, {Y2} [W:{W}, H:{H}])";


        public bool Contains(VectorInt2 p) =>
            p.X >= X && p.X <= X2 &&
            p.Y >= Y && p.Y <= Y2;


        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<VectorInt2> GetEnumerator()
        {
            for (var xx = X; xx < X + W; xx++)
            for (var yy = Y; yy < Y + H; yy++)
                yield return new VectorInt2(xx, yy);
        }

        public static RectInt CenterAt(VectorInt2 at, RectInt rect)
        {
            var half = rect.Size / new VectorInt2(2, 2);
            return new RectInt(at.X - half.X, at.Y - half.Y, rect.W, rect.H);
        }
        
        public static RectInt CenterAt(VectorInt2 at, VectorInt2 size)
        {
            var half = new VectorInt2(size.X / 2, size.Y / 2);
            return new RectInt(at.X - half.X, at.Y - half.Y, size.X, size.Y);
        }

        public IEnumerable<(VectorInt2 inner, VectorInt2 outer)> InnerVsOuter()
        {
            for (var xx = X; xx < X + W; xx++)
            for (var yy = Y; yy < Y + H; yy++)
                yield return (new VectorInt2(xx-X, yy-Y), new VectorInt2(xx, yy) );
        }

        public IRectInt Move(VectorInt2 offset) => new RectInt(TL + offset, Size);
    }
}