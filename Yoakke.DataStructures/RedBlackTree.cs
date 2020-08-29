using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Yoakke.DataStructures
{
    /// <summary>
    /// A red-black tree implementation to support other data-structures.
    /// </summary>
    /// <typeparam name="TKey">The key-type that the tree is sorted by.</typeparam>
    /// <typeparam name="TValue">The value-type the nodes store.</typeparam>
    public class RedBlackTree<TKey, TValue>
    {
        // TODO: Debug
        public string ToDOT()
        {
            int count = 0;
            var result = new StringBuilder();
            ToDOT(result, Root, out var _, ref count);
            return $"digraph g {{\n {result} }}";
        }
        public void ToDOT(StringBuilder result, Node node, out string name, ref int count)
        {
            name = $"\"{(node.IsNil ? "nil" : node.Value?.ToString())} [{count++}]\"";
            result.AppendLine($"  {name} [color={(node.Color == Color.Black ? "black" : "red")}, style=filled, fontcolor=white]");
            if (node.IsNil) return;
            ToDOT(result, node.LeftUnwrap, out var leftName, ref count);
            ToDOT(result, node.RightUnwrap, out var rightName, ref count);
            result.AppendLine($"  {name} -> {leftName}");
            result.AppendLine($"  {name} -> {rightName}");
        }

        /// <summary>
        /// The colors of a red-black tree node.
        /// </summary>
        public enum Color { Red, Black }

        /// <summary>
        /// The node-type the red-black tree consists of.
        /// </summary>
        public class Node
        {
            // Payload

            /// <summary>
            /// The <see cref="Color"/> of this <see cref="Node"/>.
            /// </summary>
            public Color Color { get; internal set; } = Color.Black;
            /// <summary>
            /// The value stored inside this <see cref="Node"/>.
            /// </summary>
            public TValue Value
            {
                get
                {
                    if (IsNil) throw new InvalidOperationException();
                    Debug.Assert(value != null);
                    return value;
                }
                internal set => this.value = value;
            }
            private TValue? value;

            // Neighbors

            /// <summary>
            /// The parent of this <see cref="Node"/>.
            /// Can be null, if this is the root or nil.
            /// </summary>
            public Node? Parent { get; internal set; }
            /// <summary>
            /// The left child of this <see cref="Node"/>.
            /// </summary>
            public Node? Left { get; set; }
            /// <summary>
            /// The right child of this <see cref="Node"/>.
            /// </summary>
            public Node? Right { get; set; }

            internal Node LeftUnwrap => Left ?? throw new InvalidOperationException();
            internal Node RightUnwrap => Right ?? throw new InvalidOperationException();

            // Observers

            /// <summary>
            /// True, if this <see cref="Node"/> is a leaf.
            /// </summary>
            public bool IsNil => Left == null && Right == null;
            /// <summary>
            /// True, if this is a left child of it's parent.
            /// </summary>
            public bool IsLeftChild => Parent?.Left == this;
            /// <summary>
            /// True, if this is a right child of it's parent.
            /// </summary>
            public bool IsRightChild => Parent?.Right == this;
            /// <summary>
            /// The parent of the parent's <see cref="Node"/>.
            /// </summary>
            public Node? Grandparent => Parent?.Parent;
            /// <summary>
            /// The other child of the parent of this <see cref="Node"/>.
            /// </summary>
            public Node? Sibling => IsLeftChild ? Parent?.Right : Parent?.Left;
            /// <summary>
            /// The sibling of the parent of this <see cref="Node"/>.
            /// </summary>
            public Node? Uncle => Parent?.Sibling;
            /// <summary>
            /// The minimum (leftmost) element in this <see cref="Node"/>s subtree.
            /// </summary>
            public Node? Minimum
            {
                get
                {
                    if (IsNil) return null;
                    var result = this;
                    while (!result.LeftUnwrap.IsNil) result = result.LeftUnwrap;
                    return result;
                }
            }
            /// <summary>
            /// The maximum (rightmost) element in this <see cref="Node"/>s subtree.
            /// </summary>
            public Node? Maximum
            {
                get
                {
                    if (IsNil) throw new InvalidOperationException();
                    var result = this;
                    while (!result.RightUnwrap.IsNil) result = result.RightUnwrap;
                    return result;
                }
            }
            /// <summary>
            /// The predecessor (maximum of the left subtree) of this <see cref="Node"/>.
            /// </summary>
            public Node? Predecessor => 
                (IsNil || LeftUnwrap.IsNil) ? null : LeftUnwrap.Maximum;
            /// <summary>
            /// The successor (minimum of the right subtree) of this <see cref="Node"/>.
            /// </summary>
            public Node? Successor =>
                (IsNil || RightUnwrap.IsNil) ? null : RightUnwrap.Minimum;
            /// <summary>
            /// The black height of this subtree.
            /// </summary>
            public int BlackHeight =>
                (Color == Color.Black ? 1 : 0) + (Left?.BlackHeight ?? 0);

            internal Node()
            {
            }

            internal void Validate()
            {
                // Every leaf must be black
                if (IsNil && Color != Color.Black) throw new ValidationException("Nil node is not black!");
                if (!IsNil)
                {
                    // Every path from this node to the leaves must contain the same amount of  black nodes
                    var leftBlackHeight = LeftUnwrap.BlackHeight;
                    var rightBlackHeight = RightUnwrap.BlackHeight;
                    if (leftBlackHeight != rightBlackHeight) throw new ValidationException("Black height mismatch!");
                    // Every red node's child must be black
                    if (Color == Color.Red && (LeftUnwrap.Color != Color.Black || RightUnwrap.Color != Color.Black))
                    {
                        throw new ValidationException("Children of red are not both black!");
                    }
                    LeftUnwrap.Validate();
                    RightUnwrap.Validate();
                }
            }
        }

        /// <summary>
        /// The root <see cref="Node"/> of this red-black tree.
        /// </summary>
        public Node Root { get; private set; }
        /// <summary>
        /// The key selector function.
        /// </summary>
        public Func<TValue, TKey> KeySelector { get; }
        /// <summary>
        /// The comparer to compare keys.
        /// </summary>
        public IComparer<TKey> Comparer { get; }
        /// <summary>
        /// The number of <see cref="Node"/>s present in the tree, not counting nil nodes.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Initializes a new <see cref="RedBlackTree{TKey, TValue}"/>.
        /// </summary>
        /// <param name="keySelector">The key selector function.</param>
        /// <param name="comparer">The key comparer.</param>
        public RedBlackTree(Func<TValue, TKey> keySelector, IComparer<TKey> comparer)
        {
            Root = new Node();
            KeySelector = keySelector;
            Comparer = comparer;
        }

        /// <summary>
        /// Initializes a new <see cref="RedBlackTree{TKey, TValue}"/>.
        /// </summary>
        /// <param name="keySelector">The key selector function.</param>
        public RedBlackTree(Func<TValue, TKey> keySelector)
            : this(keySelector, Comparer<TKey>.Default)
        {
        }

        /// <summary>
        /// Validates the constraints of this red-black tree.
        /// </summary>
        public void Validate()
        {
            if (Root.Color != Color.Black) throw new ValidationException("Root is not black!");
            Root.Validate();
        }

        // Observers

        // TODO: Doc
        public IEnumerable<Node> Preorder(bool includeNil = false)
        {
            var stack = new Stack<Node>();
            stack.Push(Root);
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                if (node.IsNil)
                {
                    if (includeNil) yield return node;
                }
                else
                {
                    yield return node;
                    stack.Push(node.RightUnwrap);
                    stack.Push(node.LeftUnwrap);
                }
            }
        }

        // TODO: Doc
        public IEnumerable<Node> Inorder(bool includeNil = false)
        {
            var stack = new Stack<Node>();
            Node? node = Root;
            while (stack.Count > 0 || node != null)
            {
                if (node != null)
                {
                    stack.Push(node);
                    node = node.IsNil ? null : node.Left;
                }
                else
                {
                    node = stack.Pop();
                    if (node.IsNil)
                    {
                        if (includeNil) yield return node;
                        node = null;
                    }
                    else
                    {
                        yield return node;
                        node = node.Right;
                    }
                }
            }
        }

        // TODO: Doc
        public IEnumerable<Node> Postorder(bool includeNil = false)
        {
            var stack = new Stack<Node>();
            Node? node = Root;
            Node? lastVisited = null;
            while (stack.Count > 0 || node != null)
            {
                if (node != null)
                {
                    stack.Push(node);
                    node = node.IsNil ? null : node.Left;
                }
                else
                {
                    var peekNode = stack.Peek();
                    if (!peekNode.IsNil && lastVisited != peekNode.Right)
                    {
                        node = peekNode.Right;
                    }
                    else
                    {
                        if (peekNode.IsNil)
                        {
                            if (includeNil) yield return peekNode;
                        }
                        else
                        {
                            yield return peekNode;
                        }
                        lastVisited = stack.Pop();
                    }
                }
            }
        }

        // Insertion

        /// <summary>
        /// Inserts a new <see cref="Node"/> with the given value.
        /// </summary>
        /// <param name="value">The value to insert the new <see cref="Node"/> with.</param>
        /// <returns>The inserted <see cref="Node"/>.</returns>
        public Node Insert(TValue value)
        {
            var node = InsertLikeBinarySearchTree(value);
            RebalanceInsertion(node);

            // Search root
            Root = node;
            while (Root.Parent != null) Root = Root.Parent;

            ++Count;
            return node;
        }

        private Node InsertLikeBinarySearchTree(TValue value)
        {
            var key = KeySelector(value);
            Node? prev = null;
            var current = Root;
            while (!current.IsNil)
            {
                prev = current;
                var cmp = Comparer.Compare(key, KeySelector(current.Value));
                current = cmp < 0 ? current.LeftUnwrap : current.RightUnwrap;
            }
            // Now current must be a nil leaf
            Debug.Assert(current.IsNil);
            // Now we must insert the value and make it red
            current.Value = value;
            current.Color = Color.Red;
            current.Parent = prev;
            // Add the two stub nil nodes
            current.Left = new Node { Parent = current };
            current.Right = new Node { Parent = current };
            return current;
        }

        private void RebalanceInsertion(Node node)
        {
            if (node.Parent == null)
            {
                // Case 1: No parent of the inserted node
                // This means that the inserted node is the root node, which has to be black
                // No other elements has been inserted yet, nothing to do
                node.Color = Color.Black;
                return;
            }
            if (node.Parent.Color == Color.Black)
            {
                // Case 2: Parent's color is black
                // We have inserted a red node, which doesn't require any additional rebalancing
                return;
            }
            var uncle = node.Uncle;
            if (uncle != null && uncle.Color == Color.Red)
            {
                // Case 3: Parent and uncle are red
                node.Parent.Color = Color.Black;
                uncle.Color = Color.Black;
                var grandparent = node.Grandparent;
                Debug.Assert(grandparent != null);
                grandparent.Color = Color.Red;
                RebalanceInsertion(grandparent);
                return;
            }
            // Case 4
            var parent = node.Parent;
            var grandpa = node.Grandparent;
            Debug.Assert(grandpa != null);
            if (node.IsRightChild && parent.IsLeftChild)
            {
                RotateLeft(parent);
                node = node.LeftUnwrap;
            }
            else if (node.IsLeftChild && parent.IsRightChild)
            {
                RotateRight(parent);
                node = node.RightUnwrap;
            }
            parent = node.Parent;
            Debug.Assert(parent != null);
            grandpa = parent.Parent;
            Debug.Assert(grandpa != null);
            if (node.IsLeftChild) RotateRight(grandpa);
            else RotateLeft(grandpa);
            parent.Color = Color.Black;
            grandpa.Color = Color.Red;
        }

        // Removal

        /// <summary>
        /// Removes the given <see cref="Node"/>.
        /// </summary>
        /// <param name="node">The <see cref="Node"/> to remove.</param>
        public void Remove(Node node)
        {
            if (node.IsNil) throw new InvalidOperationException();

            if (Count == 1)
            {
                Debug.Assert(node == Root);
                Root = new Node();
                Count = 0;
                return;
            }

            if (!node.LeftUnwrap.IsNil && !node.RightUnwrap.IsNil)
            {
                // Search for the largest element in the left subtree
                var leftMax = node.Predecessor;
                Debug.Assert(leftMax != null);
                // Swap the two nodes
                SwapNodesForDelete(node, leftMax);
            }

            Debug.Assert(node.LeftUnwrap.IsNil || node.RightUnwrap.IsNil);

            var child = node.RightUnwrap.IsNil ? node.LeftUnwrap : node.RightUnwrap;
            ReplaceForDelete(node, child);
            if (node.Color == Color.Black)
            {
                if (child.Color == Color.Red) child.Color = Color.Black;
                else DeleteCase1(child);
            }

            // Search root
            Root = child;
            while (Root.Parent != null) Root = Root.Parent;

            --Count;
        }

        private void DeleteCase1(Node node)
        {
            if (node.Parent != null)
            {
                // Case 2
                var sibling = node.Sibling;
                Debug.Assert(sibling != null);
                if (sibling.Color == Color.Red)
                {
                    Debug.Assert(node.Parent != null);
                    node.Parent.Color = Color.Red;
                    sibling.Color = Color.Black;
                    if (node.IsLeftChild) RotateLeft(node.Parent);
                    else RotateRight(node.Parent);
                }
                // Case 3
                sibling = node.Sibling;
                Debug.Assert(sibling != null);
                if (node.Parent.Color == Color.Black && sibling.Color == Color.Black
                 && sibling.LeftUnwrap.Color == Color.Black && sibling.RightUnwrap.Color == Color.Black)
                {
                    sibling.Color = Color.Red;
                    DeleteCase1(node.Parent);
                }
                else
                {
                    // Case 4
                    Debug.Assert(node.Parent != null);
                    Debug.Assert(sibling != null);
                    if (node.Parent.Color == Color.Red && sibling.Color == Color.Black
                     && sibling.LeftUnwrap.Color == Color.Black && sibling.RightUnwrap.Color == Color.Black)
                    {
                        sibling.Color = Color.Red;
                        node.Parent.Color = Color.Black;
                    }
                    else
                    {
                        DeleteCase5(node);
                    }
                }
            }
        }

        private void DeleteCase5(Node node)
        {
            var sibling = node.Sibling;
            Debug.Assert(node.Parent != null);
            Debug.Assert(sibling != null);
            if (sibling.Color == Color.Black)
            {
                if (node.IsLeftChild && sibling.RightUnwrap.Color == Color.Black
                 && sibling.LeftUnwrap.Color == Color.Red)
                {
                    sibling.Color = Color.Red;
                    sibling.LeftUnwrap.Color = Color.Black;
                    RotateRight(sibling);
                }
                else if (node.IsRightChild && sibling.LeftUnwrap.Color == Color.Black
                      && sibling.RightUnwrap.Color == Color.Red)
                {
                    sibling.Color = Color.Red;
                    sibling.RightUnwrap.Color = Color.Black;
                    RotateLeft(sibling);
                }
            }
            // Case 6
            sibling = node.Sibling;
            Debug.Assert(node.Parent != null);
            Debug.Assert(sibling != null);
            sibling.Color = node.Parent.Color;
            node.Parent.Color = Color.Black;

            if (node.IsLeftChild)
            {
                sibling.RightUnwrap.Color = Color.Black;
                RotateLeft(node.Parent);
            }
            else
            {
                sibling.LeftUnwrap.Color = Color.Black;
                RotateRight(node.Parent);
            }
        }

        private void ReplaceForDelete(Node node, Node child)
        {
            child.Parent = node.Parent;
            if (node.Parent != null)
            {
                if (node == node.Parent.Left) node.Parent.Left = child;
                else node.Parent.Right = child;
            }
        }

        private void SwapNodesForDelete(Node n1, Node n2)
        {
            var ctmp = n1.Color;
            n1.Color = n2.Color;
            n2.Color = ctmp;

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

            var n1Parent = n1.Parent;
            var n1Left = n1.LeftUnwrap;
            var n1Right = n1.RightUnwrap;

            var n2Parent = n2.Parent;
            var n2Left = n2.LeftUnwrap;
            var n2Right = n2.RightUnwrap;

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
            n1Left.Parent = n2;
            n2.Right = n1Right;
            n1Right.Parent = n2;

            n1.Left = n2Left;
            n2Left.Parent = n1;
            n1.Right = n2Right;
            n2Right.Parent = n1;
        }

        private void SwapParentAndChild(Node parent, Node child)
        {
            Debug.Assert(child.Parent != null);

            var parentParent = parent.Parent;
            var parentLeft = parent.LeftUnwrap;
            var parentRight = parent.RightUnwrap;
            var childLeft = child.LeftUnwrap;
            var childRight = child.RightUnwrap;

            childLeft.Parent = parent;
            childRight.Parent = parent;
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
                parentRight.Parent = child;
            }
            else
            {
                child.Right = parent;
                child.Left = parentLeft;
                parentLeft.Parent = child;
            }
        }

        // General utilities

        /**
         * parent            parent
         *   |                  |
         *  root              pivot
         *  /  \              /  \
         * x   pivot   =>   root  z 
         *     /  \         /  \
         *    y    z       x    y
         */
        private void RotateLeft(Node root)
        {
            var pivot = root.RightUnwrap;
            var parent = root.Parent;

            root.Right = pivot.Left;
            pivot.Left = root;
            root.RightUnwrap.Parent = root;

            if (parent != null)
            {
                if (root.IsLeftChild) parent.Left = pivot;
                else parent.Right = pivot;
            }

            root.Parent = pivot;
            pivot.Parent = parent;
        }

        /**
         *    parent        parent
         *      |              |
         *     root          pivot
         *     /  \          /  \
         *  pivot  z   =>   x   root
         *   /  \               /  \
         *  x    y             y    z
         */
        private void RotateRight(Node root)
        {
            var pivot = root.LeftUnwrap;
            var parent = root.Parent;

            root.Left = pivot.Right;
            pivot.Right = root;
            root.LeftUnwrap.Parent = root;

            if (parent != null)
            {
                if (root.IsLeftChild) parent.Left = pivot;
                else parent.Right = pivot;
            }

            root.Parent = pivot;
            pivot.Parent = parent;
        }
    }
}
