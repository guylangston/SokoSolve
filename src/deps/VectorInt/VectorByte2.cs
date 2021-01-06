using System;
using System.Numerics;

namespace VectorInt
{
    public struct VectorByte2 : IVector2<byte>,IEquatable<VectorByte2>
    {

        public VectorByte2(VectorInt2 v) : this()
        {
            X = (byte)v.X;
            Y = (byte)v.Y;
        }


        public VectorByte2(VectorByte2 v) : this()
        {
            X = v.X;
            Y = v.Y;
        }

        public VectorByte2(byte x, byte y) : this()
        {
            X = x;
            Y = y;
        }
        
        public VectorByte2(byte scalar) : this()
        {
            X = scalar;
            Y = scalar;
        }

        public void Deconstruct(out byte x, out byte y)
        {
            x = X;
            y = Y;
        }


        public byte X { get; set; }
        public byte Y { get; set; }
        
        public bool IsZero => X == 0 && Y == 0;

        public static readonly VectorByte2 Zero     = new VectorByte2(0);
        public static readonly VectorByte2 Unit     = new VectorByte2(1);
        public static readonly VectorByte2 MinValue = new VectorByte2(byte.MinValue);
        public static readonly VectorByte2 MaxValue = new VectorByte2(byte.MaxValue);

        public static VectorByte2 operator +(VectorByte2 lhs, VectorByte2 rhs) => new VectorByte2((byte)(lhs.X + rhs.X), (byte)(lhs.Y + rhs.Y));
        public static VectorByte2 operator -(VectorByte2 lhs, VectorByte2 rhs) => new VectorByte2((byte)(lhs.X - rhs.X), (byte)(lhs.Y - rhs.Y)    );
        public static VectorByte2 operator *(VectorByte2 lhs, VectorByte2 rhs) => new VectorByte2((byte)(lhs.X * rhs.X), (byte)(lhs.Y * rhs.Y));
        public static VectorByte2 operator /(VectorByte2 lhs, VectorByte2 rhs) => new VectorByte2((byte)(lhs.X / rhs.X), (byte)(lhs.Y / rhs.Y));
        public static bool operator ==(VectorByte2 lhs, VectorByte2 rhs) => lhs.Equals(rhs);
        public static bool operator !=(VectorByte2 lhs, VectorByte2 rhs) => !lhs.Equals(rhs);
        
        public static implicit operator VectorByte2((byte x, byte y) tuple) => new VectorByte2(tuple.x, tuple.y);
        public static implicit operator VectorInt2(VectorByte2       v)     => new VectorInt2(v.X, v.Y);
        public static implicit operator VectorByte2(Vector2          v)     => new VectorByte2((byte)v.X, (byte)v.Y);
        
        public bool Equals(VectorByte2 other) => X == other.X && Y == other.Y;
        public bool Equals(IVector2<byte> other) => X == other.X && Y == other.Y;

        public override bool Equals(object obj) => Equals((VectorByte2) obj);

        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();

        public override string ToString() => $"({X},{Y})";
    }
}