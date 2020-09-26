using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Yoakke.Lir.Instructions;
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

        [TestMethod]
        public void RecursiveFactorialHighLevel()
        {
            var b = GetBuilder(Type.I32);
            var func = b.CurrentProc;
            var p = b.DefineParameter(Type.I32);

            b.IfThenElse(
                condition: b => b.Cmp(Comparison.le, p, Type.I32.NewValue(3)),
                then: b => b.Ret(p),
                @else: b => b.Ret(b.Mul(p, b.Call(func, new List<Value> { b.Sub(p, Type.I32.NewValue(1)) })))
            );
            
            TestOnAllBackends<Func<Int32, Int32>>(b, Type.I32.NewValue(1), Type.I32.NewValue(1));
            TestOnAllBackends<Func<Int32, Int32>>(b, Type.I32.NewValue(2), Type.I32.NewValue(2));
            TestOnAllBackends<Func<Int32, Int32>>(b, Type.I32.NewValue(6), Type.I32.NewValue(3));
            TestOnAllBackends<Func<Int32, Int32>>(b, Type.I32.NewValue(24), Type.I32.NewValue(4));
            TestOnAllBackends<Func<Int32, Int32>>(b, Type.I32.NewValue(120), Type.I32.NewValue(5));
        }

        [TestMethod]
        public void IterativeFactorial()
        {
            var b = GetBuilder(Type.I32);
            var p = b.DefineParameter(Type.I32);

            var begin = b.CurrentBasicBlock;
            var i = b.Alloc(Type.I32);
            var ret = b.Alloc(Type.I32);
            b.Store(i, Type.I32.NewValue(1));
            b.Store(ret, Type.I32.NewValue(1));

            var loopConditionBlock = b.DefineBasicBlock("loop_condition");
            var loopBlock = b.DefineBasicBlock("loop");
            var endLoopBlock = b.DefineBasicBlock("end_loop");

            b.CurrentBasicBlock = begin;
            b.Jmp(loopConditionBlock);

            b.CurrentBasicBlock = loopConditionBlock;
            b.JmpIf(b.CmpLeEq(b.Load(i), p), loopBlock, endLoopBlock);

            b.CurrentBasicBlock = loopBlock;
            b.Store(ret, b.Mul(b.Load(ret), b.Load(i)));
            b.Store(i, b.Add(b.Load(i), Type.I32.NewValue(1)));
            b.Jmp(loopConditionBlock);

            b.CurrentBasicBlock = endLoopBlock;
            b.Ret(b.Load(ret));

            TestOnAllBackends<Func<Int32, Int32>>(b, Type.I32.NewValue(1), Type.I32.NewValue(1));
            TestOnAllBackends<Func<Int32, Int32>>(b, Type.I32.NewValue(2), Type.I32.NewValue(2));
            TestOnAllBackends<Func<Int32, Int32>>(b, Type.I32.NewValue(6), Type.I32.NewValue(3));
            TestOnAllBackends<Func<Int32, Int32>>(b, Type.I32.NewValue(24), Type.I32.NewValue(4));
            TestOnAllBackends<Func<Int32, Int32>>(b, Type.I32.NewValue(120), Type.I32.NewValue(5));
        }

        [TestMethod]
        public void IterativeFactorialHighLevel()
        {
            var b = GetBuilder(Type.I32);
            var p = b.DefineParameter(Type.I32);

            var i = b.Alloc(Type.I32);
            var ret = b.Alloc(Type.I32);
            b.Store(i, Type.I32.NewValue(1));
            b.Store(ret, Type.I32.NewValue(1));

            b.While(
                condition: b => b.CmpLeEq(b.Load(i), p),
                body: b =>
                {
                    b.Store(ret, b.Mul(b.Load(ret), b.Load(i)));
                    b.Store(i, b.Add(b.Load(i), Type.I32.NewValue(1)));
                }
            );

            b.Ret(b.Load(ret));

            TestOnAllBackends<Func<Int32, Int32>>(b, Type.I32.NewValue(1), Type.I32.NewValue(1));
            TestOnAllBackends<Func<Int32, Int32>>(b, Type.I32.NewValue(2), Type.I32.NewValue(2));
            TestOnAllBackends<Func<Int32, Int32>>(b, Type.I32.NewValue(6), Type.I32.NewValue(3));
            TestOnAllBackends<Func<Int32, Int32>>(b, Type.I32.NewValue(24), Type.I32.NewValue(4));
            TestOnAllBackends<Func<Int32, Int32>>(b, Type.I32.NewValue(120), Type.I32.NewValue(5));
        }
    }
}
