using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Yoakke.DataStructures.Tests
{
    [TestClass]
    public class BinaryTreeInterfaceTest
    {
        private class Node : BinaryTreeNodeBase<Node>
        {
            public override Node Parent { get; set; }
            public override Node Left { get; set; }
            public override Node Right { get; set; }
        }

        /*
                   a
                  / \
                 /   \
                b     c
               / \   / \
              d   e f   g
         */
        private Node GetTestTree()
        {
            var a = new Node();
            var b = new Node { Parent = a };
            var c = new Node { Parent = a };
            var d = new Node { Parent = b };
            var e = new Node { Parent = b };
            var f = new Node { Parent = c };
            var g = new Node { Parent = c };
            a.Left = b;
            a.Right = c;
            b.Left = d;
            b.Right = e;
            c.Left = f;
            c.Right = g;
            return a;
        }

        [TestMethod]
        public void TestRootLeftRotation()
        {
            var a = GetTestTree();
            var b = a.Left;
            var c = a.Right;
            var f = c.Left;
            var g = c.Right;
            a.RotateLeft();

            Assert.AreEqual(null, c.Parent);
            Assert.AreEqual(c, a.Parent);
            Assert.AreEqual(c, g.Parent);
            Assert.AreEqual(a, f.Parent);
            Assert.AreEqual(a, b.Parent);

            Assert.AreEqual(a, c.Left);
            Assert.AreEqual(g, c.Right);
            Assert.AreEqual(b, a.Left);
            Assert.AreEqual(f, a.Right);
        }

        [TestMethod]
        public void TestRootRightRotation()
        {
            var a = GetTestTree();
            var b = a.Left;
            var c = a.Right;
            var d = b.Left;
            var e = b.Right;
            a.RotateRight();

            Assert.AreEqual(null, b.Parent);
            Assert.AreEqual(b, a.Parent);
            Assert.AreEqual(b, d.Parent);
            Assert.AreEqual(a, e.Parent);
            Assert.AreEqual(a, c.Parent);

            Assert.AreEqual(d, b.Left);
            Assert.AreEqual(a, b.Right);
            Assert.AreEqual(e, a.Left);
            Assert.AreEqual(c, a.Right);
        }

        [TestMethod]
        public void TestLeafLeftRotation()
        {
            var a = GetTestTree();
            var c = a.Right;
            var f = c.Left;
            var g = c.Right;
            c.RotateLeft();

            Assert.AreEqual(a, g.Parent);
            Assert.AreEqual(g, c.Parent);
            Assert.AreEqual(c, f.Parent);

            Assert.AreEqual(g, a.Right);
            Assert.AreEqual(c, g.Left);
            Assert.AreEqual(null, g.Right);
            Assert.AreEqual(f, c.Left);
            Assert.AreEqual(null, c.Right);
        }

        [TestMethod]
        public void TestLeafRightRotation()
        {
            var a = GetTestTree();
            var c = a.Right;
            var f = c.Left;
            var g = c.Right;
            c.RotateRight();

            Assert.AreEqual(a, f.Parent);
            Assert.AreEqual(c, g.Parent);
            Assert.AreEqual(f, c.Parent);

            Assert.AreEqual(g, c.Right);
            Assert.AreEqual(null, f.Left);
            Assert.AreEqual(f, a.Right);
            Assert.AreEqual(c, f.Right);
            Assert.AreEqual(null, c.Left);
        }

        [TestMethod]
        public void TestPreOrder()
        {
            var a = GetTestTree();
            var b = a.Left;
            var c = a.Right;
            var d = b.Left;
            var e = b.Right;
            var f = c.Left;
            var g = c.Right;

            var pre = a.PreOrder().ToArray();
            Assert.AreEqual(a, pre[0]);
            Assert.AreEqual(b, pre[1]);
            Assert.AreEqual(d, pre[2]);
            Assert.AreEqual(e, pre[3]);
            Assert.AreEqual(c, pre[4]);
            Assert.AreEqual(f, pre[5]);
            Assert.AreEqual(g, pre[6]);
        }

        [TestMethod]
        public void TestInOrder()
        {
            var a = GetTestTree();
            var b = a.Left;
            var c = a.Right;
            var d = b.Left;
            var e = b.Right;
            var f = c.Left;
            var g = c.Right;

            var ino = a.InOrder().ToArray();
            Assert.AreEqual(d, ino[0]);
            Assert.AreEqual(b, ino[1]);
            Assert.AreEqual(e, ino[2]);
            Assert.AreEqual(a, ino[3]);
            Assert.AreEqual(f, ino[4]);
            Assert.AreEqual(c, ino[5]);
            Assert.AreEqual(g, ino[6]);
        }

        [TestMethod]
        public void TestPostOrder()
        {
            var a = GetTestTree();
            var b = a.Left;
            var c = a.Right;
            var d = b.Left;
            var e = b.Right;
            var f = c.Left;
            var g = c.Right;

            var post = a.PostOrder().ToArray();
            Assert.AreEqual(d, post[0]);
            Assert.AreEqual(e, post[1]);
            Assert.AreEqual(b, post[2]);
            Assert.AreEqual(f, post[3]);
            Assert.AreEqual(g, post[4]);
            Assert.AreEqual(c, post[5]);
            Assert.AreEqual(a, post[6]);
        }

        [TestMethod]
        public void TestSwapChildAndParent()
        {
            var a = GetTestTree();
            var b = a.Left;
            var c = a.Right;
            var d = b.Left;
            var e = b.Right;

            Node.Swap(a, b);

            Assert.AreEqual(null, b.Parent);
            Assert.AreEqual(b, a.Parent);
            Assert.AreEqual(b, c.Parent);
            Assert.AreEqual(a, d.Parent);
            Assert.AreEqual(a, e.Parent);

            Assert.AreEqual(a, b.Left);
            Assert.AreEqual(c, b.Right);
            Assert.AreEqual(d, a.Left);
            Assert.AreEqual(e, a.Right);
        }

        [TestMethod]
        public void TestSwapWithCommonAncestor()
        {
            var a = GetTestTree();
            var b = a.Left;
            var c = a.Right;
            var d = b.Left;
            var e = b.Right;
            var f = c.Left;
            var g = c.Right;

            Node.Swap(b, c);

            Assert.AreEqual(a, b.Parent);
            Assert.AreEqual(a, c.Parent);
            Assert.AreEqual(c, d.Parent);
            Assert.AreEqual(c, e.Parent);
            Assert.AreEqual(b, f.Parent);
            Assert.AreEqual(b, g.Parent);

            Assert.AreEqual(c, a.Left);
            Assert.AreEqual(b, a.Right);
            Assert.AreEqual(d, c.Left);
            Assert.AreEqual(e, c.Right);
            Assert.AreEqual(f, b.Left);
            Assert.AreEqual(g, b.Right);
        }

        [TestMethod]
        public void TestSwapUnrelated()
        {
            var a = GetTestTree();
            var b = a.Left;
            var c = a.Right;
            var d = b.Left;
            var e = b.Right;
            var f = c.Left;
            var g = c.Right;

            Node.Swap(b, g);

            Assert.AreEqual(a, g.Parent);
            Assert.AreEqual(a, c.Parent);
            Assert.AreEqual(g, d.Parent);
            Assert.AreEqual(g, e.Parent);
            Assert.AreEqual(c, f.Parent);
            Assert.AreEqual(c, b.Parent);

            Assert.AreEqual(g, a.Left);
            Assert.AreEqual(c, a.Right);
            Assert.AreEqual(f, c.Left);
            Assert.AreEqual(b, c.Right);
            Assert.AreEqual(null, b.Left);
            Assert.AreEqual(null, b.Right);
            Assert.AreEqual(d, g.Left);
            Assert.AreEqual(e, g.Right);
        }
    }
}
