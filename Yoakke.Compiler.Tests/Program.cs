using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace Yoakke.Compiler.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var projectFolder = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent;
            var testFolder = projectFolder.GetDirectories("tests").First();

            foreach (var item in testFolder.GetFiles())
            {
                var src = File.ReadAllText(item.FullName);
                var meta = MetadataParser.Parse(src);
                foreach (var kv in meta)
                {
                    Console.WriteLine($"{kv.Key} - {kv.Value}");
                }
            }
        }
    }
}
