using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Yoakke.Compiler.Tests
{
    // https://github.com/LPeter1997/YoakkeLang/issues/4

    [TestClass]
    public class Issue0004 : TestBase
    {
        [TestMethod]
        public void Test()
        {
            string ykSource = @"
            const bar = proc(T: type) -> var {
                proc(x: T) -> T { x }
            };
            const foo = proc() -> i32 {
                const Value = bar(i32)(5);
                Value
            };
            ";
            var f = CompileAndLoadFunc<Func<Int32>>(ykSource);
            Assert.AreEqual(f(), 5);
        }
    }
}
