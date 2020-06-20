using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Yoakke.Compiler.Tests
{
    static class TestParser
    {
        public static TestCase ParseTest(string sourcePath)
        {
            var source = File.ReadAllText(sourcePath);
            var meta = ParseMetadata(source);

            var description = meta["Name"];
            var type = meta["Type"];

            switch (type)
            {
            case "Compiles.FunctionReturnsValue":
            {
                var functionParts = meta["Function"].Split("::");
                var functionName = functionParts[0].Trim();
                var functionType = Type.GetType(functionParts[1].Trim());
                var functionInfo = functionType.GetMethod("Invoke");
                var input = ParseValues(meta["Input"], functionInfo.GetParameters().Select(x => x.ParameterType).ToArray());
                var output = ParseValue(meta["Output"], functionInfo.ReturnType);
                return new FunctionReturnsValueTestCase
                {
                    Description = description,
                    SourceFile = sourcePath,
                    OutputFile = "test.dll",
                    OutputType = OutputType.Shared,
                    FunctionName = functionName,
                    FunctionType = functionType,
                    Input = input,
                    ExpectedOutput = output,
                };
            }

            default: throw new NotImplementedException($"No test type '{type}'!");
            }
        }

        private static object[] ParseValues(string values, Type[] types)
        {
            // TODO
            return new object[] { };
        }

        private static object ParseValue(string value, Type type)
        {
            if (type == typeof(Int32)) return Int32.Parse(value);

            throw new NotImplementedException();
        }

        private static Dictionary<string, string> ParseMetadata(string source)
        {
            var result = new Dictionary<string, string>();
            using (StringReader sr = new StringReader(source))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (!line.StartsWith("// ")) break;

                    int idx = line.IndexOf(':');
                    if (idx < 0) break;

                    var key = line.Substring(3, idx - 3).Trim();
                    var value = line.Substring(idx + 1).Trim();
                    result.Add(key, value);
                }
            }
            return result;
        }
    }
}
