using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Yoakke.Compiler.Tests
{
    [TestClass]
    public class Issue0005
    {
        private void Compile(string source)
        {
            var compiler = new Compiler
            {
                Source = new Syntax.Source("test.yk", source),
                DumpIr = true, // So we don't compile
            };
            compiler.Execute();
        }

        [TestMethod]
        public void Test()
        {
            string source = @"
const bar = proc(x: var) { };

const main = proc() -> i32 {
    bar(3);
    bar(true);
    0
};
";
            Compile(source);
        }
    }
}
