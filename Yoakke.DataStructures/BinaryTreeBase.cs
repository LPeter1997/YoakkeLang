using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.DataStructures
{
    /// <summary>
    /// A base class for every binary tree structure.
    /// </summary>
    /// <typeparam name="TNode">The implementation node type.</typeparam>
    public abstract class BinaryTreeBase<TNode> where TNode : BinaryTreeNodeBase<TNode>
    {
        /// <summary>
        /// The root node of this tree.
        /// </summary>
        public abstract TNode? Root { get; }
        /// <summary>
        /// The number of nodes in this tree.
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// Inserts the given node into this tree.
        /// </summary>
        /// <param name="node">The node to insert.</param>
        /// <param name="hint">The hint for the insertion. The hint must be a parent of the subtree
        /// where the insertion should happen.</param>
        public abstract void Insert(TNode node, TNode? hint = null);

        /// <summary>
        /// Removes the given node from this tree.
        /// </summary>
        /// <param name="node">The node to remove.</param>
        public abstract void Remove(TNode node);

        /// <summary>
        /// Does preorder traversal on this tree.
        /// </summary>
        /// <returns>The <see cref="IEnumerable{TNode}"/> that yields the nodes in the order of pre-order
        /// traversal.</returns>
        public virtual IEnumerable<TNode> PreOrder() => Root?.PreOrder() ?? Enumerable.Empty<TNode>();

        /// <summary>
        /// Does inorder traversal on this tree.
        /// </summary>
        /// <returns>The <see cref="IEnumerable{TNode}"/> that yields the nodes in the order of in-order
        /// traversal.</returns>
        public virtual IEnumerable<TNode> InOrder() => Root?.InOrder() ?? Enumerable.Empty<TNode>();

        /// <summary>
        /// Does postorder traversal on this tree.
        /// </summary>
        /// <returns>The <see cref="IEnumerable{TNode}"/> that yields the nodes in the order of post-order
        /// traversal.</returns>
        public virtual IEnumerable<TNode> PostOrder() => Root?.PostOrder() ?? Enumerable.Empty<TNode>();
    }
}
