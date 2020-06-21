using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Yoakke.Compiler.Tests
{
    // https://github.com/LPeter1997/YoakkeLang/issues/1

    [TestClass]
    public class Issue0001
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
                BackendFlags = new string[] { "issue0001_c_source.c" },
            };
            var exitCode = compiler.Execute();
            Assert.AreEqual(exitCode, 0);
            return NativeUtils.LoadNativeMethod<T>(output, fname);
        }

        [TestMethod]
        public void Test()
        {
            string ykSource = @"
            const increase = @extern(""addone"", proc(i32) -> i32);
            const foo = proc() -> i32 {
                increase(34)
            };
            ";
            string cSource = @"
            #include <stdio.h>
            #include <stdint.h>

            int32_t addone(int32_t i) {
                return i + 1;
            }
";
            File.WriteAllText("issue0001_c_source.c", cSource);
            var f = CompileAndLoadFunc<Func<Int32>>(ykSource);
            Assert.AreEqual(f(), 35);
        }
    }
}
