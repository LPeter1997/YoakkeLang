using System;
using Yoakke.Syntax;

namespace Yoakke
{
    class Program
    {
        static void Main(string[] args)
        {
            var src = new Source(@$"
    const Person = struct(T: type) {{
    {'\t'}name: *char,
        age: i32,
    }}

    const main = proc() -> i32 {{
        0
    }}
");
            var tokens = Lexer.Lex(src);
            var ast = Parser.ParseProgram(tokens);
        }
    }
}
