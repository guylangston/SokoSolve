using System;
using System.Collections;
using System.Collections.Generic;

namespace VectorInt.Collections
{
    public class SparseCartesianMap<T> : IReadOnlyCartesianMap<T>
    {
        private readonly Dictionary<VectorInt2, T> inner = new Dictionary<VectorInt2, T>();

        // As this is space, it must be in the dictionary
        public bool Contains(VectorInt2 p) => inner.ContainsKey(p);

        
        public IEnumerable<(VectorInt2 Position, T Value)> ForEach()
        { 
            foreach (var kv in inner)
            {
                yield return (kv.Key, kv.Value);
            }
        }

        public int        Width  { get; private set; }
        public int        Height { get; private set; }
        public VectorInt2 Size   => new VectorInt2(Width, Height);

        public T this[int x, int y]
        {
            get 
            { 
                if (inner.TryGetValue(new VectorInt2(x, y), out var v)) return v;
                return default;
            }
            set => this[new VectorInt2(x, y)] = value;
        }

        public T this[VectorInt2 p]
        {
            get 
            { 
                if (inner.TryGetValue(p, out var v)) return v;
                return default;
            }
            set
            {
                inner[p] = value;
                if (p.X >= Width) Width = p.X + 1;
                if (p.Y >= Height) Width = p.Y + 1;
            }
        }

        public IEnumerable<T> ForEachValue() => inner.Values;
    }
}