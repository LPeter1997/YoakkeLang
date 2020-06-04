using System;
using Yoakke.Ast;
using Yoakke.IR;
using Yoakke.Semantic;
using Yoakke.Syntax;
using Type = Yoakke.Semantic.Type;

namespace Yoakke
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var src = new Source("some.yk", @$"
    const main = proc() {{
        0
    }}
");
                var tokens = Lexer.Lex(src);
                var ast = Parser.ParseProgram(tokens);

                var symbolTable = new SymbolTable();
                // Construct the i32 type
                {
                    var i32_sym = new ConstSymbol("i32", new TypeValue(Type.I32));
                    symbolTable.GlobalScope.Define(i32_sym);
                }

                DeclareSymbol.Declare(symbolTable, ast);
                DefineSymbol.Define(ast);
                AssignConstantSymbol.Assign(ast);
                AssignType.Assign(ast);

                //var entry = symbolTable.GlobalScope.Reference("main");

                var asm = Compiler.Compile(ast);
                Console.WriteLine(IrDump.Dump(asm));
            }
            catch (CompileError error)
            {
                error.Show();
            }
        }
    }
}
