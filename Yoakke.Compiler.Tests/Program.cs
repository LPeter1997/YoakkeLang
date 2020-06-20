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

            bool success = true;
            int index = 0;
            foreach (var file in testFolder.GetFiles())
            {
                var test = TestParser.ParseTest(file.FullName);

                Console.Write($"{index}. [");
                Console.ForegroundColor = ConsoleColor.Yellow;
                var iconPos = Console.CursorLeft;
                Console.Write("  ?  ");
                Console.ResetColor();
                Console.Write($"] ({file.Name}) {test.Description}");

                var oldPos = Console.CursorLeft;
                Console.CursorLeft = iconPos;
                if (test.Run(out var message))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("  OK ");
                    Console.CursorLeft = oldPos;
                    Console.WriteLine();
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("ERROR");
                    Console.CursorLeft = oldPos;
                    Console.WriteLine();
                    Console.ResetColor();
                    Console.WriteLine(message);
                    success = false;
                }
                ++index;
            }

            if (success)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("ALL TESTS PASSED");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR IN TEST CASES");
            }
            Console.ResetColor();
        }
    }
}
