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
    /// An interface every binary tree structure should implement.
    /// </summary>
    public interface IBinaryTree
    {
        /// <summary>
        /// The node interface for the binary trees.
        /// </summary>
        public interface INode
        {
            /// <summary>
            /// The parent of this node.
            /// </summary>
            public INode? Parent { get; set; }
            /// <summary>
            /// The left child of this node.
            /// </summary>
            public INode? Left { get; set; }
            /// <summary>
            /// The right child of this node.
            /// </summary>
            public INode? Right { get; set; }

            // Observers

            /// <summary>
            /// True, if this is a left child of it's parent.
            /// </summary>
            public bool IsLeftChild => Parent?.Left == this;
            /// <summary>
            /// True, if this is a right child of it's parent.
            /// </summary>
            public bool IsRightChild => Parent?.Right == this;

            // Relations

            /// <summary>
            /// The parent of this node's parent.
            /// </summary>
            public INode? Grandparent => Parent?.Parent;
            /// <summary>
            /// The other child of the parent of this node.
            /// </summary>
            public INode? Sibling => IsLeftChild ? Parent?.Right : Parent?.Left;
            /// <summary>
            /// The sibling of the parent of this node.
            /// </summary>
            public INode? Uncle => Parent?.Sibling;
            /// <summary>
            /// The minimum (leftmost) element in this node's subtree.
            /// </summary>
            public INode Minimum => Left?.Minimum ?? this;
            /// <summary>
            /// The maximum (rightmost) element in this node's subtree.
            /// </summary>
            public INode Maximum => Right?.Maximum ?? this;
            /// <summary>
            /// The predecessor (maximum of the left subtree) of this node.
            /// </summary>
            public INode? Predecessor => Left?.Maximum;
            /// <summary>
            /// The successor (minimum of the right subtree) of this node.
            /// </summary>
            public INode? Successor => Right?.Minimum;

            // Operations

            /// <summary>
            /// Does a left rotation on this subtree, with this element being the root.
            /// Description: https://en.wikipedia.org/wiki/Tree_rotation
            /// 
            /// Visually:
            /// parent            parent
            ///    |                 |
            ///  root              pivot
            ///  /   \             /  \
            /// x   pivot   =>   root  z
            ///      /  \         /  \
            ///     y   z        x    y
            /// </summary>
            public void RotateLeft()
            {
                var pivot = Right;
                if (pivot == null) throw new InvalidOperationException("Can't rotate left without a right child!");

                var parent = Parent;
                Right = pivot.Left;
                pivot.Left = this;

                if (Right != null) Right.Parent = this;
                if (parent != null)
                {
                    if (IsLeftChild) parent.Left = pivot;
                    else parent.Right = pivot;
                }

                Parent = pivot;
                pivot.Parent = parent;
            }

            /// <summary>
            /// Does a right rotation on this subtree, with this element being the root.
            /// Description: https://en.wikipedia.org/wiki/Tree_rotation
            /// 
            /// Visually:
            ///   parent        parent
            ///     |              |
            ///    root          pivot
            ///    /  \          /  \
            /// pivot  z   =>   x   root
            ///  /  \               /  \
            /// x    y             y    z
            /// </summary>
            public void RotateRight()
            {
                var pivot = Left;
                if (pivot == null) throw new InvalidOperationException("Can't rotate right without a left child!");

                var parent = Parent;
                Left = pivot.Right;
                pivot.Right = this;

                if (Left != null) Left.Parent = this;
                if (parent != null)
                {
                    if (IsLeftChild) parent.Left = pivot;
                    else parent.Right = pivot;
                }

                Parent = pivot;
                pivot.Parent = parent;
            }

            /// <summary>
            /// Does preorder traversal on this subtree.
            /// </summary>
            /// <returns>The <see cref="IEnumerable{INode}"/> that yields the nodes in the order of pre-order
            /// traversal.</returns>
            public IEnumerable<INode> PreOrder()
            {
                var stack = new Stack<INode>();
                stack.Push(this);
                while (stack.Count > 0)
                {
                    var node = stack.Pop();
                    yield return node;
                    if (node.Right != null) stack.Push(node.Right);
                    if (node.Left != null) stack.Push(node.Left);
                }
            }

            /// <summary>
            /// Does inorder traversal on this subtree.
            /// </summary>
            /// <returns>The <see cref="IEnumerable{INode}"/> that yields the nodes in the order of in-order
            /// traversal.</returns>
            public IEnumerable<INode> InOrder()
            {
                var stack = new Stack<INode>();
                INode? node = this;
                while (stack.Count > 0 || node != null)
                {
                    if (node != null)
                    {
                        stack.Push(node);
                        node = node.Left;
                    }
                    else
                    {
                        node = stack.Pop();
                        yield return node;
                        node = node.Right;
                    }
                }
            }

            /// <summary>
            /// Does postorder traversal on this subtree.
            /// </summary>
            /// <returns>The <see cref="IEnumerable{INode}"/> that yields the nodes in the order of post-order
            /// traversal.</returns>
            public IEnumerable<INode> PostOrder()
            {
                var stack = new Stack<INode>();
                INode? node = this;
                INode? lastVisited = null;
                while (stack.Count > 0 || node != null)
                {
                    if (node != null)
                    {
                        stack.Push(node);
                        node = node.Left;
                    }
                    else
                    {
                        var peekNode = stack.Peek();
                        if (peekNode.Right != null && lastVisited != peekNode.Right)
                        {
                            node = peekNode.Right;
                        }
                        else
                        {
                            yield return peekNode;
                            lastVisited = stack.Pop();
                        }
                    }
                }
            }

            /// <summary>
            /// Swaps the two nodes, taking eachother's position in the tree.
            /// This is good for swapping nodes without invalidating the references.
            /// </summary>
            /// <param name="n1">The first node to swap.</param>
            /// <param name="n2">The second node to swap.</param>
            public static void Swap(INode n1, INode n2)
            {
                if (n1.Parent == n2)
                {
                    SwapParentAndChild(n2, n1);
                    return;
                }
                if (n2.Parent == n1)
                {
                    SwapParentAndChild(n1, n2);
                    return;
                }

                var (n1Parent, n2Parent) = (n1.Parent, n2.Parent);
                var (n1Left, n2Left) = (n1.Left, n2.Left);
                var (n1Right, n2Right) = (n1.Right, n2.Right);

                if (n1Parent != null)
                {
                    if (n1Parent.Left == n1) n1Parent.Left = n2;
                    else n1Parent.Right = n2;
                }
                if (n2Parent != null)
                {
                    if (n2Parent.Left == n2) n2Parent.Left = n1;
                    else n2Parent.Right = n1;
                }
                n1.Parent = n2Parent;
                n2.Parent = n1Parent;

                n2.Left = n1Left;
                n2.Right = n1Right;
                if (n1Left != null) n1Left.Parent = n2;
                if (n1Right != null) n1Right.Parent = n2;

                n1.Left = n2Left;
                n1.Right = n2Right;
                if (n2Left != null) n2Left.Parent = n1;
                if (n2Right != null) n2Right.Parent = n1;

                void SwapParentAndChild(INode parent, INode child)
                {
                    Debug.Assert(child.Parent == parent);

                    var parentParent = parent.Parent;
                    var (parentLeft, childLeft) = (parent.Left, child.Left);
                    var (parentRight, childRight) = (parent.Right, child.Right);

                    if (childLeft != null) childLeft.Parent = parent;
                    if (childRight != null) childRight.Parent = parent;
                    parent.Left = childLeft;
                    parent.Right = childRight;
                    parent.Parent = child;
                    child.Parent = parentParent;

                    if (parentParent != null)
                    {
                        if (parentParent.Left == parent) parentParent.Left = child;
                        else parentParent.Right = child;
                    }

                    if (parentLeft == child)
                    {
                        child.Left = parent;
                        child.Right = parentRight;
                        if (parentRight != null) parentRight.Parent = child;
                    }
                    else
                    {
                        child.Right = parent;
                        child.Left = parentLeft;
                        if (parentLeft != null) parentLeft.Parent = child;
                    }
                }
            }
        }

        /// <summary>
        /// The root node of this tree.
        /// </summary>
        public INode? Root { get; }
        /// <summary>
        /// The number of nodes in this tree.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Inserts the given node into this tree.
        /// </summary>
        /// <param name="node">The node to insert.</param>
        /// <param name="hint">The hint for the insertion. The hint must be a parent of the subtree
        /// where the insertion should happen.</param>
        public void Insert(INode node, INode? hint = null);

        /// <summary>
        /// Removes the given node from this tree.
        /// </summary>
        /// <param name="node">The node to remove.</param>
        public void Remove(INode node);

        /// <summary>
        /// Does preorder traversal on this tree.
        /// </summary>
        /// <returns>The <see cref="IEnumerable{INode}"/> that yields the nodes in the order of pre-order
        /// traversal.</returns>
        public IEnumerable<INode> PreOrder() => Root?.PreOrder() ?? Enumerable.Empty<INode>();

        /// <summary>
        /// Does inorder traversal on this tree.
        /// </summary>
        /// <returns>The <see cref="IEnumerable{INode}"/> that yields the nodes in the order of in-order
        /// traversal.</returns>
        public IEnumerable<INode> InOrder() => Root?.InOrder() ?? Enumerable.Empty<INode>();

        /// <summary>
        /// Does postorder traversal on this tree.
        /// </summary>
        /// <returns>The <see cref="IEnumerable{INode}"/> that yields the nodes in the order of post-order
        /// traversal.</returns>
        public IEnumerable<INode> PostOrder() => Root?.PostOrder() ?? Enumerable.Empty<INode>();
    }
}
