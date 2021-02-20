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

    [InputQueryGroup]
    partial interface IKeyedInputs
    {
        public string OneKeyedInput(string k1);
        public string TwoKeyedInput(string k1, int k2);
        public int ThreeKeyedInput(string k1, int k2, string k3);
    }

    [TestClass]
    public class InputQueryTests
    {
        [TestMethod]
        public void KeylessInputQueryTests()
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
            Assert.AreEqual("Some prop value", system.Get<IKeylessInputs>().PropInput);
            Assert.AreEqual("Some method value", system.Get<IKeylessInputs>().MethodInput());

            // Change
            system.Get<IKeylessInputs>().PropInput = "A";
            system.Get<IKeylessInputs>().SetMethodInput("B");

            // Compare
            Assert.AreEqual("A", system.Get<IKeylessInputs>().PropInput);
            Assert.AreEqual("B", system.Get<IKeylessInputs>().MethodInput());
        }

        [TestMethod]
        public void KeyedInputQueryTests()
        {
            var system = new DependencySystem()
                .Register<IKeyedInputs>();

            // Try to access all without initialization
            Assert.ThrowsException<InvalidOperationException>(() => system.Get<IKeyedInputs>().OneKeyedInput("a"));
            Assert.ThrowsException<InvalidOperationException>(() => system.Get<IKeyedInputs>().OneKeyedInput("b"));
            Assert.ThrowsException<InvalidOperationException>(() => system.Get<IKeyedInputs>().TwoKeyedInput("a", 1));
            Assert.ThrowsException<InvalidOperationException>(() => system.Get<IKeyedInputs>().TwoKeyedInput("b", 1));
            Assert.ThrowsException<InvalidOperationException>(() => system.Get<IKeyedInputs>().ThreeKeyedInput("a", 1, "x"));
            Assert.ThrowsException<InvalidOperationException>(() => system.Get<IKeyedInputs>().ThreeKeyedInput("b", 1, "y"));

            // Set some of them
            system.Get<IKeyedInputs>().SetOneKeyedInput("a", "hello");
            system.Get<IKeyedInputs>().SetTwoKeyedInput("a", 1, "there");
            system.Get<IKeyedInputs>().SetThreeKeyedInput("a", 1, "x", 42);

            // Half of them should yield the set results, other half should still throw
            Assert.AreEqual("hello", system.Get<IKeyedInputs>().OneKeyedInput("a"));
            Assert.ThrowsException<InvalidOperationException>(() => system.Get<IKeyedInputs>().OneKeyedInput("b"));
            Assert.AreEqual("there", system.Get<IKeyedInputs>().TwoKeyedInput("a", 1));
            Assert.ThrowsException<InvalidOperationException>(() => system.Get<IKeyedInputs>().TwoKeyedInput("b", 1));
            Assert.AreEqual(42, system.Get<IKeyedInputs>().ThreeKeyedInput("a", 1, "x"));
            Assert.ThrowsException<InvalidOperationException>(() => system.Get<IKeyedInputs>().ThreeKeyedInput("b", 1, "y"));

            // Set the other half, update the first half
            system.Get<IKeyedInputs>().SetOneKeyedInput("b", "abc");
            system.Get<IKeyedInputs>().SetTwoKeyedInput("b", 1, "xyz");
            system.Get<IKeyedInputs>().SetThreeKeyedInput("a", 1, "x", 21);
            system.Get<IKeyedInputs>().SetOneKeyedInput("a", "bye");
            system.Get<IKeyedInputs>().SetTwoKeyedInput("a", 1, "here");
            system.Get<IKeyedInputs>().SetThreeKeyedInput("b", 1, "y", 123);

            // Now all of them should work
            Assert.AreEqual("bye", system.Get<IKeyedInputs>().OneKeyedInput("a"));
            Assert.AreEqual("abc", system.Get<IKeyedInputs>().OneKeyedInput("b"));
            Assert.AreEqual("here", system.Get<IKeyedInputs>().TwoKeyedInput("a", 1));
            Assert.AreEqual("xyz", system.Get<IKeyedInputs>().TwoKeyedInput("b", 1));
            Assert.AreEqual(21, system.Get<IKeyedInputs>().ThreeKeyedInput("a", 1, "x"));
            Assert.AreEqual(123, system.Get<IKeyedInputs>().ThreeKeyedInput("b", 1, "y"));
        }
    }
}
