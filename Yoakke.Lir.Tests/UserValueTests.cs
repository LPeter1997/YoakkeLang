using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir.Types;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Tests
{
    [TestClass]
    public class UserValueTests : TestBase
    {
        private class Person : ICloneable
        {
            public string Name { get; set; }
            public int Age { get; set; }

            public override bool Equals(object obj) =>
                   obj is Person p 
                && Name == p.Name && Age == p.Age;
            public override int GetHashCode() => HashCode.Combine(Name, Age);

            public object Clone() => new Person
            {
                Name = Name,
                Age = Age,
            };
        }

        [TestMethod]
        public void ReturnConstant()
        {
            var b = GetBuilder(Type.User_);
            b.Ret(new Value.User(new Person { Name = "Jon", Age = 24 }));

            TestOnVirtualMachine(b, new Value.User(new Person { Name = "Jon", Age = 24 }));
        }

        [TestMethod]
        public void EqualityCompareTrue()
        {
            var b = GetBuilder(Type.I32);
            var p1 = new Person { Name = "Jon", Age = 24 };
            var p2 = new Person { Name = "Jon", Age = 24 };

            b.Ret(b.CmpEq(new Value.User(p1), new Value.User(p2)));

            TestOnVirtualMachine(b, Type.I32.NewValue(1));
        }

        [TestMethod]
        public void EqualityCompareFalse()
        {
            var b = GetBuilder(Type.I32);
            var p1 = new Person { Name = "Jon", Age = 24 };
            var p2 = new Person { Name = "Doe", Age = 24 };

            b.Ret(b.CmpEq(new Value.User(p1), new Value.User(p2)));

            TestOnVirtualMachine(b, Type.I32.NewValue(0));
        }
    }
}
