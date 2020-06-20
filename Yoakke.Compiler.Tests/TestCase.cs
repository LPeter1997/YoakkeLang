using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Text;

namespace Yoakke.Compiler.Tests
{
    abstract class TestCase
    {
        public string Description { get; set; }
        public string SourceFile { get; set; }

        public abstract bool Run(out string message);
    }

    abstract class CompilingTestCase : TestCase
    {
        public OutputType OutputType { get; set; }
        public string OutputFile { get; set; }

        public override bool Run(out string message)
        {
            try
            {
                var output = new StringWriter();
                Compiler.Output = output;
                var compiler = new Compiler
                {
                    SourceFile = SourceFile,
                    OutputType = OutputType,
                    OutputPath = OutputFile,
                };
                int errCode = compiler.Execute();
                if (errCode != 0)
                {
                    message = output.ToString();
                    return false;
                }
                return TestBinary(out message);
            }
            catch (Exception exception)
            {
                message = exception.ToString();
                return false;
            }
        }

        public abstract bool TestBinary(out string message);
    }

    class FunctionReturnsValueTestCase : CompilingTestCase
    {
        public string FunctionName { get; set; }
        public Type FunctionType { get; set; }
        public object[] Input { get; set; }
        public object ExpectedOutput { get; set; }

        public override bool TestBinary(out string message)
        {
            message = null;
            var testMethod = NativeUtils.LoadNativeMethod(OutputFile, FunctionName, FunctionType);
            var output = testMethod.Method.Invoke(null, Input);
            if (!output.Equals(ExpectedOutput))
            {
                message = $"{output} != {ExpectedOutput}";
                return false;
            }
            return true;
        }
    }
}
