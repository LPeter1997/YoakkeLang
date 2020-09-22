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
    }
}
