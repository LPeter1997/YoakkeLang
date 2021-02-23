using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Dependency.Tests
{
    // a   b   c
    //  \ / \ /
    //   v1  v2
    //    \ /
    //     v3

    [InputQueryGroup]
    partial interface INumberInputs
    {
        public int Variable(string name);
    }

    [QueryGroup]
    partial interface IBasicArithmetic
    {
        public int V1 { get; }
        public int V2 { get; }
        public int V3(int add);
    }

    class MyBasicArithmetic : IBasicArithmetic
    {
        public int V1_invocations = 0;
        public int V2_invocations = 0;
        public Dictionary<int, int> V3_invocations = new Dictionary<int, int>();

        [QueryGroup]
        public INumberInputs Inputs { get; set; }

        [QueryGroup]
        public IBasicArithmetic Arithmetic { get; set; }

        public int V1
        {
            get
            {
                ++V1_invocations;
                return Inputs.Variable("a") + Inputs.Variable("b");
            }
        }
        public int V2
        {
            get
            {
                ++V2_invocations;
                return Inputs.Variable("b") * Inputs.Variable("c");
            }
        }
        public int V3(int add)
        {
            if (!V3_invocations.TryGetValue(add, out var n)) n = 0;
            ++n;
            V3_invocations[add] = n;
            return Arithmetic.V2 * add;
        }
    }

    [TestClass]
    public class BasicArithmeticTests
    {
        [TestMethod]
        public void FullComputationOnce()
        {
            var arith = new MyBasicArithmetic();
            var system = new DependencySystem()
                .Register<INumberInputs>()
                .Register<IBasicArithmetic>(() => arith);

            system.Get<INumberInputs>().SetVariable("a", 1);
            system.Get<INumberInputs>().SetVariable("b", 2);
            system.Get<INumberInputs>().SetVariable("c", 3);

            Assert.AreEqual(3, system.Get<IBasicArithmetic>().V1);
            Assert.AreEqual(6, system.Get<IBasicArithmetic>().V3(1));
            Assert.AreEqual(1, arith.V1_invocations);
            Assert.AreEqual(1, arith.V2_invocations);
            Assert.AreEqual(1, arith.V3_invocations[1]);
            Assert.AreEqual(6, system.Get<IBasicArithmetic>().V2);
            Assert.AreEqual(1, arith.V2_invocations);
        }

        [TestMethod]
        public void UpdateB()
        {
            var arith = new MyBasicArithmetic();
            var system = new DependencySystem()
                .Register<INumberInputs>()
                .Register<IBasicArithmetic>(() => arith);

            system.Get<INumberInputs>().SetVariable("a", 1);
            system.Get<INumberInputs>().SetVariable("b", 2);
            system.Get<INumberInputs>().SetVariable("c", 3);

            // Now force-eval everyone
            var _1 = system.Get<IBasicArithmetic>().V1;
            var _2 = system.Get<IBasicArithmetic>().V3(1);

            // Update b
            system.Get<INumberInputs>().SetVariable("b", 3);

            // Everything should be computed twice
            Assert.AreEqual(4, system.Get<IBasicArithmetic>().V1);
            Assert.AreEqual(9, system.Get<IBasicArithmetic>().V3(1));
            Assert.AreEqual(2, arith.V1_invocations);
            Assert.AreEqual(2, arith.V2_invocations);
            Assert.AreEqual(2, arith.V3_invocations[1]);
            Assert.AreEqual(9, system.Get<IBasicArithmetic>().V2);
            Assert.AreEqual(2, arith.V2_invocations);
        }

        [TestMethod]
        public void FlipBandCEarlyTerminatesV3()
        {
            var arith = new MyBasicArithmetic();
            var system = new DependencySystem()
                .Register<INumberInputs>()
                .Register<IBasicArithmetic>(() => arith);

            system.Get<INumberInputs>().SetVariable("a", 1);
            system.Get<INumberInputs>().SetVariable("b", 2);
            system.Get<INumberInputs>().SetVariable("c", 3);

            // Now force-eval everyone
            var _1 = system.Get<IBasicArithmetic>().V1;
            var _2 = system.Get<IBasicArithmetic>().V3(1);

            // Swap values of b and c
            system.Get<INumberInputs>().SetVariable("b", 3);
            system.Get<INumberInputs>().SetVariable("c", 2);

            // v1 and v2 should be computed twice, v3 shouldn't be re-computed
            Assert.AreEqual(4, system.Get<IBasicArithmetic>().V1);
            Assert.AreEqual(6, system.Get<IBasicArithmetic>().V3(1));
            Assert.AreEqual(2, arith.V1_invocations);
            Assert.AreEqual(2, arith.V2_invocations);
            Assert.AreEqual(1, arith.V3_invocations[1]);
            Assert.AreEqual(6, system.Get<IBasicArithmetic>().V2);
        }
    }
}
