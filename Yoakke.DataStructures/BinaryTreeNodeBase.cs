using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.DataStructures
{
    /// <summary>
    /// The node interface for the binary trees.
    /// </summary>
    /// <typeparam name="TNode">The implementation type.</typeparam>
    public abstract class BinaryTreeNodeBase<TNode> where TNode : BinaryTreeNodeBase<TNode>
    {
        /// <summary>
        /// The parent of this node.
        /// </summary>
        public abstract TNode? Parent { get; set; }
        /// <summary>
        /// The left child of this node.
        /// </summary>
        public abstract TNode? Left { get; set; }
        /// <summary>
        /// The right child of this node.
        /// </summary>
        public abstract TNode? Right { get; set; }

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
        public TNode? Grandparent => Parent?.Parent;
        /// <summary>
        /// The other child of the parent of this node.
        /// </summary>
        public TNode? Sibling => IsLeftChild ? Parent?.Right : Parent?.Left;
        /// <summary>
        /// The sibling of the parent of this node.
        /// </summary>
        public TNode? Uncle => Parent?.Sibling;
        /// <summary>
        /// The minimum (leftmost) element in this node's subtree.
        /// </summary>
        public TNode Minimum => Left?.Minimum ?? (TNode)this;
        /// <summary>
        /// The maximum (rightmost) element in this node's subtree.
        /// </summary>
        public TNode Maximum => Right?.Maximum ?? (TNode)this;
        /// <summary>
        /// The predecessor (maximum of the left subtree) of this node.
        /// </summary>
        public TNode? Predecessor => Left?.Maximum;
        /// <summary>
        /// The successor (minimum of the right subtree) of this node.
        /// </summary>
        public TNode? Successor => Right?.Minimum;

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
        public virtual void RotateLeft()
        {
            var pivot = Right;
            if (pivot == null) throw new InvalidOperationException("Can't rotate left without a right child!");

            var parent = Parent;
            Right = pivot.Left;
            pivot.Left = (TNode)this;

            if (Right != null) Right.Parent = (TNode)this;
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
        public virtual void RotateRight()
        {
            var pivot = Left;
            if (pivot == null) throw new InvalidOperationException("Can't rotate right without a left child!");

            var parent = Parent;
            Left = pivot.Right;
            pivot.Right = (TNode)this;

            if (Left != null) Left.Parent = (TNode)this;
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
        /// <returns>The <see cref="IEnumerable{TNode}"/> that yields the nodes in the order of pre-order
        /// traversal.</returns>
        public virtual IEnumerable<TNode> PreOrder()
        {
            var stack = new Stack<TNode>();
            stack.Push((TNode)this);
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
        /// <returns>The <see cref="IEnumerable{TNode}"/> that yields the nodes in the order of in-order
        /// traversal.</returns>
        public virtual IEnumerable<TNode> InOrder()
        {
            var stack = new Stack<TNode>();
            TNode? node = (TNode)this;
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
        /// <returns>The <see cref="IEnumerable{TNode}"/> that yields the nodes in the order of post-order
        /// traversal.</returns>
        public virtual IEnumerable<TNode> PostOrder()
        {
            var stack = new Stack<TNode>();
            TNode? node = (TNode)this;
            TNode? lastVisited = null;
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
        public static void Swap(TNode n1, TNode n2)
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

            // NOTE: Needed in case the parent is the same
            var n1IsLeftChild = n1Parent?.Left == n1;
            var n2IsLeftChild = n2Parent?.Left == n2;
            if (n1Parent != null)
            {
                if (n1IsLeftChild) n1Parent.Left = n2;
                else n1Parent.Right = n2;
            }
            if (n2Parent != null)
            {
                if (n2IsLeftChild) n2Parent.Left = n1;
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

            void SwapParentAndChild(TNode parent, TNode child)
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
}
