using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Yoakke.DataStructures
{
    /// <summary>
    /// The colors of a red-black tree node.
    /// </summary>
    public enum Color
    {
        Red, Black,
    }

    /// <summary>
    /// The red-black tree node type.
    /// </summary>
    public class RedBlackTreeNode<TValue> : BinaryTreeNodeBase<RedBlackTreeNode<TValue>>
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
        public override RedBlackTreeNode<TValue>? Parent { get; set; }
        /// <summary>
        /// The left child of this <see cref="Node"/>.
        /// </summary>
        public override RedBlackTreeNode<TValue>? Left { get; set; }
        /// <summary>
        /// The right child of this <see cref="Node"/>.
        /// </summary>
        public override RedBlackTreeNode<TValue>? Right { get; set; }

        internal RedBlackTreeNode<TValue> LeftUnwrap => Left ?? throw new InvalidOperationException();
        internal RedBlackTreeNode<TValue> RightUnwrap => Right ?? throw new InvalidOperationException();

        // Observers

        /// <summary>
        /// True, if this <see cref="Node"/> is a leaf.
        /// </summary>
        public bool IsNil => Left == null && Right == null;
        
        public override RedBlackTreeNode<TValue> Minimum => base.Minimum?.Parent ?? throw new InvalidOperationException();
        public override RedBlackTreeNode<TValue> Maximum => base.Maximum?.Parent ?? throw new InvalidOperationException();

        /// <summary>
        /// The black height of this subtree.
        /// </summary>
        public int BlackHeight =>
            (Color == Color.Black ? 1 : 0) + (Left?.BlackHeight ?? 0);

        internal RedBlackTreeNode()
        {
        }

        internal void Validate()
        {
            // Every leaf must be black
            if (IsNil && Color != Color.Black) throw new ValidationException("Nil node is not black!");
            if (!IsNil)
            {
                // Links must be two-way
                if (LeftUnwrap.Parent != this) throw new ValidationException("Left parent mismatch!");
                if (RightUnwrap.Parent != this) throw new ValidationException("Right parent mismatch!");
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

        new public static void Swap(RedBlackTreeNode<TValue> n1, RedBlackTreeNode<TValue> n2)
        {
            var ctmp = n1.Color;
            n1.Color = n2.Color;
            n2.Color = ctmp;
            BinaryTreeNodeBase<RedBlackTreeNode<TValue>>.Swap(n1, n2);
        }
    }

    /// <summary>
    /// A red-black tree implementation to support other data-structures.
    /// </summary>
    /// <typeparam name="TKey">The key-type that the tree is sorted by.</typeparam>
    /// <typeparam name="TValue">The value-type the nodes store.</typeparam>
    public class RedBlackTree<TKey, TValue> : BinaryTreeBase<RedBlackTreeNode<TValue>>
    {
        // TODO: Debug
        public string ToDOT()
        {
            int count = 0;
            var result = new StringBuilder();
            ToDOT(result, root, out var _, ref count);
            return $"digraph g {{\n {result} }}";
        }
        public void ToDOT(StringBuilder result, RedBlackTreeNode<TValue> node, out string name, ref int count)
        {
            name = $"\"{(node.IsNil ? "nil" : node.Value?.ToString())} [{count++}]\"";
            result.AppendLine($"  {name} [color={(node.Color == Color.Black ? "black" : "red")}, style=filled, fontcolor=white]");
            if (node.IsNil) return;
            ToDOT(result, node.LeftUnwrap, out var leftName, ref count);
            ToDOT(result, node.RightUnwrap, out var rightName, ref count);
            result.AppendLine($"  {name} -> {leftName}");
            result.AppendLine($"  {name} -> {rightName}");
        }

        public override RedBlackTreeNode<TValue>? Root => root;
        private RedBlackTreeNode<TValue> root;
        public override int Count => count;
        private int count;

        /// <summary>
        /// The key selector function.
        /// </summary>
        public Func<TValue, TKey> KeySelector { get; }
        /// <summary>
        /// The comparer to compare keys.
        /// </summary>
        public IComparer<TKey> Comparer { get; }

        /// <summary>
        /// Initializes a new <see cref="RedBlackTree{TKey, TValue}"/>.
        /// </summary>
        /// <param name="keySelector">The key selector function.</param>
        /// <param name="comparer">The key comparer.</param>
        public RedBlackTree(Func<TValue, TKey> keySelector, IComparer<TKey> comparer)
        {
            root = new RedBlackTreeNode<TValue>();
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
            if (root.Color != Color.Black) throw new ValidationException("Root is not black!");
            root.Validate();
        }

        // Observers

        public override IEnumerable<RedBlackTreeNode<TValue>> PreOrder() =>
            base.PreOrder().Where(n => !n.IsNil);
        public override IEnumerable<RedBlackTreeNode<TValue>> InOrder() =>
            base.InOrder().Where(n => !n.IsNil);
        public override IEnumerable<RedBlackTreeNode<TValue>> PostOrder() =>
            base.PostOrder().Where(n => !n.IsNil);

        // Insertion

        /// <summary>
        /// Inserts a new nodewith the given value.
        /// </summary>
        /// <param name="value">The value to insert the new node with.</param>
        /// <param name="hint">The hint node to insert at.</param>
        /// <returns>The inserted node.</returns>
        public RedBlackTreeNode<TValue> Insert(TValue value, RedBlackTreeNode<TValue>? hint = null)
        {
            var node = new RedBlackTreeNode<TValue>();
            node.Value = value;
            Insert(node, hint);
            return node;
        }

        public override void Insert(RedBlackTreeNode<TValue> node, RedBlackTreeNode<TValue>? hint = null)
        {
            InsertLikeBinarySearchTree(node, hint);
            RebalanceInsertion(node);
            root = FindRoot(node);
            ++count;
        }

        private void InsertLikeBinarySearchTree(RedBlackTreeNode<TValue> node, RedBlackTreeNode<TValue>? hint)
        {
            var key = KeySelector(node.Value);
            RedBlackTreeNode<TValue>? prev = null;
            var current = hint ?? root;
            while (!current.IsNil)
            {
                prev = current;
                var cmp = Comparer.Compare(key, KeySelector(current.Value));
                current = cmp < 0 ? current.LeftUnwrap : current.RightUnwrap;
            }
            // Now current must be a nil leaf
            Debug.Assert(current.IsNil);
            // Now we must insert the value and make it red
            // Let's just use the swap method to ease ourselves
            current.Left = node;
            node.Parent = current;
            current.Right = new RedBlackTreeNode<TValue> { Parent = current };
            // Both are black, we can just swap
            RedBlackTreeNode<TValue>.Swap(current, node);
            node.Color = Color.Red;
        }

        private void RebalanceInsertion(RedBlackTreeNode<TValue> node)
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
                parent.RotateLeft();
                node = node.LeftUnwrap;
            }
            else if (node.IsLeftChild && parent.IsRightChild)
            {
                parent.RotateRight();
                node = node.RightUnwrap;
            }
            parent = node.Parent;
            Debug.Assert(parent != null);
            grandpa = parent.Parent;
            Debug.Assert(grandpa != null);
            if (node.IsLeftChild) grandpa.RotateRight();
            else grandpa.RotateLeft();
            parent.Color = Color.Black;
            grandpa.Color = Color.Red;
        }

        // Removal

        public override void Remove(RedBlackTreeNode<TValue> node)
        {
            if (node.IsNil) throw new InvalidOperationException();

            if (Count == 1)
            {
                Debug.Assert(node == Root);
                root = new RedBlackTreeNode<TValue>();
                count = 0;
                return;
            }

            if (!node.LeftUnwrap.IsNil && !node.RightUnwrap.IsNil)
            {
                // Search for the largest element in the left subtree
                var leftMax = node.Predecessor;
                Debug.Assert(leftMax != null);
                // Swap the two nodes
                RedBlackTreeNode<TValue>.Swap(node, leftMax);
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
            root = FindRoot(child);
            --count;
        }

        private void DeleteCase1(RedBlackTreeNode<TValue> node)
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
                    if (node.IsLeftChild) node.Parent.RotateLeft();
                    else node.Parent.RotateRight();
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

        private void DeleteCase5(RedBlackTreeNode<TValue> node)
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
                    sibling.RotateRight();
                }
                else if (node.IsRightChild && sibling.LeftUnwrap.Color == Color.Black
                      && sibling.RightUnwrap.Color == Color.Red)
                {
                    sibling.Color = Color.Red;
                    sibling.RightUnwrap.Color = Color.Black;
                    sibling.RotateLeft();
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
                node.Parent.RotateLeft();
            }
            else
            {
                sibling.LeftUnwrap.Color = Color.Black;
                node.Parent.RotateRight();
            }
        }

        private void ReplaceForDelete(RedBlackTreeNode<TValue> node, RedBlackTreeNode<TValue> child)
        {
            child.Parent = node.Parent;
            if (node.Parent != null)
            {
                if (node == node.Parent.Left) node.Parent.Left = child;
                else node.Parent.Right = child;
            }
        }
    }
}
