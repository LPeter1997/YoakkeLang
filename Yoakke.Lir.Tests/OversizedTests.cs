using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Yoakke.DataStructures;
using Yoakke.Lir.Types;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Tests
{
    [TestClass]
    public class OversizedTests : TestBase
    {
        [TestMethod]
        public void Return64BitInt()
        {
            var b = GetBuilder(Type.I64);
            b.Ret(Type.I64.NewValue(5_123_987_576));
            TestOnAllBackends<Func<Int64>>(b, Type.I64.NewValue(5_123_987_576));
        }

        [TestMethod]
        public void Return64BitIntIndirect()
        {
            var b = GetBuilder(Type.I64);
            var entry = b.CurrentProc;

            var retbig = b.DefineProc("retbig");
            retbig.Return = Type.I64;
            b.Ret(Type.I64.NewValue(9_321_897_264));
            
            b.CurrentProc = entry;
            b.Ret(b.Call(retbig, new List<Value> { }));
            TestOnAllBackends<Func<Int64>>(b, Type.I64.NewValue(9_321_897_264));
        }

        [TestMethod]
        public void BigConditionIfElseThenPartBitsLow()
        {
            var b = GetBuilder(Type.I32);

            var lastBlock = b.CurrentBasicBlock;
            var thenBlock = b.DefineBasicBlock("then");
            b.Ret(Type.I32.NewValue(36));

            var elsBlock = b.DefineBasicBlock("els");
            b.Ret(Type.I32.NewValue(383));

            b.CurrentBasicBlock = lastBlock;
            b.JmpIf(Type.I64.NewValue(0x0000000000000001), thenBlock, elsBlock);

            TestOnAllBackends<Func<Int32>>(b, Type.I32.NewValue(36));
        }

        [TestMethod]
        public void BigConditionIfElseThenPartBitsHigh()
        {
            var b = GetBuilder(Type.I32);

            var lastBlock = b.CurrentBasicBlock;
            var thenBlock = b.DefineBasicBlock("then");
            b.Ret(Type.I32.NewValue(36));

            var elsBlock = b.DefineBasicBlock("els");
            b.Ret(Type.I32.NewValue(383));

            b.CurrentBasicBlock = lastBlock;
            b.JmpIf(Type.I64.NewValue(0x0000010000000000), thenBlock, elsBlock);

            TestOnAllBackends<Func<Int32>>(b, Type.I32.NewValue(36));
        }

        [TestMethod]
        public void BigConditionIfElseElsePart()
        {
            var b = GetBuilder(Type.I32);

            var lastBlock = b.CurrentBasicBlock;
            var thenBlock = b.DefineBasicBlock("then");
            b.Ret(Type.I32.NewValue(36));

            var elsBlock = b.DefineBasicBlock("els");
            b.Ret(Type.I32.NewValue(383));

            b.CurrentBasicBlock = lastBlock;
            b.JmpIf(Type.I64.NewValue(0), thenBlock, elsBlock);

            TestOnAllBackends<Func<Int32>>(b, Type.I32.NewValue(383));
        }

        [TestMethod]
        public void ModifyBigParameter()
        {
            var intPtr = new Type.Ptr(Type.I64);
            var b = GetBuilder(Type.I64);
            var entry = b.CurrentProc;

            var modify = b.DefineProc("modify");
            var p = b.DefineParameter(intPtr);
            b.Store(p, Type.I64.NewValue(8_724_105_835));
            b.Ret();

            b.CurrentProc = entry;
            var storage = b.Alloc(Type.I64);
            b.Store(storage, Type.I64.NewValue(1_176_864_900));
            b.Call(modify, new List<Value> { storage });
            b.Ret(b.Load(storage));
            TestOnAllBackends<Func<Int64>>(b, Type.I64.NewValue(8_724_105_835));
        }

        [TestMethod]
        public void BigCmpEqTrue()
        {
            var b = GetBuilder(Type.I32);
            b.Ret(b.CmpEq(Type.I64.NewValue(0x17ab35c7b8893d4e), Type.I64.NewValue(0x17ab35c7b8893d4e)));
            TestOnAllBackends<Func<Int32>>(b, Type.I32.NewValue(1));
        }

        [TestMethod]
        public void BigCmpEqFalseDiffHi()
        {
            var b = GetBuilder(Type.I32);
            b.Ret(b.CmpEq(Type.I64.NewValue(0x17eb35c7b8893d4e), Type.I64.NewValue(0x17ab35c7b8893d4e)));
            TestOnAllBackends<Func<Int32>>(b, Type.I32.NewValue(0));
        }

        [TestMethod]
        public void BigCmpEqFalseDiffLo()
        {
            var b = GetBuilder(Type.I32);
            b.Ret(b.CmpEq(Type.I64.NewValue(0x17ab35c7b889344e), Type.I64.NewValue(0x17ab35c7b8893d4e)));
            TestOnAllBackends<Func<Int32>>(b, Type.I32.NewValue(0));
        }

        [TestMethod]
        public void BigCmpNeTrueDiffHi()
        {
            var b = GetBuilder(Type.I32);
            b.Ret(b.CmpNe(Type.I64.NewValue(0x17eb35c7b8893d4e), Type.I64.NewValue(0x17ab35c7b8893d4e)));
            TestOnAllBackends<Func<Int32>>(b, Type.I32.NewValue(1));
        }

        [TestMethod]
        public void BigCmpNeTrueDiffLo()
        {
            var b = GetBuilder(Type.I32);
            b.Ret(b.CmpNe(Type.I64.NewValue(0x17ab35c7b889344e), Type.I64.NewValue(0x17ab35c7b8893d4e)));
            TestOnAllBackends<Func<Int32>>(b, Type.I32.NewValue(1));
        }

        [TestMethod]
        public void BigCmpNeFalse()
        {
            var b = GetBuilder(Type.I32);
            b.Ret(b.CmpNe(Type.I64.NewValue(0x17ab35c7b8893d4e), Type.I64.NewValue(0x17ab35c7b8893d4e)));
            TestOnAllBackends<Func<Int32>>(b, Type.I32.NewValue(0));
        }

        [TestMethod]
        public void BigCmpGrTrueDiffHi()
        {
            var b = GetBuilder(Type.I32);
            b.Ret(b.CmpGr(Type.I64.NewValue(0x17ac35c7b8893d4e), Type.I64.NewValue(0x17ab35c7b8893d4e)));
            TestOnAllBackends<Func<Int32>>(b, Type.I32.NewValue(1));
        }

        [TestMethod]
        public void BigCmpGrTrueDiffLo()
        {
            var b = GetBuilder(Type.I32);
            b.Ret(b.CmpGr(Type.I64.NewValue(0x17ab35c7b8894d4e), Type.I64.NewValue(0x17ab35c7b8893d4e)));
            TestOnAllBackends<Func<Int32>>(b, Type.I32.NewValue(1));
        }

        [TestMethod]
        public void BigCmpGrFalseDiffHi()
        {
            var b = GetBuilder(Type.I32);
            b.Ret(b.CmpEq(Type.I64.NewValue(0x17ab35c7b8893d4e), Type.I64.NewValue(0x19ab35c7b8893d4e)));
            TestOnAllBackends<Func<Int32>>(b, Type.I32.NewValue(0));
        }

        [TestMethod]
        public void BigCmpGrFalseDiffLo()
        {
            var b = GetBuilder(Type.I32);
            b.Ret(b.CmpEq(Type.I64.NewValue(0x17ab35c7b8893d4e), Type.I64.NewValue(0x17ab35c7b8893d5e)));
            TestOnAllBackends<Func<Int32>>(b, Type.I32.NewValue(0));
        }

        [TestMethod]
        public void BigCmpLeTrueDiffHi()
        {
            var b = GetBuilder(Type.I32);
            b.Ret(b.CmpLe(Type.I64.NewValue(0x17ab35c7b8893d4e), Type.I64.NewValue(0x17ac35c7b8893d4e)));
            TestOnAllBackends<Func<Int32>>(b, Type.I32.NewValue(1));
        }

        [TestMethod]
        public void BigCmpLeTrueDiffLo()
        {
            var b = GetBuilder(Type.I32);
            b.Ret(b.CmpLe(Type.I64.NewValue(0x17ab35c7b8893d4e), Type.I64.NewValue(0x17ab35c7b8894d4e)));
            TestOnAllBackends<Func<Int32>>(b, Type.I32.NewValue(1));
        }

        [TestMethod]
        public void BigCmpLeFalseDiffHi()
        {
            var b = GetBuilder(Type.I32);
            b.Ret(b.CmpLe(Type.I64.NewValue(0x19ab35c7b8893d4e), Type.I64.NewValue(0x17ab35c7b8893d4e)));
            TestOnAllBackends<Func<Int32>>(b, Type.I32.NewValue(0));
        }

        [TestMethod]
        public void BigCmpLeFalseDiffLo()
        {
            var b = GetBuilder(Type.I32);
            b.Ret(b.CmpLe(Type.I64.NewValue(0x17ab35c7b8893d5e), Type.I64.NewValue(0x17ab35c7b8893d4e)));
            TestOnAllBackends<Func<Int32>>(b, Type.I32.NewValue(0));
        }

        [TestMethod]
        public void BigCmpGrTrueAssym()
        {
            var b = GetBuilder(Type.I32);
            b.Ret(b.CmpGr(Type.I64.NewValue(0x18ab35c7b8893a4e), Type.I64.NewValue(0x17ab35c7b8893d4e)));
            TestOnAllBackends<Func<Int32>>(b, Type.I32.NewValue(1));
        }

        [TestMethod]
        public void BigCmpGrFalseAssym()
        {
            var b = GetBuilder(Type.I32);
            b.Ret(b.CmpGr(Type.I64.NewValue(0x16ab35c7b8893dae), Type.I64.NewValue(0x17ab35c7b8893d4e)));
            TestOnAllBackends<Func<Int32>>(b, Type.I32.NewValue(0));
        }

        [TestMethod]
        public void BigCmpLeTrueAssym()
        {
            var b = GetBuilder(Type.I32);
            b.Ret(b.CmpLe(Type.I64.NewValue(0x17ab35c7b8893d4e), Type.I64.NewValue(0x18ab35c7b8893a4e)));
            TestOnAllBackends<Func<Int32>>(b, Type.I32.NewValue(1));
        }

        [TestMethod]
        public void BigCmpLeFalseAssym()
        {
            var b = GetBuilder(Type.I32);
            b.Ret(b.CmpLe(Type.I64.NewValue(0x17ab35c7b8893d4e), Type.I64.NewValue(0x16ab35c7b8893dae)));
            TestOnAllBackends<Func<Int32>>(b, Type.I32.NewValue(0));
        }

        [TestMethod]
        public void BigAdd()
        {
            var b = GetBuilder(Type.I64);
            b.Ret(b.Add(Type.I64.NewValue(0x14ba5cefd35aa361), Type.I64.NewValue(0x0fba5cefb3540361)));
            var res = new BigInt(true, 64, 0x14ba5cefd35aa361) + new BigInt(true, 64, 0x0fba5cefb3540361);
            TestOnAllBackends<Func<Int64>>(b, Type.I64.NewValue(res));
        }

        [TestMethod]
        public void BigSub()
        {
            var b = GetBuilder(Type.I64);
            b.Ret(b.Sub(Type.I64.NewValue(0x14ba5cefd35aa361), Type.I64.NewValue(0x0fba5cefb3540361)));
            var res = new BigInt(true, 64, 0x14ba5cefd35aa361) - new BigInt(true, 64, 0x0fba5cefb3540361);
            TestOnAllBackends<Func<Int64>>(b, Type.I64.NewValue(res));
        }

        [TestMethod]
        public void BigBitAnd()
        {
            var b = GetBuilder(Type.I64);
            b.Ret(b.BitAnd(Type.I64.NewValue(0x04813baa599ef4a0), Type.I64.NewValue(0x13b1b58ac58ade82)));
            TestOnAllBackends<Func<Int64>>(b, Type.I64.NewValue(0x81318a418ad480));
        }

        [TestMethod]
        public void BigBitOr()
        {
            var b = GetBuilder(Type.I64);
            b.Ret(b.BitOr(Type.I64.NewValue(0x04813baa599ef4a0), Type.I64.NewValue(0x13b1b58ac58ade82)));
            TestOnAllBackends<Func<Int64>>(b, Type.I64.NewValue(0x17b1bfaadd9efea2));
        }

        [TestMethod]
        public void BigBitXor()
        {
            var b = GetBuilder(Type.I64);
            b.Ret(b.BitXor(Type.I64.NewValue(0x04813baa599ef4a0), Type.I64.NewValue(0x13b1b58ac58ade82)));
            TestOnAllBackends<Func<Int64>>(b, Type.I64.NewValue(0x17308e209c142a22));
        }

        [TestMethod]
        public void BigBitNot()
        {
            var b = GetBuilder(Type.I64);
            b.Ret(b.BitNot(Type.I64.NewValue(0x04813baa599ef4a0)));
            // TODO: Is this correct? Two's complement is confusing
            TestOnAllBackends<Func<Int64>>(b, Type.I64.NewValue(-0x4813baa599ef4a1));
        }
    }
}
