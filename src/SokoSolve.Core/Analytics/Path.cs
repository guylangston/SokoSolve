using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VectorInt;
using static VectorInt.VectorInt2;

namespace SokoSolve.Core.Analytics
{
    public class Path : List<VectorInt2>
    {
        public const char Up = 'U';
        public const char Down = 'D';
        public const char Left = 'L';
        public const char Right = 'R';

        public Path()
        {
        }

        public Path(IEnumerable<VectorInt2> collection) : base(collection.Where(x => x != Zero))
        {
        }

        public Path(IEnumerable<Path> paths)
            : this(Flatten(paths))
        {
        }

        public Path(string pathStr) : this(
            pathStr.ToUpperInvariant()
                .Where(x => "UDLR".Contains(x))
                .Select(x => ToVector(char.ToUpperInvariant(x))))
        {
        }

        public string? Description  { get; set; }
        public int     NodeDepth    => NodeDepthFwd + NodeDepthRev;
        public int     NodeDepthFwd { get; set; }
        public int     NodeDepthRev { get; set; }

        private static IEnumerable<VectorInt2> Flatten(IEnumerable<Path> paths)
        {
            foreach (var path in paths)
            foreach (var dir in path)
                yield return dir;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var cc = 0;
            foreach (var dir in this)
            {
                sb.Append(ToString(dir));
                if (cc++ > 80)
                {
                    sb.AppendLine();
                    cc = 0;
                }
            }

            return sb.ToString();
        }

        public string ToStringFull() => $"{Description}, Depth:{NodeDepthFwd}+{NodeDepthRev}={NodeDepth} => {ToString()}";

        public string ToStringSummary() => $"{Description}(Depth:{NodeDepthFwd}+{NodeDepthRev}={NodeDepth}, Steps:{Count})";

        public static string ToString(VectorInt2 x)
        {
            if (x == VectorInt2.Up)    return new string(new[] {Up});
            if (x == VectorInt2.Down)  return new string(new[] {Down});
            if (x == VectorInt2.Left)  return new string(new[] {Left});
            if (x == VectorInt2.Right) return new string(new[] {Right});
            return $"[{x}]";
        }

        public static VectorInt2 ToVector(char d)
        {
            if (d == Up) return VectorInt2.Up;
            if (d == Down) return VectorInt2.Down;
            if (d == Left) return VectorInt2.Left;
            if (d == Right) return VectorInt2.Right;

            throw new Exception("Unknown char:" + d);
        }
    }
}
