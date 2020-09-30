using System;
using System.Runtime.InteropServices;
using Yoakke.Compiler.Compile;
using Yoakke.Compiler.Semantic;
using Yoakke.Lir.Values;
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
            // Syntax
            var srcFile = new SourceFile("foo.yk", src);
            var status = new SyntaxStatus();
            var tokens = Lexer.Lex(srcFile, status);
            var parser = new Parser(tokens, status);
            var prg = parser.ParseFile();
            var ast = ParseTreeToAst.Convert(prg);
            ast = Desugaring.Desugar(ast);

            //Console.WriteLine(prg.Dump());
            Console.WriteLine(ast.Dump());

            // Semantics
            var symTab = new SymbolTable();
            symTab.GlobalScope.Define(new Symbol.Const("i32", new Value.User(Lir.Types.Type.I32)));
            new DefineScope(symTab).Define(ast);
            new DeclareSymbol(symTab).Declare(ast);
            new ResolveSymbol(symTab).Resolve(ast);

            // Compilation
            var dependencySystem = new DependencySystem();
            var assembly = dependencySystem.Compile(ast);
        }
    }
}
