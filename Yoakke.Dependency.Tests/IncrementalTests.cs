using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Dependency.Tests
{
    [InputQueryGroup]
    partial interface IIncrementalInputs
    {
        public int SomeConstant { get; set; }
        public string SomeValue(string k1);
    }

    [QueryGroup]
    partial interface IIncrementalQuery
    {
        public int CalculatedValue { get; }
        public string CalculateFoo(string k1, int k2);
    }

    class MyIncrementalQuery : IIncrementalQuery
    {
        public int CalculatedValue_invocations = 0;
        public int CalculateFoo_invocations = 0;

        [QueryGroup]
        public IIncrementalInputs Inputs { get; set; }

        public int CalculatedValue
        {
            get
            {
                ++CalculatedValue_invocations;
                return Inputs.SomeConstant * 3;
            }
        }

        public string CalculateFoo(string k1, int k2)
        {
            ++CalculateFoo_invocations;
            return $"{Inputs.SomeValue(k1)}_{Inputs.SomeConstant * k2}";
        }
    }

    [TestClass]
    public class IncrementalTests
    {
        [TestMethod]
        public void NoRecalculationWhenNoInputChange()
        {
            var derived = new MyIncrementalQuery();
            var system = new DependencySystem()
                .Register<IIncrementalInputs>()
                .Register<IIncrementalQuery>(() => derived);

            system.Get<IIncrementalInputs>().SomeConstant = 7;
            system.Get<IIncrementalInputs>().SetSomeValue("abc", "xyz");

            // First invocation should cause a recomputation
            var _1 = system.Get<IIncrementalQuery>().CalculatedValue;
            var _2 = system.Get<IIncrementalQuery>().CalculateFoo("abc", 4);
            Assert.AreEqual(derived.CalculatedValue_invocations, 1);
            Assert.AreEqual(derived.CalculateFoo_invocations, 1);

            // Next recomputation should not
            var _3 = system.Get<IIncrementalQuery>().CalculatedValue;
            var _4 = system.Get<IIncrementalQuery>().CalculateFoo("abc", 4);
            Assert.AreEqual(derived.CalculatedValue_invocations, 1);
            Assert.AreEqual(derived.CalculateFoo_invocations, 1);
        }

        [TestMethod]
        public void RecalculationWhenInputChange()
        {
            var derived = new MyIncrementalQuery();
            var system = new DependencySystem()
                .Register<IIncrementalInputs>()
                .Register<IIncrementalQuery>(() => derived);

            system.Get<IIncrementalInputs>().SomeConstant = 7;
            system.Get<IIncrementalInputs>().SetSomeValue("abc", "xyz");

            // First invocation should cause a recomputation
            var _1 = system.Get<IIncrementalQuery>().CalculatedValue;
            var _2 = system.Get<IIncrementalQuery>().CalculateFoo("abc", 4);
            Assert.AreEqual(derived.CalculatedValue_invocations, 1);
            Assert.AreEqual(derived.CalculateFoo_invocations, 1);

            // Next recomputation should not
            var _3 = system.Get<IIncrementalQuery>().CalculatedValue;
            var _4 = system.Get<IIncrementalQuery>().CalculateFoo("abc", 4);
            Assert.AreEqual(derived.CalculatedValue_invocations, 1);
            Assert.AreEqual(derived.CalculateFoo_invocations, 1);

            // Changing again should
            system.Get<IIncrementalInputs>().SomeConstant = 6;
            system.Get<IIncrementalInputs>().SetSomeValue("abc", "xyw");
            var _5 = system.Get<IIncrementalQuery>().CalculatedValue;
            var _6 = system.Get<IIncrementalQuery>().CalculateFoo("abc", 4);
            Assert.AreEqual(derived.CalculatedValue_invocations, 1);
            Assert.AreEqual(derived.CalculateFoo_invocations, 1);
        }
    }
}
