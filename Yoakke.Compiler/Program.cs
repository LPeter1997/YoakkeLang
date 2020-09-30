using System;
using Yoakke.Syntax;
using Yoakke.Text;

namespace Yoakke.Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            var src = @"// This is now
            // File documentation

            // I am now documenting foo
            // Which is fun
            // And should work
            const foo = proc() -> i32 { 
                var x: i32;
                x = 0;
                while x < 10 {
                    x = x + 1;
                }
                x
            }; // LISTEN
";
            var srcFile = new SourceFile("foo.yk", src);
            var status = new SyntaxStatus();
            var tokens = Lexer.Lex(srcFile, status);
            var parser = new Parser(tokens, status);
            var prg = parser.ParseFile();
            var ast = ParseTreeToAst.Convert(prg);
            ast = Desugaring.Desugar(ast);

            Console.WriteLine(prg.Dump());
            Console.WriteLine("\n===================\n");
            Console.WriteLine(ast.Dump());
        }
    }
}
