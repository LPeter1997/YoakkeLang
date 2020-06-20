using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Yoakke.Compiler.Tests
{
    /// <summary>
    /// Utility for parsing test metadata from Yoakke test files.
    /// </summary>
    static class MetadataParser
    {
        public static Dictionary<string, string> Parse(string source)
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
