using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Yoakke.Dependency;

namespace Yoakke.Dependency.Tests
{
    [InputQueryGroup]
    partial interface IKeylessInputs
    {
        public string PropInput { get; set; }

        public string MethodInput();
    }

    [TestClass]
    public class InputQueryTests
    {
        [TestMethod]
        public void KeylessInputQueryAsPropertyTests()
        {
            var system = new DependencySystem()
                .Register<IKeylessInputs>();

            // Try to access both without initialization, should throw
            Assert.ThrowsException<InvalidOperationException>(() => system.Get<IKeylessInputs>().PropInput);
            Assert.ThrowsException<InvalidOperationException>(() => system.Get<IKeylessInputs>().MethodInput());

            // Initialize
            system.Get<IKeylessInputs>().PropInput = "Some prop value";
            system.Get<IKeylessInputs>().SetMethodInput("Some method value");

            // Compare
            Assert.AreEqual(system.Get<IKeylessInputs>().PropInput, "Some prop value");
            Assert.AreEqual(system.Get<IKeylessInputs>().MethodInput(), "Some method value");

            // Change
            system.Get<IKeylessInputs>().PropInput = "A";
            system.Get<IKeylessInputs>().SetMethodInput("B");

            // Compare
            Assert.AreEqual(system.Get<IKeylessInputs>().PropInput, "A");
            Assert.AreEqual(system.Get<IKeylessInputs>().MethodInput(), "B");
        }
    }
}
