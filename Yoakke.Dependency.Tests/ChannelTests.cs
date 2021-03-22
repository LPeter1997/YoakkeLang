using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Dependency.Tests
{
    // We use INumberInputs as the input query

    [QueryGroup]
    partial interface IChannelArithmetic
    {
        public event EventHandler<string> ErrorChannel;
        public event EventHandler<string> WarningChannel;

        [QueryChannel(nameof(ErrorChannel))]
        public int Value { get; }
        
        [QueryChannel(nameof(ErrorChannel))]
        public int AddValue(string varName);

        [QueryChannel(nameof(WarningChannel))]
        [QueryChannel(nameof(ErrorChannel))]
        public int Divide(string varName, int divisor);
    }

    class MyChannelArithmetic : IChannelArithmetic
    {
        public event EventHandler<string> ErrorChannel;
        public event EventHandler<string> WarningChannel;

        [QueryGroup]
        public INumberInputs Inputs { get; set; }

        [QueryGroup]
        public IChannelArithmetic Arithmetic { get; set; }

        public int Value
        {
            get
            {
                var result = Inputs.Variable("x");
                if (result < 0) ErrorChannel?.Invoke(this, "x was negative");
                return result + 2;
            }
        }

        public int AddValue(string varName)
        {
            var otherValue = Inputs.Variable(varName);
            if (otherValue == 0) ErrorChannel?.Invoke(this, "other was zero");
            return Arithmetic.Value + otherValue;
        }

        public int Divide(string varName, int divisor)
        {
            var divided = Inputs.Variable(varName);
            if (divisor == 0)
            {
                ErrorChannel?.Invoke(this, "division by zero");
                divisor = 1;
            }
            if (divided % 2 == 1 && divisor % 2 == 0) WarningChannel?.Invoke(this, "oddity lost");
            return divided / divisor;
        }
    }

    [TestClass]
    public class ChannelTests
    {
        private List<string> errors = new List<string>();
        private List<string> warnings = new List<string>();

        private DependencySystem MakeSystem()
        {
            var system = new DependencySystem()
                .Register<INumberInputs>()
                .Register<IChannelArithmetic, MyChannelArithmetic>();

            system.Get<IChannelArithmetic>().ErrorChannel += (s, e) => errors.Add(e);
            system.Get<IChannelArithmetic>().WarningChannel += (s, e) => warnings.Add(e);

            return system;
        }

        private void ClearChannels()
        {
            errors.Clear();
            warnings.Clear();
        }

        private void AssertErrors(params string[] vs)
        {
            Assert.AreEqual(vs.Length, errors.Count);
            foreach (var v in vs) Assert.IsTrue(errors.Contains(v));
        }

        private void AssertWarnings(params string[] vs)
        {
            Assert.AreEqual(vs.Length, warnings.Count);
            foreach (var v in vs) Assert.IsTrue(warnings.Contains(v));
        }

        [TestMethod]
        public void BasicTest()
        {
            var system = MakeSystem();

            // Set inputs
            system.Get<INumberInputs>().SetVariable("x", -1);
            system.Get<INumberInputs>().SetVariable("y", 0);
            system.Get<INumberInputs>().SetVariable("z", 3);

            // Calculate addition, should have 2 errors reported
            var result = system.Get<IChannelArithmetic>().AddValue("y");
            Assert.AreEqual(1, result);
            AssertWarnings();
            AssertErrors("other was zero", "x was negative");

            ClearChannels();

            // Calculating a sub-value should return the sub-messages
            result = system.Get<IChannelArithmetic>().Value;
            Assert.AreEqual(1, result);
            AssertWarnings();
            AssertErrors("x was negative");

            ClearChannels();

            // Redoing the calculation should yield all messages again
            result = system.Get<IChannelArithmetic>().AddValue("y");
            Assert.AreEqual(result, 1);
            AssertWarnings();
            AssertErrors("other was zero", "x was negative");
        }

        [TestMethod]
        public void MultipleChannelsTest()
        {
            var system = MakeSystem();

            // Set inputs
            system.Get<INumberInputs>().SetVariable("z", 3);

            // Division of odd by even, should raise a warning
            var result = system.Get<IChannelArithmetic>().Divide("z", 2);
            Assert.AreEqual(1, result);
            AssertWarnings("oddity lost");
            AssertErrors();

            ClearChannels();

            // Redoing it should yield them again
            result = system.Get<IChannelArithmetic>().Divide("z", 2);
            Assert.AreEqual(1, result);
            AssertWarnings("oddity lost");
            AssertErrors();
        }
    }
}
