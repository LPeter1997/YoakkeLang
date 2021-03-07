using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Dependency.Tests
{
    // For input we will just use the INumberInputs

    [QueryGroup]
    partial interface IAsyncComputation
    {
        public Task<int> ComputeKeylessValue();
        public Task<int> ComputeKeyedValue(string s1, string s2);
    }

    class MyAsyncComputation : IAsyncComputation
    {
        [QueryGroup]
        public INumberInputs Inputs { get; set; }

        [QueryGroup]
        public IAsyncComputation Computation { get; set; }

        public Task<int> ComputeKeylessValue()
        {
            return Task.FromResult(Inputs.Variable("x") + Inputs.Variable("y"));
        }

        public async Task<int> ComputeKeyedValue(string s1, string s2)
        {
            var v1 = await Computation.ComputeKeylessValue();
            return v1 * Inputs.Variable(s1) + Inputs.Variable(s2);
        }
    }

    [TestClass]
    public class AsyncTests
    {
        [TestMethod]
        public void BasicTests()
        {
            //IAsyncComputation.Proxy;

            var system = new DependencySystem()
                .Register<INumberInputs>()
                .Register<IAsyncComputation, MyAsyncComputation>();

            system.Get<INumberInputs>().SetVariable("x", 3);
            system.Get<INumberInputs>().SetVariable("y", 4);
            system.Get<INumberInputs>().SetVariable("z", 2);
            system.Get<INumberInputs>().SetVariable("w", 9);

            Assert.AreEqual(7, system.Get<IAsyncComputation>().ComputeKeylessValue().Result);
            Assert.AreEqual(23, system.Get<IAsyncComputation>().ComputeKeyedValue("z", "w").Result);
        }
    }
}
