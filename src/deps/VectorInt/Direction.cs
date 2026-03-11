using System;
using System.Collections.Generic;
using System.IO;

namespace VectorInt;

public readonly struct Direction
{
    readonly byte dir;

    private Direction(byte dir)
    {
        this.dir = dir;
    }

    public Direction(char c)
    {
        this.dir = (byte)ToCharLookup.IndexOf(c);
    }

    public Direction(VectorInt2 d)
    {
        for(byte idx=0; idx<ToVectByIndex.Length; idx++)
        {
            if (ToVectByIndex[idx] == d)
            {
                dir = idx;
                return;
            }
        }
        throw new InvalidDataException(d.ToString());
    }

    public Direction RotLeft()  => ToRotLeft[dir];
    public Direction RotRight() => ToRotRight[dir];
    public Direction Reverse()  => ToReverse[dir];

    public static readonly Direction Left  = new Direction(0);
    public static readonly Direction Right = new Direction(1);
    public static readonly Direction Up    = new Direction(2);
    public static readonly Direction Down  = new Direction(3);

    public override string ToString() => ToStringLookup[dir];
    static readonly string[] ToStringLookup =
    [
        nameof(Left),
        nameof(Right),
        nameof(Up),
        nameof(Down),
    ];
    public char ToChar() => ToCharLookup[dir];
    static readonly char[] ToCharLookup =
    [
        'L',
        'R',
        'U',
        'D',
    ];

    static readonly VectorInt2[] ToVectByIndex =
        [
            new VectorInt2(-1, 0), // left
            new VectorInt2(+1, 0), // right
            new VectorInt2(0, -1), // up
            new VectorInt2(0, 1), // down
        ];

    static readonly Direction[] ToRotLeft =
        [
            Down, // From Left
            Up,   // From Right
            Left, // From Up
            Right // From Down
        ];

    static readonly Direction[] ToRotRight =
        [
            Up,     // From Left
            Down,   // From Right
            Right,  // From Up
            Left    // From Down
        ];

    static readonly Direction[] ToReverse =
        [
            Right, // From Left
            Left,  // From Right
            Down,  // From Up
            Up     // From Down
        ];

    public static VectorInt2 operator+ (VectorInt2 v, Direction d) => v + ToVectByIndex[d.dir];
    public static VectorInt2 operator- (VectorInt2 v, Direction d) => v - ToVectByIndex[d.dir];


    public static IEnumerable<Direction> Flatten(params IEnumerable<DirectionPath> paths)
    {
        foreach (var path in paths)
        {
            foreach (var dir in path)
            {
                yield return dir;
            }
        }
    }
}
