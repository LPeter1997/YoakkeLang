using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Yoakke.Compiler.Tests
{
    [TestClass]
    public class StructAssociationTests
    {
        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void CreateBinariesFolder(TestContext testContext)
        {
            Directory.CreateDirectory("binaries");
        }

        private T CompileAndLoadFunc<T>(string source, string fname = "foo") where T : Delegate
        {
            var output = Path.GetFullPath($"binaries/{TestContext.TestName}.dll");
            var compiler = new Compiler
            {
                Source = new Syntax.Source("test.yk", source),
                OutputType = OutputType.Shared,
                OutputPath = output,
            };
            var exitCode = compiler.Execute();
            Assert.AreEqual(exitCode, 0);
            return NativeUtils.LoadNativeMethod<T>(output, fname);
        }

        [TestMethod]
        public void AssociatedConstant()
        {
            string source = @"
            const Bar = struct {
                const Value = 3;
            };
            const foo = proc() -> i32 {
                Bar.Value
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 3);
        }

        [TestMethod]
        public void AssociatedType()
        {
            string source = @"
            const Bar = struct {
                const ReturnType = i32;
            };
            const foo = proc() -> Bar.ReturnType {
                7
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 7);
        }

        [TestMethod]
        public void AssociatedProcedure()
        {
            string source = @"
            const Bar = struct {
                const get_answer = proc() -> i32 { 54 };
            };
            const foo = proc() -> i32 {
                Bar.get_answer()
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 54);
        }

        [TestMethod]
        public void AssociatedSelfReferencingType()
        {
            string source = @"
            const Bar = struct {
                value: i32;

                const Self = Bar;
            };
            const foo = proc() -> i32 {
                var x = Bar.Self { value = 3; };
                x.value
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 3);
        }

        [TestMethod]
        public void AssociatedProcedureWithSelfTypeReference()
        {
            string source = @"
            const Bar = struct {
                value: i32;

                const new() -> Bar {
                    Bar { value = 37; }
                }
            };
            const foo = proc() -> i32 {
                Bar.new().value
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 37);
        }
    }
}
