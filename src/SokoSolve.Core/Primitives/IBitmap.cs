using System;
using VectorInt;
using VectorInt.Collections;

namespace SokoSolve.Core.Primitives
{
    public interface IReadOnlyBitmap :  IReadOnlyCartesianMap<bool>, IEquatable<IBitmap>, IComparable<IBitmap>
    {
        int Count { get; }
        int SizeInBytes();
    }
    
    public interface IBitmap : IReadOnlyBitmap
    {
        new bool this[VectorInt2 pos] { get; set; }
        new bool this[int        pX, int pY] { get; set; }
    }
    
    
}