using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir.Instructions;
using Yoakke.Lir.Types;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Tests
{
    [TestClass]
    public class ComplexTests : TestBase
    {
        [TestMethod]
        public void RecursiveFactorial()
        {
            var b = GetBuilder(Type.I32);
            var func = b.CurrentProc;
            var p = b.DefineParameter(Type.I32);
            var lastBlock = b.CurrentBasicBlock;
            var lessThan3Block = b.DefineBasicBlock("less_than_3");
            var elseBlock = b.DefineBasicBlock("els");

            b.CurrentBasicBlock = lastBlock;
            b.JmpIf(b.Cmp(Comparison.le, p, Type.I32.NewValue(3)), lessThan3Block, elseBlock);

            b.CurrentBasicBlock = lessThan3Block;
            b.Ret(p);

            b.CurrentBasicBlock = elseBlock;
            b.Ret(b.Mul(p, b.Call(func, new List<Value> { b.Sub(p, Type.I32.NewValue(1)) })));

            TestOnAllBackends<Func<Int32, Int32>>(b, Type.I32.NewValue(1), Type.I32.NewValue(1));
            TestOnAllBackends<Func<Int32, Int32>>(b, Type.I32.NewValue(2), Type.I32.NewValue(2));
            TestOnAllBackends<Func<Int32, Int32>>(b, Type.I32.NewValue(6), Type.I32.NewValue(3));
            TestOnAllBackends<Func<Int32, Int32>>(b, Type.I32.NewValue(24), Type.I32.NewValue(4));
            TestOnAllBackends<Func<Int32, Int32>>(b, Type.I32.NewValue(120), Type.I32.NewValue(5));
        }
    }
}
