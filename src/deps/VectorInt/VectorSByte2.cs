using System;
using System.Numerics;

namespace VectorInt
{
    public struct VectorSByte2 : IVector2<sbyte>, IEquatable<VectorSByte2>
    {

        public VectorSByte2(Vector2 v) : this()
        {
            X = (sbyte)v.X;
            Y = (sbyte)v.Y;
        }
        

        public VectorSByte2(VectorSByte2 v) : this()
        {
            X = v.X;
            Y = v.Y;
        }

        public VectorSByte2(sbyte x, sbyte y) : this()
        {
            X = x;
            Y = y;
        }
        
        public VectorSByte2(sbyte scalar) : this()
        {
            X = scalar;
            Y = scalar;
        }

        public void Deconstruct(out sbyte x, out sbyte y)
        {
            x = X;
            y = Y;
        }


        public sbyte X { get; set; }
        public sbyte Y { get; set; }

        public bool IsUnit => this == Up || this == Down || this == Left || this == Right;
        public bool IsZero => X == 0 && Y == 0;

        public static readonly VectorSByte2 Zero     = new VectorSByte2(0);
        public static readonly VectorSByte2 Unit     = new VectorSByte2(1);
        public static readonly VectorSByte2 MinValue = new VectorSByte2(sbyte.MinValue);
        public static readonly VectorSByte2 MaxValue = new VectorSByte2(sbyte.MaxValue);
        
        public static readonly VectorSByte2   Up         = new VectorSByte2(0, -1);
        public static readonly VectorSByte2   Down       = new VectorSByte2(0, 1);
        public static readonly VectorSByte2   Left       = new VectorSByte2(-1, 0);
        public static readonly VectorSByte2   Right      = new VectorSByte2(1, 0);
        public static readonly VectorSByte2[] Directions = {Up, Down, Left, Right};

        public static VectorSByte2 operator +(VectorSByte2 lhs, VectorSByte2 rhs) => new VectorSByte2((sbyte)(lhs.X + rhs.X), (sbyte)(lhs.Y + rhs.Y));
        public static VectorSByte2 operator -(VectorSByte2 lhs, VectorSByte2 rhs) => new VectorSByte2((sbyte)(lhs.X - rhs.X), (sbyte)(lhs.Y - rhs.Y));
        public static VectorSByte2 operator *(VectorSByte2 lhs, VectorSByte2 rhs) => new VectorSByte2((sbyte)(lhs.X * rhs.X), (sbyte)(lhs.Y * rhs.Y));
        public static VectorSByte2 operator /(VectorSByte2 lhs, VectorSByte2 rhs) => new VectorSByte2((sbyte)(lhs.X / rhs.X), (sbyte)(lhs.Y / rhs.Y));
        public static bool operator ==(VectorSByte2 lhs, VectorSByte2 rhs) => lhs.Equals(rhs);
        public static bool operator !=(VectorSByte2 lhs, VectorSByte2 rhs) => !lhs.Equals(rhs);
        
        public static implicit operator VectorSByte2((sbyte x, sbyte y) tuple) => new VectorSByte2(tuple.x, tuple.y);
        public static implicit operator Vector2(VectorSByte2            v) => new Vector2(v.X, v.Y);
        public static implicit operator VectorSByte2(Vector2            v) => new VectorSByte2((sbyte)v.X, (sbyte)v.Y);
        
        public bool Equals(VectorSByte2 other) => X == other.X && Y == other.Y;
        public bool Equals(IVector2<sbyte> other) => X == other.X && Y == other.Y;

        public override bool Equals(object obj) => Equals((VectorSByte2) obj);

        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();

        public override string ToString() => $"({X},{Y})";
    }
}