using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Yoakke.Compiler.Tests
{
    [TestClass]
    public abstract class TestBase
    {
        public TestContext TestContext { get; set; }

        static TestBase()
        {
            // We do the common initialization here

            // Create the binaries directory, where the binaries will go when compiled
            Directory.CreateDirectory("binaries");
        }

        protected int Compile(string source)
        {
            var compiler = new Compiler
            {
                Source = new Syntax.Source($"{TestContext.TestName}.yk", source),
                DumpIr = true, // So we don't compile
            };
            return compiler.Execute();
        }

        protected T CompileAndLoadFunc<T>(string source, string fname = "foo") where T : Delegate
        {
            var output = Path.GetFullPath($"binaries/{TestContext.TestName}.dll");
            var compiler = new Compiler
            {
                Source = new Syntax.Source($"{TestContext.TestName}.yk", source),
                OutputType = OutputType.Shared,
                OutputPath = output,
            };
            var exitCode = compiler.Execute();
            Assert.AreEqual(exitCode, 0);
            return NativeUtils.LoadNativeMethod<T>(output, fname);
        }
    }
}
