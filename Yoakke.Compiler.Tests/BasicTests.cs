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

        [TestMethod]
        public void RecursiveFactorial()
        {
            string src = @"
const entry = proc(x: i32) -> i32 {
    if x < 3 { x } else { x * entry(x - 1) }
};
";
            TestOnAllBackends<Func<Int32, Int32>>(src, Type.I32.NewValue(1), Type.I32.NewValue(1));
            TestOnAllBackends<Func<Int32, Int32>>(src, Type.I32.NewValue(2), Type.I32.NewValue(2));
            TestOnAllBackends<Func<Int32, Int32>>(src, Type.I32.NewValue(6), Type.I32.NewValue(3));
            TestOnAllBackends<Func<Int32, Int32>>(src, Type.I32.NewValue(24), Type.I32.NewValue(4));
            TestOnAllBackends<Func<Int32, Int32>>(src, Type.I32.NewValue(120), Type.I32.NewValue(5));
        }

        [TestMethod]
        public void RecursiveFactorialConstant()
        {
            string src = @"
const factorial = proc(x: i32) -> i32 {
    if x < 3 { x } else { x * factorial(x - 1) }
};

const fact5 = factorial(5);

const entry = proc() -> i32 {
    fact5
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(120));
        }

        [TestMethod]
        public void SimpleStruct()
        {
            string src = @"
const Vector2 = struct {
    x: i32;
    y: i32;
};

const entry = proc() -> i32 {
    var x = Vector2 { x = 3; y = 4; };
    x.x
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(3));
        }

        [TestMethod]
        public void SimpleStructLvalue()
        {
            string src = @"
const Vector2 = struct {
    x: i32;
    y: i32;
};

const entry = proc() -> i32 {
    var x = Vector2 { x = 3; y = 4; };
    x.y = 632;
    x.y
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(632));
        }

        [TestMethod]
        public void NestedStruct()
        {
            string src = @"
const S1 = struct {
    f: S2;
};

const S2 = struct {
    g: i32;
};

const entry = proc() -> i32 {
    var x = S1 { f = S2 { g = 13; }; };
    x.f.g
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(13));
        }

        [TestMethod]
        public void NestedStructAssignInner()
        {
            string src = @"
const S1 = struct {
    f: S2;
};

const S2 = struct {
    g: i32;
};

const entry = proc() -> i32 {
    var x = S1 { f = S2 { g = 13; }; };
    x.f.g = 632;
    x.f.g
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(632));
        }

        [TestMethod]
        public void NestedStructAssignOuter()
        {
            string src = @"
const S1 = struct {
    f: S2;
};

const S2 = struct {
    g: i32;
};

const entry = proc() -> i32 {
    var x = S1 { f = S2 { g = 13; }; };
    x.f = S2 { g = 632; };
    x.f.g
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(632));
        }

        [TestMethod]
        public void NestedStructCopyInner()
        {
            string src = @"
const S1 = struct {
    f: S2;
};

const S2 = struct {
    g: i32;
};

const entry = proc() -> i32 {
    var x = S1 { f = S2 { g = 13; }; };
    var y = x.f;
    y.g = 27;
    x.f.g
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(13));
        }
    }
}
