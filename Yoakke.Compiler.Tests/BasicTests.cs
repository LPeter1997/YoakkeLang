using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Yoakke.Lir.Tests;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Compiler.Tests
{
    [TestClass]
    public class BasicTests : TestBase
    {
        [TestMethod]
        public void ReturnConstant()
        {
            string src = @"
const entry = proc() -> i32 {
    6243
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(6243));
        }
    }
}
