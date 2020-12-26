using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Yoakke.Lir.Runtime;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Tests
{
    [TestClass]
    public class UserProcTests : TestBase
    {
        private class ISqrt : IUserProc
        {
            public Type ReturnType => Type.I32;

            public Value Execute(VirtualMachine vm, IEnumerable<Values.Value> args)
            {
                var argsList = args.ToList();
                Assert.AreEqual(argsList.Count, 1);
                var arg = argsList[0];
                Assert.AreEqual(arg.Type, Type.I32);
                var argValue = (int)((Value.Int)argsList[0]).Value;
                return Type.I32.NewValue((int)Math.Sqrt(argValue));
            }
        }

        [TestMethod]
        public void CallSqrt()
        {
            var b = GetBuilder(Type.I32);
            var p = b.DefineParameter(Type.I32);
            b.Ret(b.Call(new ISqrt(), new List<Value> { p }));

            TestOnVirtualMachine(b, Type.I32.NewValue(2), Type.I32.NewValue(4));
            TestOnVirtualMachine(b, Type.I32.NewValue(3), Type.I32.NewValue(9));
            TestOnVirtualMachine(b, Type.I32.NewValue(4), Type.I32.NewValue(16));
        }
    }
}
