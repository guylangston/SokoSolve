using System;
using System.Collections.Generic;
using System.Linq;

namespace SokoSolve.Core.Common
{
    /// <summary>
    ///     Enumerable is a recursive function
    /// </summary>
    public interface ITreeNode : IEnumerable<ITreeNode>
    {
        ITreeNode Parent { get; }
        IEnumerable<ITreeNode> Children { get; }

        bool HasChildren { get; }

        ITreeNode Add(ITreeNode newChild);

        void Remove(ITreeNode existingNode);
    }

    public static class ITreeNodeExt
    {
        public static int GetDepth<T>(this T node) where T : ITreeNode
        {
            var d = 0;
            while (node != null && node.Parent != null)
            {
                d++;
                node = (T) node.Parent;
            }

            return d;
        }

        public static T Root<T>(this T node) where T : ITreeNode
        {
            if (node == null) return default;

            while (node.Parent != null) node = (T) node.Parent;
            return node;
        }

        public static List<T> PathToRoot<T>(this T node) where T : ITreeNode
        {
            var res = new List<T>();
            while (node != null)
            {
                res.Add(node);
                node = (T) node.Parent;
            }

            res.Reverse();
            return res;
        }

        //##########################################################################################
        //##########################################################################################
        //##########################################################################################
        // LINQ functions - subset

        /// <summary>
        ///     Recursive where tree function
        /// </summary>
        public static IEnumerable<T> RecursiveAll<T>(this T node) where T : ITreeNode
        {
            yield return node;

            if (node.HasChildren)
                foreach (var inner in node.Children)
                foreach (T i in inner)
                    yield return i;
        }


        /// <summary>
        ///     Recursive where tree function
        /// </summary>
        public static IEnumerable<T> Where<T>(this T node, Func<T, bool> where) where T : ITreeNode
        {
            if (where(node)) yield return node;

            if (node.HasChildren)
                foreach (var inner in node.Children)
                foreach (T i in inner)
                    if (where(i))
                        yield return i;
        }

        /// <summary>
        ///     Recursive where tree function
        /// </summary>
        public static T FirstOrDefault<T>(this T node, Func<T, bool> where) where T : ITreeNode
        {
            if (where(node)) return node;

            foreach (var n in node.Where(where)) return n;
            return default;
        }

        /// <summary>
        ///     Recursive where tree function
        /// </summary>
        public static int Count<T>(this T node, Func<T, bool> where) where T : ITreeNode
        {
            // May not be very efficient?
            return node.Where(where).Count();
        }
    }

    /// <summary>
    ///     It is more efficient to subclass TreeNodeBase in a specific class than to use this Generic declaration
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TreeNode<T> : TreeNodeBase
    {
        public T Data { get; set; }
    }
}