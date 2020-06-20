using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text;

namespace Yoakke.Compiler.Tests
{
    [TestClass]
    public class ImplicitReturnTests
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
        public void ReturnNumberLiteral()
        {
            string source = @"const foo = proc() -> i32 { 7 };";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 7);
        }

        [TestMethod]
        public void ReturnDefinedConstant()
        {
            string source = @"
            const Value = 15;    
            const foo = proc() -> i32 { Value };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 15);
        }

        [TestMethod]
        public void ReturnDefinedConstantAfterFunction()
        {
            string source = @"
            const foo = proc() -> i32 { Value };
            const Value = 15;
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 15);
        }

        [TestMethod]
        public void ReturnValueReturnedByFunction()
        {
            string source = @"
            const bar = proc() -> i32 { 3 };
            const foo = proc() -> i32 { bar() };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 3);
        }

        [TestMethod]
        public void ReturnValueReturnedByFunctionAfterFunction()
        {
            string source = @"
            const foo = proc() -> i32 { bar() };
            const bar = proc() -> i32 { 123 };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 123);
        }

        [TestMethod]
        public void ReturnValueReturnedByFunctionCompileTime()
        {
            string source = @"
            const bar = proc() -> i32 { 9 };
            const Value = bar();
            const foo = proc() -> i32 { Value };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 9);
        }

        [TestMethod]
        public void ReturnValueReturnedByFunctionCompileTimeReverseOrder()
        {
            string source = @"
            const Value = bar();
            const foo = proc() -> i32 { Value };
            const bar = proc() -> i32 { 9 };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 9);
        }
    }
}
