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

        [TestMethod]
        public void LazyAndAllTrue()
        {
            string src = @"
var x = 0;
const f = proc() -> bool { x = x + 1; true };
const g = proc() -> bool { x = x + 1; true };
const entry = proc() -> i32 {
    f() && g();
    x
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(2));
        }

        [TestMethod]
        public void LazyAndFirstFalse()
        {
            string src = @"
var x = 0;
const f = proc() -> bool { x = x + 1; false };
const g = proc() -> bool { x = x + 1; true };
const entry = proc() -> i32 {
    f() && g();
    x
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(1));
        }

        [TestMethod]
        public void LazyOrAllTrue()
        {
            string src = @"
var x = 0;
const f = proc() -> bool { x = x + 1; true };
const g = proc() -> bool { x = x + 1; true };
const entry = proc() -> i32 {
    f() || g();
    x
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(1));
        }

        [TestMethod]
        public void LazyOrFirstFalse()
        {
            string src = @"
var x = 0;
const f = proc() -> bool { x = x + 1; false };
const g = proc() -> bool { x = x + 1; true };
const entry = proc() -> i32 {
    f() || g();
    x
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(2));
        }

        [TestMethod]
        public void NumericNegation()
        {
            string src = @"
const entry = proc() -> i32 {
    -123
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(-123));
        }

        [TestMethod]
        public void NumericPonoted()
        {
            string src = @"
const entry = proc() -> i32 {
    +265
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(265));
        }

        [TestMethod]
        public void BasicPointers()
        {
            string src = @"
const modify = proc(x: *i32) {
    x~ = 24;
};
const entry = proc() -> i32 {
    var x = 0;
    modify(&x);
    x
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(24));
        }

        [TestMethod]
        public void BitwiseNegation()
        {
            string src = @"
const entry = proc() -> i32 {
    !2
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(-3));
        }

        [TestMethod]
        public void PointerFromGlobal()
        {
            string src = @"
var x: i32;
const get_ptr = proc() -> *i32 {
    &x
};
const entry = proc() -> i32 {
    get_ptr()~ = 462;
    x
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(462));
        }

        [TestMethod]
        public void BasicArrays()
        {
            string src = @"
const entry = proc() -> i32 {
    var arr: [3]i32;
    arr[0] = 5;
    arr[1] = 38;
    arr[2] = 16;
    arr[0] + arr[1] - arr[2]
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(27));
        }

        [TestMethod]
        public void OutOfOrderGlobal()
        {
            string src = @"
const entry = proc() -> i32 {
    x
};

var x = 72;
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(72));
        }

        [TestMethod]
        public void AssociatedConstantProcedure()
        {
            string src = @"
const Math = struct {
    const abs = proc(x: i32) -> i32 {
        if x > 0 { x } else { -x }
    };
};
const entry = proc() -> i32 {
    Math.abs(-15)
};

";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(15));
        }

        [TestMethod]
        public void AssociatedConstantGlobal()
        {
            string src = @"
const Math = struct {
    var x = 7;
};
const entry = proc() -> i32 {
    Math.x
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(7));
        }

        [TestMethod]
        public void StringLiteralLength()
        {
            string src = @"
const entry = proc() -> u32 {
    var x = ""Hello, World!"";
    x.len
};
";
            TestOnAllBackends<Func<UInt32>>(src, Type.U32.NewValue(13));
        }

        [TestMethod]
        public void AssociatedGenericConstant()
        {
            string src = @"
const Id = proc(T: type) -> type {
    struct {
        const id = proc(x: T) -> T { x };
    }
};
const entry = proc() -> i32 {
    Id(i32).id(345)
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(345));
        }

        [TestMethod]
        public void AssociatedGenericConstantReverse()
        {
            string src = @"
const entry = proc() -> i32 {
    Id(i32).id(345)
};
const Id = proc(T: type) -> type {
    struct {
        const id = proc(x: T) -> T { x };
    }
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(345));
        }

        [TestMethod]
        public void AnonymousFunctionCalledInline()
        {
            string src = @"
const entry = proc() -> i32 {
    (proc(x: i32) -> i32 { x })(12)
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(12));
        }

        [TestMethod]
        public void DependentCall()
        {
            string src = @"
const identity = proc(T: type, x: T) -> T { x };
const entry = proc() -> i32 {
    identity(i32, 532)
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(532));
        }

        [TestMethod]
        public void DependentCallOutOfOrder()
        {
            string src = @"
const entry = proc() -> i32 {
    identity(i32, 532)
};
const identity = proc(T: type, x: T) -> T { x };
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(532));
        }

        [TestMethod]
        public void EmulatedDependentCall()
        {
            string src = @"
const entry = proc() -> i32 {
    (proc(T: type) -> type {
        struct { const f = proc(x: T) -> T { x }; }
    })(i32).f(532)
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(532));
        }

        [TestMethod]
        public void GenericPointers()
        {
            string src = @"
const ptr = proc(T: type) -> type {
    *T
};
const entry = proc() -> i32 {
    var x = 123;
    var y: ptr(i32) = &x;
    y~
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(123));
        }

        [TestMethod]
        public void RecursiveTypesCompile()
        {
            string src = @"
const A = struct {
    next: *A;
};
const entry = proc() -> i32 {
    0
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(0));
        }

        [TestMethod]
        public void StructTypeConstructor()
        {
            string src = @"
const Frac = struct {
    nom: i32;
    den: i32;

    const new = proc(x: i32, y: i32) -> Frac {
        Frac{ nom = x; den = y; }
    };
};
const entry = proc() -> i32 {
    var f = Frac.new(2, 654);
    f.den
};
";
            TestOnAllBackends<Func<Int32>>(src, Type.I32.NewValue(654));
        }
    }
}
