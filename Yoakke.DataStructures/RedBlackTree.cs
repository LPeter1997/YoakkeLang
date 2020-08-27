using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Yoakke.DataStructures
{
    public class RedBlackTree<TKey, TValue>
    {
        private enum Color { Red, Black }

        private class Node
        {
            public bool IsNil => 
                Left == null && Right == null && Color == Color.Black && !HasValue;

            public Node? Parent { get; set; }
            public Node? Left { get; set; }
            public Node? Right { get; set; }

            public Color Color { get; set; } = Color.Black;
            public bool HasValue { get; private set; } = false;
            public TValue? Value { get; private set; }

            public Node? Sibling => Parent?.Left == this ? Parent?.Right : Parent?.Left;
            public Node? Uncle => Parent?.Sibling;

            // TODO: Debug only
            public string ToJSON()
            {
                if (IsNil) return "null";
                Debug.Assert(Left != null);
                Debug.Assert(Right != null);
                Debug.Assert(Value != null);
                return $"{{ \"value\": {Value}, \"color\": \"{(Color == Color.Black ? "black" : "red")}\", \"left\": {Left.ToJSON()}, \"right\": {Right.ToJSON()} }}";
            }

            public void AssignValue(TValue value)
            {
                Debug.Assert(!HasValue);
                HasValue = true;
                Value = value;
            }
        }

        private Node root;
        private Func<TValue, TKey> keySelector;
        private IComparer<TKey> comparer;

        public RedBlackTree()
        {
            root = new Node();
            // TODO: Let these be passed as an argument
            keySelector = x => (TKey)(object?)x ?? throw new NotImplementedException();
            comparer = Comparer<TKey>.Default;
        }

        // TODO: Debug only
        public string ToJSON() => root.ToJSON();

        public void Insert(TValue value)
        {
            var node = InsertLikeBinarySearchTree(value);
            RebalanceInsertion(node);
        }

        private Node InsertLikeBinarySearchTree(TValue value)
        {
            var key = keySelector(value);
            Node? prev = null;
            var current = root;
            while (current.HasValue)
            {
                // A node which has a value must have two children, as it's not a leaf node
                Debug.Assert(current.Value != null);
                Debug.Assert(current.Left != null);
                Debug.Assert(current.Right != null);

                prev = current;
                var cmp = comparer.Compare(key, keySelector(current.Value));
                current = cmp < 0 ? current.Left : current.Right;
            }
            // Now current must be a nil leaf
            Debug.Assert(current.IsNil);
            // Now we must insert the value and make it red
            current.AssignValue(value);
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
                var grandparent = node.Parent.Parent;
                Debug.Assert(grandparent != null);
                grandparent.Color = Color.Red;
                RebalanceInsertion(grandparent);
                return;
            }
            // Case 4
            var p = node.Parent;
            var g = p.Parent;
            Debug.Assert(g != null);
            if (node == p.Right && p == g.Left)
            {
                Debug.Assert(node.Left != null);
                RotateLeft(p);
                node = node.Left;
            }
            else if (node == p.Left && p == g.Right)
            {
                Debug.Assert(node.Right != null);
                RotateRight(p);
                node = node.Right;
            }
            p = node.Parent;
            Debug.Assert(p != null);
            g = p.Parent;
            Debug.Assert(g != null);
            if (node == p.Left) RotateRight(g);
            else RotateLeft(g);
            p.Color = Color.Black;
            g.Color = Color.Red;
        }

        private void RotateLeft(Node n)
        {
            var nnew = n.Right;
            var p = n.Parent;
            Debug.Assert(nnew != null);

            n.Right = nnew.Left;
            nnew.Left = n;
            n.Parent = nnew;

            if (n.Right != null) n.Right.Parent = n;

            if (p != null)
            {
                if (n == p.Left) p.Left = nnew;
                else if (n == p.Right) p.Right = nnew;
            }

            nnew.Parent = p;
        }

        private void RotateRight(Node n)
        {
            var nnew = n.Left;
            var p = n.Parent;
            Debug.Assert(nnew != null);

            n.Left = nnew.Right;
            nnew.Right = n;
            n.Parent = nnew;

            if (n.Left != null) n.Left.Parent = n;

            if (p != null)
            {
                if (n == p.Left) p.Left = nnew;
                else if (n == p.Right) p.Right = nnew;
            }

            nnew.Parent = p;
        }
    }
}
