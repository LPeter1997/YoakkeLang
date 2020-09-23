using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
