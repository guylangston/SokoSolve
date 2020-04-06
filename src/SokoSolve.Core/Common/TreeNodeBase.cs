using System.Collections;
using System.Collections.Generic;

namespace SokoSolve.Core.Common
{
    public abstract class TreeNodeBase : ITreeNode
    {
        private List<ITreeNode>? children;
        public  ITreeNode?       Parent { get; protected set; }
        
        public  IEnumerable<ITreeNode>? Children    => children;
        public  bool                   HasChildren => children != null && children.Count > 0;

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
}