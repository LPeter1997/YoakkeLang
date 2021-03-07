using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Yoakke.Dependency.Tests
{
    // For input we will just use the INumberInputs

    [QueryGroup]
    partial interface IAsyncCancellableComputation
    {
        public Task<int> ComputeKeylessValue(CancellationToken cancellationToken);
        public Task<int> ComputeKeyedValue(string s1, string s2, CancellationToken cancellationToken);
    }

    class MyAsyncCancellableComputation : IAsyncCancellableComputation
    {
        public int keylessCount = 0;
        public Dictionary<(string, string), int> keyedCount = new Dictionary<(string, string), int>();

        [QueryGroup]
        public INumberInputs Inputs { get; set; }

        [QueryGroup]
        public IAsyncCancellableComputation Computation { get; set; }

        public Task<int> ComputeKeylessValue(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return Task.FromResult(0);
            ++keylessCount;
            return Task.FromResult(Inputs.Variable("x") + Inputs.Variable("y"));
        }

        public async Task<int> ComputeKeyedValue(string s1, string s2, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return 0;
            var v1 = await Computation.ComputeKeylessValue(cancellationToken);
            if (cancellationToken.IsCancellationRequested) return 0;
            int newCount = 0;
            if (keyedCount.TryGetValue((s1, s2), out var oldCount)) newCount = oldCount;
            keyedCount[(s1, s2)] = newCount + 1;
            return v1 * Inputs.Variable(s1) + Inputs.Variable(s2);
        }
    }

    [TestClass]
    public class AsyncCancellationTests
    {
        [TestMethod]
        public void TestCancellationTokenNotPartOfKey()
        {
            // ICancellableComputation.Proxy;

            var comp = new MyAsyncCancellableComputation();
            var system = new DependencySystem()
                .Register<INumberInputs>()
                .Register<IAsyncCancellableComputation>(() => comp);

            system.Get<INumberInputs>().SetVariable("x", 2);
            system.Get<INumberInputs>().SetVariable("y", 3);
            system.Get<INumberInputs>().SetVariable("z", 4);

            // Call it with one CT source
            var ctSource1 = new CancellationTokenSource();
            var v1 = system.Get<IAsyncCancellableComputation>().ComputeKeylessValue(ctSource1.Token).Result;
            var v2 = system.Get<IAsyncCancellableComputation>().ComputeKeyedValue("x", "z", ctSource1.Token).Result;
            var v3 = system.Get<IAsyncCancellableComputation>().ComputeKeyedValue("y", "z", ctSource1.Token).Result;
            Assert.AreEqual(5, v1);
            Assert.AreEqual(14, v2);
            Assert.AreEqual(19, v3);
            Assert.AreEqual(1, comp.keylessCount);
            Assert.AreEqual(1, comp.keyedCount[("x", "z")]);
            Assert.AreEqual(1, comp.keyedCount[("y", "z")]);

            // Now the other CT source
            var ctSource2 = new CancellationTokenSource();
            v1 = system.Get<IAsyncCancellableComputation>().ComputeKeylessValue(ctSource2.Token).Result;
            v2 = system.Get<IAsyncCancellableComputation>().ComputeKeyedValue("x", "z", ctSource2.Token).Result;
            v3 = system.Get<IAsyncCancellableComputation>().ComputeKeyedValue("y", "z", ctSource2.Token).Result;
            Assert.AreEqual(5, v1);
            Assert.AreEqual(14, v2);
            Assert.AreEqual(19, v3);
            // No recomputation should happen
            Assert.AreEqual(1, comp.keylessCount);
            Assert.AreEqual(1, comp.keyedCount[("x", "z")]);
            Assert.AreEqual(1, comp.keyedCount[("y", "z")]);
        }
    }
}
