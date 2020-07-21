using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Yoakke.Compiler.Tests
{
    // https://github.com/LPeter1997/YoakkeLang/issues/4

    [TestClass]
    public class Issue0004
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
        public void Test()
        {
            string ykSource = @"
            const bar = proc(T: type) -> var {
                proc(x: T) -> T { x }
            };
            const foo = proc() -> i32 {
                const Value = bar(i32)(5);
                Value
            };
            ";
            var f = CompileAndLoadFunc<Func<Int32>>(ykSource);
            Assert.AreEqual(f(), 5);
        }
    }
}
