using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Dependency.Tests
{
    [InputQueryGroup]
    partial interface IDerivedInputs
    {
        public int MyConstant { get; set; }
        public int Variable(string name);
    }

    [QueryGroup]
    partial interface IComputation
    {
        public int ComputeSomething();
        public int ComputeOther(string v1, string v2);
    }

    class MyComputation : IComputation
    {
        [QueryGroup]
        public IDerivedInputs Inputs { get; set; }

        public int ComputeSomething()
        {
            return Inputs.MyConstant + Inputs.Variable("a");
        }

        public int ComputeOther(string v1, string v2)
        {
            return Inputs.MyConstant * (Inputs.Variable(v1) + Inputs.Variable(v2));
        }
    }

    [TestClass]
    public class BasicDerivedValueTests
    {
        [TestMethod]
        public void DeriveWithoutKey()
        {
            var system = new DependencySystem()
                .Register<IDerivedInputs>()
                .Register<IComputation, MyComputation>();

            // Asking for the computation should throw initially as the inputs are not set
            Assert.ThrowsException<InvalidOperationException>(() => system.Get<IComputation>().ComputeSomething());

            // Let's set one of the inputs
            system.Get<IDerivedInputs>().MyConstant = 3;
            // Should still throw
            Assert.ThrowsException<InvalidOperationException>(() => system.Get<IComputation>().ComputeSomething());

            // Setting the other should make it valid
            system.Get<IDerivedInputs>().SetVariable("a", 5);
            Assert.AreEqual(8, system.Get<IComputation>().ComputeSomething());

            // Updating a value should change result
            system.Get<IDerivedInputs>().SetVariable("a", 7);
            Assert.AreEqual(10, system.Get<IComputation>().ComputeSomething());
        }

        [TestMethod]
        public void DeriveWithKey()
        {
            var system = new DependencySystem()
                .Register<IDerivedInputs>()
                .Register<IComputation, MyComputation>();

            // Asking for the computation should throw initially as the inputs are not set
            Assert.ThrowsException<InvalidOperationException>(() => system.Get<IComputation>().ComputeOther("x", "y"));

            // Setting the variables is not enough, the constant is required
            system.Get<IDerivedInputs>().SetVariable("x", 4);
            system.Get<IDerivedInputs>().SetVariable("y", 7);
            Assert.ThrowsException<InvalidOperationException>(() => system.Get<IComputation>().ComputeOther("x", "y"));

            // Setting the constant should resolve it
            system.Get<IDerivedInputs>().MyConstant = 2;
            Assert.AreEqual(22, system.Get<IComputation>().ComputeOther("x", "y"));

            // Updating a value should change result
            system.Get<IDerivedInputs>().SetVariable("y", 3);
            Assert.AreEqual(14, system.Get<IComputation>().ComputeOther("x", "y"));
        }
    }
}
