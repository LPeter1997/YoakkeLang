using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Yoakke.DataStructures.Tests
{
    [TestClass]
    public class BigIntTests
    {
        [TestMethod]
        public void CreateZero()
        {
            var i = new BigInt(false, 64);
            Assert.AreEqual(i.AsMemory().Length, 8);
            Assert.IsTrue(i.IsZero);
            Assert.IsTrue(i.IsEven);
            Assert.IsFalse(i.IsOdd);
        }

        [TestMethod]
        public void CreateAllOnes()
        {
            var i = BigInt.AllOnes(false, 32);
            Assert.AreEqual(i.AsMemory().Length, 4);
            Assert.IsTrue(i.IsOdd);
            Assert.IsTrue(MemoryMarshal.ToEnumerable(i.AsMemory()).All(b => b == 255));
            Assert.AreEqual(-1, (int)i);
            Assert.AreEqual(uint.MaxValue, (uint)i);
        }

        [TestMethod]
        public void NegativeOne()
        {
            var i = new BigInt(false, 32, -1);
            Assert.AreEqual(i.AsMemory().Length, 4);
            Assert.IsTrue(MemoryMarshal.ToEnumerable(i.AsMemory()).All(b => b == 255));
            Assert.AreEqual(BigInt.AllOnes(false, 32), i);
        }

        [TestMethod]
        public void ManipulateBytes()
        {
            var i = new BigInt(false, 8);
            i[0] = true;
            i[2] = true;
            Assert.IsTrue(i[0]);
            Assert.IsFalse(i[1]);
            Assert.IsTrue(i[2]);
            Assert.AreEqual(5, (int)i);

            i[7] = true;
            Assert.AreEqual(-123, (int)i);
        }

        [TestMethod]
        public void Boundlaries()
        {
            Assert.AreEqual(long.MinValue, (long)BigInt.MinValue(true, 64));
            Assert.AreEqual(long.MaxValue, (long)BigInt.MaxValue(true, 64));
            Assert.AreEqual(ulong.MinValue, (ulong)BigInt.MinValue(false, 64));
            Assert.AreEqual(ulong.MaxValue, (ulong)BigInt.MaxValue(false, 64));
        }

        [TestMethod]
        public void BinaryNot()
        {
            var i = new BigInt(false, 8, 0b10011010);
            i = ~i;
            Assert.AreEqual((byte)0b01100101, (byte)i);
        }

        [TestMethod]
        public void TwosComplement()
        {
            var i = new BigInt(false, 16, 237);
            i = -i;
            Assert.AreEqual((ushort)0b1111111100010011, (ushort)i);
        }

        [TestMethod]
        public void SignedArithmetic()
        {
            var ns = new int[] {
                -758373627, -971429535, -196760301, -308974598, 534580882, 74251856, -698399917, 
                947149041, 548737911, -993708156, 713740392, -993670816, 893512852, 497526774, 
                542371407, -972543078, -106116221, -165481548, 315734702, 396249911, 263831299, 
                -295630055, -685275396, 881986713, -893219011, -989181881, -925563361, 226807330, 
                -74527356, -730197948,
            };
            foreach (var n1 in ns)
            {
                foreach (var n2 in ns)
                {
                    var b1 = new BigInt(true, 32, n1);
                    var b2 = new BigInt(true, 32, n2);

                    Assert.AreEqual(unchecked(n1 + n2), (int)(b1 + b2), $"{n1} + {n2}");
                    Assert.AreEqual(unchecked(n1 - n2), (int)(b1 - b2), $"{n1} - {n2}");
                    Assert.AreEqual(unchecked(n1 * n2), (int)(b1 * b2), $"{n1} * {n2}");
                    Assert.AreEqual(unchecked(n1 / n2), (int)(b1 / b2), $"{n1} / {n2}");
                    Assert.AreEqual(unchecked(n1 % n2), (int)(b1 % b2), $"{n1} % {n2}");
                }
            }
        }

        [TestMethod]
        public void UnsignedArithmetic()
        {
            var ns = new int[] {
                -758373627, -971429535, -196760301, -308974598, 534580882, 74251856, -698399917,
                947149041, 548737911, -993708156, 713740392, -993670816, 893512852, 497526774,
                542371407, -972543078, -106116221, -165481548, 315734702, 396249911, 263831299,
                -295630055, -685275396, 881986713, -893219011, -989181881, -925563361, 226807330,
                -74527356, -730197948,
            };
            foreach (var n1_ in ns)
            {
                foreach (var n2_ in ns)
                {
                    var n1 = unchecked((uint)n1_);
                    var n2 = unchecked((uint)n2_);

                    var b1 = new BigInt(false, 32, n1);
                    var b2 = new BigInt(false, 32, n2);

                    Assert.AreEqual(unchecked(n1 + n2), (uint)(b1 + b2), $"{n1} + {n2}");
                    Assert.AreEqual(unchecked(n1 - n2), (uint)(b1 - b2), $"{n1} - {n2}");
                    Assert.AreEqual(unchecked(n1 * n2), (uint)(b1 * b2), $"{n1} * {n2}");
                    Assert.AreEqual(unchecked(n1 / n2), (uint)(b1 / b2), $"{n1} / {n2}");
                    Assert.AreEqual(unchecked(n1 % n2), (uint)(b1 % b2), $"{n1} % {n2}");
                }
            }
        }

        [TestMethod]
        public void SignedRelational()
        {
            var ns = new int[] {
                -758373627, -971429535, -196760301, -308974598, 534580882, 74251856, -698399917,
                947149041, 548737911, -993708156, 713740392, -993670816, 893512852, 497526774,
                542371407, -972543078, -106116221, -165481548, 315734702, 396249911, 263831299,
                -295630055, -685275396, 881986713, -893219011, -989181881, -925563361, 226807330,
                -74527356, -730197948,
            };
            foreach (var n1 in ns)
            {
                foreach (var n2 in ns)
                {
                    var b1 = new BigInt(true, 32, n1);
                    var b2 = new BigInt(true, 32, n2);

                    Assert.AreEqual(n1 == n2, b1 == b2, $"{n1} == {n2}");
                    Assert.AreEqual(n1 != n2, b1 != b2, $"{n1} != {n2}");
                    Assert.AreEqual(n1 > n2, b1 > b2, $"{n1} > {n2}");
                    Assert.AreEqual(n1 < n2, b1 < b2, $"{n1} < {n2}");
                    Assert.AreEqual(n1 >= n2, b1 >= b2, $"{n1} >= {n2}");
                    Assert.AreEqual(n1 <= n2, b1 <= b2, $"{n1} <= {n2}");
                }
            }
        }

        [TestMethod]
        public void UnsignedRelational()
        {
            var ns = new int[] {
                -758373627, -971429535, -196760301, -308974598, 534580882, 74251856, -698399917,
                947149041, 548737911, -993708156, 713740392, -993670816, 893512852, 497526774,
                542371407, -972543078, -106116221, -165481548, 315734702, 396249911, 263831299,
                -295630055, -685275396, 881986713, -893219011, -989181881, -925563361, 226807330,
                -74527356, -730197948,
            };
            foreach (var n1_ in ns)
            {
                foreach (var n2_ in ns)
                {
                    var n1 = unchecked((uint)n1_);
                    var n2 = unchecked((uint)n2_);

                    var b1 = new BigInt(false, 32, n1);
                    var b2 = new BigInt(false, 32, n2);

                    Assert.AreEqual(n1 == n2, b1 == b2, $"{n1} == {n2}");
                    Assert.AreEqual(n1 != n2, b1 != b2, $"{n1} != {n2}");
                    Assert.AreEqual(n1 > n2, b1 > b2, $"{n1} > {n2}");
                    Assert.AreEqual(n1 < n2, b1 < b2, $"{n1} < {n2}");
                    Assert.AreEqual(n1 >= n2, b1 >= b2, $"{n1} >= {n2}");
                    Assert.AreEqual(n1 <= n2, b1 <= b2, $"{n1} <= {n2}");
                }
            }
        }
    }
}
