using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using TextRenderZ;

namespace SokoSolve.Core.Primitives
{
    public interface ISearchTree<T>
    {
        int Count { get; }
        bool TryFind(T item, out T match);  // Better than contains as it yields match
        bool TryAdd(T item, out T? dup);     // false mean, not added - as it already exists (returns dup)

        void ForEachOptimistic(Action<T> each);
        void ForEachSafe(Action<T> each);         // Effectively a global lock

        void CopyTo(List<T> dest);
    }
    
    // Assumptions:
    // - Does not allow add/remove of null
    // - Set (only allows one equal instance), ie. no dups
    // - Tree sorted by Hash
    public partial class OptimisticLockingBinarySearchTree<T> : ISearchTree<T>
    {
        private volatile int count;
        private volatile int countHashCollision;
        private readonly IComparer<T> compare;
        private readonly Func<T, int> hasher;
        private Node? root;

        public OptimisticLockingBinarySearchTree(IComparer<T> compare, Func<T, int> hasher)
        {
            this.compare = compare;
            this.hasher  = hasher;
        }

        public int Count => count;
        public int CountHashCollision => countHashCollision;

        public bool TryFind(T item, out T? match)
        {
            Debug.Assert(item is not null);
            if (root is null)
            {
                match = default;
                return true;
            }
            
            var itemHash = hasher(item);
            var curr        = root!;

            while (curr != null)
            {
                curr.CheckLockBySpin();
                lock (curr)
                {
                    var currHash = hasher(curr.Value);
                    var cmp      = currHash.CompareTo(itemHash);
                    if (cmp == 0)
                    {
                    
                        // TODO: This is currently an UNORDERED list, it should be ORDERED
                        foreach (var eq in curr.ForeachSameHash())
                        {
                            if (compare.Compare(eq, item) == 0)
                            {
                                match = eq;
                                return true;
                            }
                        }

                        match = default;
                        return false;
                    }
                

                    if (cmp < 0)
                    {
                        if (curr.Left is null)
                        {
                            match = default;
                            return false;
                        }
                        curr = curr.Left;
                    }
                    else // > 0
                    {
                        if (curr.Right is null)
                        {
                            match = default;
                            return false;
                        
                        }
                        curr = curr.Right;
                    }    
                }
            }

            match = default;
            return false;
            
        }
        
        public bool TryAdd(T item, out T? dup)
        {
            Debug.Assert(item is not null);
            if (root is null)
            {
                lock (this)
                {
                    if (root is null)
                    {
                        root = new Node(null, item);
                        Interlocked.Increment(ref count);
                        dup = default;
                        return true;    
                    }
                }
            }
            var itemHash = hasher(item);
            var curr        = root!;

            while (curr != null)
            {
                curr.CheckLockBySpin();
                lock (curr)     // HOPE TO: remove need for this
                {
                    var currHash = hasher(curr.Value);
                    var cmp      = currHash.CompareTo(itemHash);
                    if (cmp == 0)
                    {
                        // TODO: This is currently an UNORDERED list, it should be ORDERED
                        foreach (var eq in curr.ForeachSameHash())
                        {
                            if (compare.Compare(eq, item) == 0)
                            {
                                dup = eq;
                                return false;
                            }
                        }

                        curr.AddEqual(item);
                        Interlocked.Increment(ref count);
                        Interlocked.Increment(ref countHashCollision);
                    
                        dup = default;
                        return true;
                    }

                    if (cmp < 0)
                    {
                        if (curr.Left is null)
                        {
                            if (curr.TrySetLeft(item))
                            {
                                Interlocked.Increment(ref count);
                        
                                dup = default;
                                return true;    
                            }
                        
                        
                        }
                        curr = curr.Left;
                    }
                    else // > 0
                    {
                        if (curr.Right is null)
                        {
                            if (curr.TrySetRight(item))
                            {
                                Interlocked.Increment(ref count);
                        
                                dup = default;
                                return true;    
                            }
                        
                        
                        }
                        curr = curr.Right;
                    }    
                }
                
               
            }

            throw new InvalidOperationException();

        }
        
        public void ForEachOptimistic(Action<T> each)
        {
            if (root is null) return;
            root.Walk(each);
        }
        
        
        public void ForEachSafe(Action<T> each)
        {
            throw new NotImplementedException();
        }
        
        public void CopyTo(List<T> dest)
        {
            throw new NotImplementedException();
        }


    }

  
}