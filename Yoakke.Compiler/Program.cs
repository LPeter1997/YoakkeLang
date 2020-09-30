using System;
using System.Runtime.InteropServices;
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
            var src = @"
            const foo = proc() -> i32 { 
                0
            };
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
            //var dependencySystem = new DependencySystem();
            //var assembly = dependencySystem.Compile(ast);
        }
    }
}
