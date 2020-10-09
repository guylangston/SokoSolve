using System;
using System.Collections;
using System.Collections.Generic;

namespace SokoSolve.Core.Common
{
    public abstract class TreeNodeBase : ITreeNode
    {
        private List<ITreeNode>? children;
        public ITreeNodeParent?       Parent { get; protected set; }
        
        public  IEnumerable<ITreeNode>? Children    => children;
        public  bool                   HasChildren => children != null && children.Count > 0;


        public void Clear()
        {
            children = null;
            Parent = null;
        }

        public ITreeNode Add(ITreeNode newChild)
        {
            if (children == null) children = new List<ITreeNode>();
            var node = (TreeNodeBase) newChild;
            node.Parent = this;
            children.Add(node);
            return newChild;
        }

        public void Remove(ITreeNode existingNode) => children?.Remove(existingNode);

        public IEnumerator<ITreeNode> GetEnumerator()
        {
            yield return this;
            if (HasChildren)
                foreach (var inner in children)
                    foreach (var i in inner)
                        yield return i;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    
    
    public abstract class TreeNodeBaseFixedKids: ITreeNode
    {
        private ITreeNodeParent? parent;
        private ITreeNode[]? children;

        public ITreeNodeParent? Parent
        {
            get => parent;
            protected set => parent = value;
        }

        public IEnumerable<ITreeNode>? Children    => children;
        public bool HasChildren => children != null && children.Length > 0;


        public void Clear()
        {
            children = null;
            parent   = null;
        }

        public ITreeNode Add(ITreeNode newChild)
        {
            var node = (TreeNodeBaseFixedKids) newChild;
            node.parent = this;

            
            if (children == null)
            {
                children = new []{ node};
                return node;
            }
            else
            {
                
                Array.Resize(ref children, children.Length + 1);
                children[children.Length -1] = node;

                return node;    
            }
            
        }
        
        public void SetChildren(ITreeNode[] kids)
        {
            children = kids;
            foreach (TreeNodeBaseFixedKids kid in kids)
            {
                kid.parent = this;
            }

        }

        public void Remove(ITreeNode existingNode) => throw new NotSupportedException();

        public IEnumerator<ITreeNode> GetEnumerator()
        {
            yield return this;
            if (HasChildren)
                foreach (var inner in children!)
                foreach (var i in inner)
                    yield return i;
        }
        
       

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}