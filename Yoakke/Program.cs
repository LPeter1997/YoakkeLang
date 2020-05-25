using System;
using Yoakke.Syntax;

namespace Yoakke
{
    class Program
    {
        static void Main(string[] args)
        {
            var toks = Lexer.Lex(@"
    const Person = struct(T: type) {
        name: *char,
        age: i32,
    }

    const main = proc() -> i32 {
        0
    }
");
            foreach (var t in toks)
            {
                Console.WriteLine($"{t.Type} - '{t.Value}' ({t.Position})");
            }
        }
    }
}
