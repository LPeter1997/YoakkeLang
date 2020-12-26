using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Yoakke.DataStructures.Tests
{
    [TestClass]
    public class StringMetricsTests
    {
        private int Lev(string s1, string s2)
        {
            var d1 = StringMetrics.LevenshteinDistance(s1, s2);
            var d2 = StringMetrics.LevenshteinDistance(s2, s1);
            Assert.AreEqual(d1, d2);
            return d1;
        }

        private int OSA(string s1, string s2)
        {
            var d1 = StringMetrics.OptimalStringAlignmentDistance(s1, s2);
            var d2 = StringMetrics.OptimalStringAlignmentDistance(s2, s1);
            Assert.AreEqual(d1, d2);
            return d1;
        }

        [TestMethod]
        public void LevenshteinDistanceTests()
        {
            Assert.AreEqual(3, Lev("kitten", "sitting"));
            Assert.AreEqual(2, Lev("abcd", "acbd"));
            Assert.AreEqual(3, Lev("abc", "def"));
            Assert.AreEqual(4, Lev("abcd", "defg"));
            Assert.AreEqual(2, Lev("apple", "appel"));
            Assert.AreEqual(2, Lev("mug", "gum"));
            Assert.AreEqual(3, Lev("mcdonalds", "mcdnoald"));
            Assert.AreEqual(5, Lev("table", "desk"));
            Assert.AreEqual(3, Lev("abcd", "badc"));
            Assert.AreEqual(2, Lev("book", "back"));
            Assert.AreEqual(4, Lev("pattern", "parent"));
            Assert.AreEqual(4, Lev("abcd", ""));
            Assert.AreEqual(0, Lev("abcd", "abcd"));
            Assert.AreEqual(4, Lev("abcd", "da"));
        }

        [TestMethod]
        public void OSADistanceTests()
        {
            Assert.AreEqual(3, OSA("kitten", "sitting"));
            Assert.AreEqual(1, OSA("abcd", "acbd"));
            Assert.AreEqual(3, OSA("abc", "def"));
            Assert.AreEqual(4, OSA("abcd", "defg"));
            Assert.AreEqual(1, OSA("apple", "appel"));
            Assert.AreEqual(2, OSA("mug", "gum"));
            Assert.AreEqual(2, OSA("mcdonalds", "mcdnoald"));
            Assert.AreEqual(5, OSA("table", "desk"));
            Assert.AreEqual(2, OSA("abcd", "badc"));
            Assert.AreEqual(2, OSA("book", "back"));
            Assert.AreEqual(4, OSA("abcd", ""));
            Assert.AreEqual(0, OSA("abcd", "abcd"));
            Assert.AreEqual(4, OSA("abcd", "da"));
        }
    }
}
