using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Yoakke.Compiler.Tests
{
    // https://github.com/LPeter1997/YoakkeLang/issues/5

    [TestClass]
    public class Issue0005 : TestBase
    {
        [TestMethod]
        public void Test()
        {
            string source = @"
const bar = proc(x: var) { };

const main = proc() -> i32 {
    bar(3);
    bar(true);
    0
};
";
            Assert.AreEqual(Compile(source), 0);
        }
    }
}
