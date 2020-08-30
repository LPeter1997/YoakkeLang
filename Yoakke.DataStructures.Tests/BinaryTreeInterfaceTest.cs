using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
    }
}
