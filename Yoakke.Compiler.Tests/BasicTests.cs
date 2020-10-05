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

        [TestMethod]
        public void ReturnGlobal()
        {
            string src = @"
var x = 673;

const entry = proc() -> i32 {
    x
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(673));
        }

        [TestMethod]
        public void RewriteGlobal()
        {
            string src = @"
var x: i32;

const entry = proc() -> i32 {
    x = 9432;
    x
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(9432));
        }

        [TestMethod]
        public void IfElseThen()
        {
            string src = @"
const entry = proc() -> i32 {
    if true { 62 } else { 176 }
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(62));
        }

        [TestMethod]
        public void IfElseElse()
        {
            string src = @"
const entry = proc() -> i32 {
    if false { 62 } else { 176 }
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(176));
        }

        [TestMethod]
        public void WhileSum1To10()
        {
            string src = @"
const entry = proc() -> i32 {
    var i = 0;
    var sum = 0;
    while i < 10 {
        i = i + 1;
        sum = sum + i;
    }
    sum
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(55));
        }

        [TestMethod]
        public void EarlyReturnFromSum()
        {
            string src = @"
const entry = proc() -> i32 {
    var i = 0;
    var sum = 0;
    while i < 10 {
        i = i + 1;
        sum = sum + i;
        if i == 5 { return sum; }
    }
    sum
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(15));
        }
    }
}
