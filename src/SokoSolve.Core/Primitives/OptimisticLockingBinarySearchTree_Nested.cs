using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using TextRenderZ;

namespace SokoSolve.Core.Primitives
{
    public partial class OptimisticLockingBinarySearchTree<T> : ISearchTree<T>
    {
        public class Node
        {
            private volatile bool locked;
            
            public Node(Node? parent, T value)
            {
                Debug.Assert(value is not null);
                Value  = value;
                Parent = parent;
            }
            
            
            public T          Value  { get; private set; }
            public Node?      Parent { get; private set; }
            public Node?      Left   { get; private set; }
            public Node?      Right  { get; private set; }
            public EqualNode? Equal  { get; private set; }   // Bucket (LinkedList) for all T with the same hash (Should be sorted)
            

            public IEnumerable<T> ForeachSameHash()
            {
                CheckLockBySpin();
                yield return Value;
                
                var e = Equal;
                while (e is not null)
                {
                    yield return e.Value;
                    e = e.Next;
                }
            }
            
            public void WalkNodes(Action<Node> each)
            {
                CheckLockBySpin();
                each(this);
                Left?.WalkNodes(each);
                Right?.WalkNodes(each);
            }
            
                
            public void Walk(Action<T> each)
            {
                CheckLockBySpin();
                each(Value);
                var eq = Equal;
                while (eq is not null)
                {
                    each(eq.Value);
                    eq = eq.Next;
                }
                
                Left?.Walk(each);
                Right?.Walk(each);
            }

            public override string ToString()
            {
                var c  = 0;
                var eq = Equal;
                while (eq != null)
                {
                    c++;
                    eq = eq.Next;
                }
                return new FluentString(" ")
                       .Append($"[{Left!.Value}]")
                       .Append(Value!.ToString())
                       .If(c > 0, $" eq{c}")
                       .IfNotNull(Right, x=>$"[{Right!.Value}]")
                    ;
            }
            
            public void CheckLockBySpin()
            {
                int cc = 0;
                while (locked)
                {
                    cc++;
                    
                    if (cc > 200)
                    {
                        throw new Exception("Locked Cycle? Timeout?"); // super crude
                    }
                    if (cc > 100)
                    {
                        Thread.Sleep(100);
                    }
                }
            }


            private object locker = new object();
            private void LockAndMutate(Action<Node> change)
            {
                CheckLockBySpin();
                lock (locker)
                {
                    locked = true;
                    change(this);
                }
                locked = false;
            }
            
            public void AddEqual(T item)
            {
                CheckLockBySpin();
                LockAndMutate(x => {
                    var newFirst = new EqualNode(item)
                    {
                        Next = Equal
                    };
                    Equal = newFirst;    
                });
                
            }

            public bool TrySetLeft(T item)
            {
                CheckLockBySpin();
                bool res = false;
                LockAndMutate(x => {
                    if (Left is null)
                    {
                        Left = new Node(this, item);
                        res  = true;
                    }
                });
                return res;
            }
            
            public bool TrySetRight(T item)
            {
                CheckLockBySpin();
                bool res = false;
                LockAndMutate(x => {
                    if (Right is null)
                    {
                        Right = new Node(this, item);
                        res  = true;
                    }
                });
                
                return res;
            }
            
            public bool TryRemove(IComparer<T> comparer, T item)
            {
                CheckLockBySpin();
                bool       ret  = false;
                EqualNode? prev = null;
                LockAndMutate(x => {
                    
                    var ee = Equal;
                    while (ee != null)
                    {
                        if (comparer.Compare(ee.Value, item) == 0)
                        {
                            if (prev == null)
                            {
                                Equal = ee.Next;
                            }
                            else
                            {
                                prev.Next = ee.Next;
                            }
                            ret = true;
                        }
                        ee = ee.Next;
                    }

                    if (comparer.Compare(Value, item) == 0)
                    {
                        if (Equal == null)
                        {

                            throw new NotImplementedException("HARD CASE: Remove this node from the tree");

                        }
                        else
                        {
                            Value = Equal.Value;
                            Equal = Equal.Next;
                        }
                    }
                });
                return ret;
            }
            
        }
        
        
        public class EqualNode
        {
            public EqualNode(T value)
            {
                Debug.Assert(value is not null);
                Value = value;
            }

            public T          Value { get; }
            public EqualNode? Next  { get; set; }
        }
    }
}