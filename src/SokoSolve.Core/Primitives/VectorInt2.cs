using System;
using System.Collections;

namespace Sokoban.Core.Primitives
{

    public class RandomFloat
    {
        Random random = new Random();

        public float Next(float from, float too)
        {
            return (float)random.Next((int) from, (int) too) + (float)random.NextDouble();
        }

    }

    public struct VectorInt2 : IEquatable<VectorInt2>
    {
        public VectorInt2(int x, int y) : this()
        {
            X = x;
            Y = y;
        }


        public int X { get; set; }
        public int Y { get; set; }

        public bool IsUnit
        {
            get
            {
                return this == VectorInt2.Up || this == VectorInt2.Down
                       || this == VectorInt2.Left || this == VectorInt2.Right;
            }
        }

        public static readonly VectorInt2 Zero = new VectorInt2();
        
        public static readonly VectorInt2 MinValue = new VectorInt2(int.MinValue, int.MinValue);
        public static readonly VectorInt2 Up = new VectorInt2(0, -1);
        public static readonly VectorInt2 Down = new VectorInt2(0, 1);
        public static readonly VectorInt2 Left = new VectorInt2(-1, 0);
        public static readonly VectorInt2 Right = new VectorInt2(1, 0);
        public static readonly VectorInt2[] Directions = { Up, Down, Left, Right };


        public static VectorInt2 operator +(VectorInt2 lhs, VectorInt2 rhs) { return new VectorInt2(lhs.X + rhs.X, lhs.Y + rhs.Y); }
        public static VectorInt2 operator -(VectorInt2 lhs, VectorInt2 rhs) { return new VectorInt2(lhs.X - rhs.X, lhs.Y - rhs.Y); }
        public static VectorInt2 operator *(VectorInt2 lhs, VectorInt2 rhs) { return new VectorInt2(lhs.X * rhs.X, lhs.Y * rhs.Y); }
        public static VectorInt2 operator /(VectorInt2 lhs, VectorInt2 rhs) { return new VectorInt2(lhs.X / rhs.X, lhs.Y / rhs.Y); }
        public bool Equals(VectorInt2 other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return Equals((VectorInt2)obj);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public static bool operator ==(VectorInt2 lhs, VectorInt2 rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(VectorInt2 lhs, VectorInt2 rhs)
        {
            return !lhs.Equals(rhs);
        }

        public override string ToString()
        {
            return string.Format("({0},{1})", X, Y);
        }

        
    }

    public struct RectInt2
    {
        public VectorInt2 TopLeft { get; set; }
        public VectorInt2 Size { get; set; }

        public VectorInt2 BottomRight
        {
            get { return TopLeft + Size; }
        }

        public bool Contains(VectorInt2 p)
        {
            if (p.X < TopLeft.X) return false;
            if (p.X > BottomRight.X) return false;
            if (p.Y < TopLeft.Y) return false;
            if (p.Y > BottomRight.Y) return false;
            return true;
        }

        public override string ToString()
        {
            return string.Format("TopLeft: {0}, Size: {1}", TopLeft, Size);
        }
    }
}