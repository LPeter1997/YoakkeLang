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
    partial interface ICancellableComputation
    {
        public int ComputeKeylessValue(CancellationToken ct);
        public int ComputeKeyedValue(string s1, string s2, CancellationToken ct);
    }

    class MyCancellableComputation : ICancellableComputation
    {
        public int keylessCount = 0;
        public Dictionary<(string, string), int> keyedCount = new Dictionary<(string, string), int>();

        [QueryGroup]
        public INumberInputs Inputs { get; set; }

        public int ComputeKeylessValue(CancellationToken ct)
        {
            if (ct.IsCancellationRequested) return 0;
            ++keylessCount;
            return Inputs.Variable("x") * Inputs.Variable("y");
        }

        public int ComputeKeyedValue(string s1, string s2, CancellationToken ct)
        {
            if (ct.IsCancellationRequested) return 0;
            int newCount = 0;
            if (keyedCount.TryGetValue((s1, s2), out var oldCount)) newCount = oldCount;
            keyedCount[(s1, s2)] = newCount + 1;
            return Inputs.Variable(s1) * Inputs.Variable(s2);
        }
    }

    [TestClass]
    public class CancellationTests
    {
        [TestMethod]
        public void TestCancellationTokenNotPartOfKey()
        {
            // ICancellableComputation.Proxy;

            var comp = new MyCancellableComputation();
            var system = new DependencySystem()
                .Register<INumberInputs>()
                .Register<ICancellableComputation>(() => comp);

            system.Get<INumberInputs>().SetVariable("x", 2);
            system.Get<INumberInputs>().SetVariable("y", 3);
            system.Get<INumberInputs>().SetVariable("z", 4);

            // Call it with one CT source
            var ctSource1 = new CancellationTokenSource();
            var v1 = system.Get<ICancellableComputation>().ComputeKeylessValue(ctSource1.Token);
            var v2 = system.Get<ICancellableComputation>().ComputeKeyedValue("x", "z", ctSource1.Token);
            var v3 = system.Get<ICancellableComputation>().ComputeKeyedValue("y", "z", ctSource1.Token);
            Assert.AreEqual(6, v1);
            Assert.AreEqual(8, v2);
            Assert.AreEqual(12, v3);
            Assert.AreEqual(1, comp.keylessCount);
            Assert.AreEqual(1, comp.keyedCount[("x", "z")]);
            Assert.AreEqual(1, comp.keyedCount[("y", "z")]);

            // Now the other CT source
            var ctSource2 = new CancellationTokenSource();
            v1 = system.Get<ICancellableComputation>().ComputeKeylessValue(ctSource2.Token);
            v2 = system.Get<ICancellableComputation>().ComputeKeyedValue("x", "z", ctSource2.Token);
            v3 = system.Get<ICancellableComputation>().ComputeKeyedValue("y", "z", ctSource2.Token);
            Assert.AreEqual(6, v1);
            Assert.AreEqual(8, v2);
            Assert.AreEqual(12, v3);
            // No recomputation should happen
            Assert.AreEqual(1, comp.keylessCount);
            Assert.AreEqual(1, comp.keyedCount[("x", "z")]);
            Assert.AreEqual(1, comp.keyedCount[("y", "z")]);
        }
    }
}
