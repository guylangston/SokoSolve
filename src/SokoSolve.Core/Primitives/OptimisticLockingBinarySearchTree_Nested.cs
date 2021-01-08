using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using TextRenderZ;

namespace SokoSolve.Core.Primitives
{
    public partial class OptimisticLockingBinarySearchTree<T> : ISearchTree<T>
    {
        public class Node
        {
            private volatile bool locked;
            
            public Node(OptimisticLockingBinarySearchTree<T> owner, Node? parent, T value)
            {
                Debug.Assert(value is not null);
                Debug.Assert(owner is not null);

                Owner  = owner;
                Value  = value;
                Parent = parent;
                Hash   = owner.hasher(value);
            }
            public int                                  Hash   { get; }
            public OptimisticLockingBinarySearchTree<T> Owner  { get; }
            public T                                    Value  { get; private set; }
            public Node?                                Parent { get; private set; }
            public Node?                                Left   { get; private set; }
            public Node?                                Right  { get; private set; }
            public EqualNode?                           Equal  { get; private set; }   // Bucket (LinkedList) for all T with the same hash (Should be sorted)
            public bool                                 IsLeaf => Left is null && Right is null && Equal is null;


            public void Verify()
            {
                if (Parent is null && Owner.root != this) throw new Exception($"Parent missing: {this}");
                
                foreach (var item in ForeachSameHash())
                {
                    if (Owner.hasher(item) != Owner.hasher(Value)) throw new Exception($"{item} != {Value}");
                    
                }
                if (Left is object)
                {
                    foreach (var item in Left.ForeachSameHash())
                    {
                        if (Owner.compare.Compare(item, Value) >= 0) throw new Exception($"{item} >= {Value}");
                    }
                    Left.Verify();
                }
                if (Right is object)
                {
                    foreach (var item in Right.ForeachSameHash())
                    {
                        if (Owner.compare.Compare(item, Value) <= 0) throw new Exception($"{item} <= {Value}");
                    }
                    Right.Verify();
                }
            }

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
                       .IfNotNull(Left,x =>$"[{(x != null ? x.Value : null)}]")
                       .Append(Value!.ToString())
                       .If(c > 0, $" eq{c}")
                       .IfNotNull(Right, x=>$"[{(x != null ? x.Value : null)}]")
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
                        Left = new Node(Owner, this, item);
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
                        Right = new Node(Owner,this, item);
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
                            if (Parent is null) throw new NotSupportedException("Must be removed by parent");

                            if (Left is null && Right is null)
                            {
                                // Just remove, no joins needed
                                if (Parent.Left == this)
                                {
                                    Parent.Left = null;
                                }
                                else if (Parent.Right == this)
                                {
                                    Parent.Right = null;
                                }
                                ret = true;
                                return;
                            }
                            
                            if (Parent.Left == this)
                            {
                                if (Right is not null)
                                {
                                    Parent.Left = Right;
                                    Right.Left  = Left;    
                                }
                                else
                                {
                                    if (Left is not null)
                                    {
                                        Parent.Left = Left;
                                    }
                                }
                            }
                            else if (Parent.Right == this)
                            {
                                if (Left is not null)
                                {
                                    Parent.Right = Left;
                                    Left.Right  = Right;    
                                }
                                else
                                {
                                    if (Right is not null)
                                    {
                                        Parent.Right = Right;
                                    }
                                }
                            }
                            
                        }
                        else
                        {
                            Value = Equal.Value;
                            Equal = Equal.Next;
                            ret   = true;
                            return;
                        }
                    }
                });
                return ret;
            }

            public Node RotateRight()
            {
                if (Left is null) throw new InvalidOperationException();

                var a = this;
                var b = Left;

                a.Left = null;
                b.InsertBelow(a);
                return b;
            }
            
            public Node RotateLeft()
            {
                if (Right is null) throw new InvalidOperationException();

                var a = this;
                var b = Right;

                a.Right = null;
                b.InsertBelow(a);
                return b;
            }
            
            
            private void InsertBelow(Node node)
            {
                var curr = this;
                while (curr != null)
                {
                    curr.CheckLockBySpin();
                    //lock (curr)
                    {
                        var cmp =  node.Hash.CompareTo(curr.Hash);
                        if (cmp == 0)
                        {
                            throw new Exception("Should never happen");
                        }
                    
                        if (cmp < 0)
                        {
                            if (curr.Left is null)
                            {
                                curr.Left   = node;
                                node.Parent = curr;
                                return;
                            }
                            curr = curr.Left;
                        }
                        else // > 0
                        {
                            if (curr.Right is null)
                            {
                                curr.Right   = node;
                                node.Parent = curr;
                                return;
                            }
                            curr = curr.Right;
                        }    
                    }
                }
                throw new InvalidOperationException();
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