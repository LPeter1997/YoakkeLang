using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Yoakke.Lir.Backend;
using Yoakke.Lir.Backend.Toolchain;
using Yoakke.Lir.Runtime;
using Yoakke.Lir.Types;
using Yoakke.Lir.Utils;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Tests
{
    [TestClass]
    public class BasicTests : TestBase
    {
        private Builder GetBuilder() => GetBuilder(Type.I32);
        private void TestOnAllBackends(Builder b, Value v) => TestOnAllBackends<Func<Int32>>(b, v);
        
        [TestMethod]
        public void ReturnConstant()
        {
            var b = GetBuilder();
            b.Ret(Type.I32.NewValue(263));
            TestOnAllBackends(b, Type.I32.NewValue(263));
        }

        [TestMethod]
        public void ReturnParameter()
        {
            var b = GetBuilder();
            var entry = b.CurrentProc;

            var identity = b.DefineProc("identity");
            identity.Return = Type.I32;
            var p = b.DefineParameter(Type.I32);
            b.Ret(p);

            b.CurrentProc = entry;
            b.Ret(b.Call(identity, new List<Value> { Type.I32.NewValue(524) }));
            TestOnAllBackends(b, Type.I32.NewValue(524));
        }

        [TestMethod]
        public void ModifyParameter()
        {
            var intPtr = new Type.Ptr(Type.I32);
            var b = GetBuilder();
            var entry = b.CurrentProc;

            var modify = b.DefineProc("modify");
            var p = b.DefineParameter(intPtr);
            b.Store(p, Type.I32.NewValue(73));
            b.Ret();

            b.CurrentProc = entry;
            var storage = b.Alloc(Type.I32);
            b.Store(storage, Type.I32.NewValue(62));
            b.Call(modify, new List<Value> { storage });
            b.Ret(b.Load(storage));
            TestOnAllBackends(b, Type.I32.NewValue(73));
        }

        [TestMethod]
        public void IfElseThenPart()
        {
            var b = GetBuilder();

            var lastBlock = b.CurrentBasicBlock;
            var thenBlock = b.DefineBasicBlock("then");
            b.Ret(Type.I32.NewValue(36));

            var elsBlock = b.DefineBasicBlock("els");
            b.Ret(Type.I32.NewValue(383));

            b.CurrentBasicBlock = lastBlock;
            b.JmpIf(Type.I32.NewValue(1), thenBlock, elsBlock);

            TestOnAllBackends(b, Type.I32.NewValue(36));
        }

        [TestMethod]
        public void IfElseElsePart()
        {
            var b = GetBuilder();

            var lastBlock = b.CurrentBasicBlock;
            var thenBlock = b.DefineBasicBlock("then");
            b.Ret(Type.I32.NewValue(36));

            var elsBlock = b.DefineBasicBlock("els");
            b.Ret(Type.I32.NewValue(383));

            b.CurrentBasicBlock = lastBlock;
            b.JmpIf(Type.I32.NewValue(0), thenBlock, elsBlock);

            TestOnAllBackends(b, Type.I32.NewValue(383));
        }

        [TestMethod]
        public void CmpEqTrue()
        {
            var b = GetBuilder();
            b.Ret(b.CmpEq(Type.I32.NewValue(7), Type.I32.NewValue(7)));
            TestOnAllBackends(b, Type.I32.NewValue(1));
        }

        [TestMethod]
        public void CmpEqFalse()
        {
            var b = GetBuilder();
            b.Ret(b.CmpEq(Type.I32.NewValue(62), Type.I32.NewValue(25)));
            TestOnAllBackends(b, Type.I32.NewValue(0));
        }

        [TestMethod]
        public void CmpNeTrue()
        {
            var b = GetBuilder();
            b.Ret(b.CmpNe(Type.I32.NewValue(15), Type.I32.NewValue(27)));
            TestOnAllBackends(b, Type.I32.NewValue(1));
        }

        [TestMethod]
        public void CmpNeFalse()
        {
            var b = GetBuilder();
            b.Ret(b.CmpNe(Type.I32.NewValue(16), Type.I32.NewValue(16)));
            TestOnAllBackends(b, Type.I32.NewValue(0));
        }

        [TestMethod]
        public void CmpGrTrue()
        {
            var b = GetBuilder();
            b.Ret(b.CmpGr(Type.I32.NewValue(273), Type.I32.NewValue(238)));
            TestOnAllBackends(b, Type.I32.NewValue(1));
        }

        [TestMethod]
        public void CmpGrFalse()
        {
            var b = GetBuilder();
            b.Ret(b.CmpGr(Type.I32.NewValue(27), Type.I32.NewValue(42)));
            TestOnAllBackends(b, Type.I32.NewValue(0));

            b = GetBuilder();
            b.Ret(b.CmpGr(Type.I32.NewValue(27), Type.I32.NewValue(27)));
            TestOnAllBackends(b, Type.I32.NewValue(0));
        }

        [TestMethod]
        public void CmpLeTrue()
        {
            var b = GetBuilder();
            b.Ret(b.CmpLe(Type.I32.NewValue(238), Type.I32.NewValue(273)));
            TestOnAllBackends(b, Type.I32.NewValue(1));
        }

        [TestMethod]
        public void CmpLeFalse()
        {
            var b = GetBuilder();
            b.Ret(b.CmpLe(Type.I32.NewValue(42), Type.I32.NewValue(27)));
            TestOnAllBackends(b, Type.I32.NewValue(0));

            b = GetBuilder();
            b.Ret(b.CmpLe(Type.I32.NewValue(27), Type.I32.NewValue(27)));
            TestOnAllBackends(b, Type.I32.NewValue(0));
        }

        [TestMethod]
        public void CmpGrEqTrue()
        {
            var b = GetBuilder();
            b.Ret(b.CmpGrEq(Type.I32.NewValue(52), Type.I32.NewValue(38)));
            TestOnAllBackends(b, Type.I32.NewValue(1));

            b = GetBuilder();
            b.Ret(b.CmpGrEq(Type.I32.NewValue(38), Type.I32.NewValue(38)));
            TestOnAllBackends(b, Type.I32.NewValue(1));
        }

        [TestMethod]
        public void CmpGrEqFalse()
        {
            var b = GetBuilder();
            b.Ret(b.CmpGrEq(Type.I32.NewValue(15), Type.I32.NewValue(438)));
            TestOnAllBackends(b, Type.I32.NewValue(0));
        }

        [TestMethod]
        public void CmpLeEqTrue()
        {
            var b = GetBuilder();
            b.Ret(b.CmpLeEq(Type.I32.NewValue(38), Type.I32.NewValue(52)));
            TestOnAllBackends(b, Type.I32.NewValue(1));

            b = GetBuilder();
            b.Ret(b.CmpLeEq(Type.I32.NewValue(38), Type.I32.NewValue(38)));
            TestOnAllBackends(b, Type.I32.NewValue(1));
        }

        [TestMethod]
        public void CmpLeEqFalse()
        {
            var b = GetBuilder();
            b.Ret(b.CmpLeEq(Type.I32.NewValue(438), Type.I32.NewValue(16)));
            TestOnAllBackends(b, Type.I32.NewValue(0));
        }

        [TestMethod]
        public void CmpGrUnsigned()
        {
            var b = GetBuilder();
            b.Ret(b.CmpGr(Type.U32.NewValue(Type.U32.MaxValue), Type.U32.NewValue(1)));
            TestOnAllBackends(b, Type.I32.NewValue(1));

            b = GetBuilder();
            b.Ret(b.CmpGr(Type.U32.NewValue(1), Type.U32.NewValue(Type.U32.MaxValue)));
            TestOnAllBackends(b, Type.I32.NewValue(0));
        }

        [TestMethod]
        public void CmpLeUnsigned()
        {
            var b = GetBuilder();
            b.Ret(b.CmpLe(Type.U32.NewValue(Type.U32.MaxValue), Type.U32.NewValue(1)));
            TestOnAllBackends(b, Type.I32.NewValue(0));

            b = GetBuilder();
            b.Ret(b.CmpLe(Type.U32.NewValue(1), Type.U32.NewValue(Type.U32.MaxValue)));
            TestOnAllBackends(b, Type.I32.NewValue(1));
        }

        [TestMethod]
        public void Add()
        {
            var b = GetBuilder();
            b.Ret(b.Add(Type.I32.NewValue(25), Type.I32.NewValue(16)));
            TestOnAllBackends(b, Type.I32.NewValue(41));
        }

        [TestMethod]
        public void Sub()
        {
            var b = GetBuilder();
            b.Ret(b.Sub(Type.I32.NewValue(25), Type.I32.NewValue(16)));
            TestOnAllBackends(b, Type.I32.NewValue(9));
        }

        [TestMethod]
        public void Mul()
        {
            var b = GetBuilder();
            b.Ret(b.Mul(Type.I32.NewValue(7), Type.I32.NewValue(3)));
            TestOnAllBackends(b, Type.I32.NewValue(21));
        }

        [TestMethod]
        public void Div()
        {
            var b = GetBuilder();
            b.Ret(b.Div(Type.I32.NewValue(30), Type.I32.NewValue(6)));
            TestOnAllBackends(b, Type.I32.NewValue(5));

            b = GetBuilder();
            b.Ret(b.Div(Type.I32.NewValue(33), Type.I32.NewValue(6)));
            TestOnAllBackends(b, Type.I32.NewValue(5));
        }

        [TestMethod]
        public void Mod()
        {
            var b = GetBuilder();
            b.Ret(b.Mod(Type.I32.NewValue(26), Type.I32.NewValue(7)));
            TestOnAllBackends(b, Type.I32.NewValue(5));

            b = GetBuilder();
            b.Ret(b.Mod(Type.I32.NewValue(21), Type.I32.NewValue(7)));
            TestOnAllBackends(b, Type.I32.NewValue(0));
        }

        [TestMethod]
        public void BitAnd()
        {
            var b = GetBuilder();
            b.Ret(b.BitAnd(Type.I32.NewValue(0b11101101000), Type.I32.NewValue(0b00110101101)));
            TestOnAllBackends(b, Type.I32.NewValue(0b00100101000));
        }

        [TestMethod]
        public void BitOr()
        {
            var b = GetBuilder();
            b.Ret(b.BitOr(Type.I32.NewValue(0b11101101000), Type.I32.NewValue(0b00110101101)));
            TestOnAllBackends(b, Type.I32.NewValue(0b11111101101));
        }

        [TestMethod]
        public void BitXor()
        {
            var b = GetBuilder();
            b.Ret(b.BitXor(Type.I32.NewValue(0b11101101000), Type.I32.NewValue(0b00110101101)));
            TestOnAllBackends(b, Type.I32.NewValue(0b11011000101));
        }

        [TestMethod]
        public void BitNot()
        {
            var b = GetBuilder();
            b.Ret(b.BitNot(Type.I32.NewValue(0b1100010001)));
            TestOnAllBackends(b, Type.I32.NewValue(-786));
        }

        [TestMethod]
        public void Shl()
        {
            var b = GetBuilder();
            b.Ret(b.Shl(Type.I32.NewValue(1), Type.I32.NewValue(3)));
            TestOnAllBackends(b, Type.I32.NewValue(8));
        }

        [TestMethod]
        public void Shr()
        {
            var b = GetBuilder();
            b.Ret(b.Shr(Type.I32.NewValue(11), Type.I32.NewValue(2)));
            TestOnAllBackends(b, Type.I32.NewValue(2));
        }

        [TestMethod]
        public void ElementPtrElement0()
        {
            var b = GetBuilder();
            var s = b.DefineStruct(new Type[] { Type.I32, Type.I32, Type.I32 });
            var sPtr = b.Alloc(s);
            b.Store(b.ElementPtr(sPtr, 0), Type.I32.NewValue(13));
            b.Store(b.ElementPtr(sPtr, 1), Type.I32.NewValue(29));
            b.Store(b.ElementPtr(sPtr, 2), Type.I32.NewValue(41));
            b.Ret(b.Load(b.ElementPtr(sPtr, 0)));
            TestOnAllBackends(b, Type.I32.NewValue(13));
        }

        [TestMethod]
        public void ElementPtrElement1()
        {
            var b = GetBuilder();
            var s = b.DefineStruct(new Type[] { Type.I32, Type.I32, Type.I32 });
            var sPtr = b.Alloc(s);
            b.Store(b.ElementPtr(sPtr, 0), Type.I32.NewValue(13));
            b.Store(b.ElementPtr(sPtr, 1), Type.I32.NewValue(29));
            b.Store(b.ElementPtr(sPtr, 2), Type.I32.NewValue(41));
            b.Ret(b.Load(b.ElementPtr(sPtr, 1)));
            TestOnAllBackends(b, Type.I32.NewValue(29));
        }

        [TestMethod]
        public void ElementPtrElement2()
        {
            var b = GetBuilder();
            var s = b.DefineStruct(new Type[] { Type.I32, Type.I32, Type.I32 });
            var sPtr = b.Alloc(s);
            b.Store(b.ElementPtr(sPtr, 0), Type.I32.NewValue(13));
            b.Store(b.ElementPtr(sPtr, 1), Type.I32.NewValue(29));
            b.Store(b.ElementPtr(sPtr, 2), Type.I32.NewValue(41));
            b.Ret(b.Load(b.ElementPtr(sPtr, 2)));
            TestOnAllBackends(b, Type.I32.NewValue(41));
        }

        [TestMethod]
        public void ArrayElement0()
        {
            var b = GetBuilder();
            var arrTy = new Type.Array(Type.I32, 3);
            var arrPtr = b.Alloc(arrTy);
            var intPtr = b.Cast(new Type.Ptr(Type.I32), arrPtr);
            for (int i = 0; i < 3; ++i)
            {
                b.Store(b.Add(intPtr, Type.I32.NewValue(i)), Type.I32.NewValue(2 * i + 1));
            }
            b.Ret(b.Load(b.Add(intPtr, Type.I32.NewValue(0))));
            TestOnAllBackends(b, Type.I32.NewValue(1));
        }

        [TestMethod]
        public void ArrayElement1()
        {
            var b = GetBuilder();
            var arrTy = new Type.Array(Type.I32, 3);
            var arrPtr = b.Alloc(arrTy);
            var intPtr = b.Cast(new Type.Ptr(Type.I32), arrPtr);
            for (int i = 0; i < 3; ++i)
            {
                b.Store(b.Add(intPtr, Type.I32.NewValue(i)), Type.I32.NewValue(2 * i + 1));
            }
            b.Ret(b.Load(b.Add(intPtr, Type.I32.NewValue(1))));
            TestOnAllBackends(b, Type.I32.NewValue(3));
        }

        [TestMethod]
        public void ArrayElement2()
        {
            var b = GetBuilder();
            var arrTy = new Type.Array(Type.I32, 3);
            var arrPtr = b.Alloc(arrTy);
            var intPtr = b.Cast(new Type.Ptr(Type.I32), arrPtr);
            for (int i = 0; i < 3; ++i)
            {
                b.Store(b.Add(intPtr, Type.I32.NewValue(i)), Type.I32.NewValue(2 * i + 1));
            }
            b.Ret(b.Load(b.Add(intPtr, Type.I32.NewValue(2))));
            TestOnAllBackends(b, Type.I32.NewValue(5));
        }

        [TestMethod]
        public void PointerPointer()
        {
            var b = GetBuilder();
            var intPtr = new Type.Ptr(Type.I32);
            var ip = b.Alloc(intPtr);
            var i = b.Alloc(Type.I32);
            b.Store(ip, i);
            b.Store(b.Load(ip), Type.I32.NewValue(123));
            b.Ret(b.Load(i));

            TestOnAllBackends(b, Type.I32.NewValue(123));
        }

        [TestMethod]
        public void GlobalStorage()
        {
            var b = GetBuilder();
            var g = b.DefineGlobal("foo", Type.I32);
            b.Store(g, Type.I32.NewValue(3745));
            b.Ret(b.Load(g));
            TestOnAllBackends(b, Type.I32.NewValue(3745));
        }
    }
}
