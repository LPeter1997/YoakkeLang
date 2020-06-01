using System;
using System.Diagnostics.CodeAnalysis;
using Yoakke.Semantic;
using Yoakke.Semantic.Steps;
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
    const main = proc(x: i32) -> i32 {{
        @
    }}
");
                var tokens = Lexer.Lex(src);
                var ast = Parser.ParseProgram(tokens);

                var symbolTable = new SymbolTable();
                // Construct the i32 type
                {
                    var i32_sym = new ConstSymbol(new Token(new Position(), TokenType.Identifier, "i32"));
                    i32_sym.Type = Type.I32;
                    symbolTable.GlobalScope.Define(i32_sym);
                }

                DeclareSymbols.Declare(symbolTable, ast);
                DefineSymbols.Define(ast);
                TypeChecker.CheckType(ast);
            }
            catch (CompileError error)
            {
                error.Show();
            }
        }
    }
}
