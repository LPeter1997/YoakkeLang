using System;
using Yoakke.Semantic;
using Yoakke.Semantic.Steps;
using Yoakke.Syntax;

namespace Yoakke
{
    class Program
    {
        static void Main(string[] args)
        {
            var src = new Source(@$"
    const main = proc() -> i32 {{
        0
    }}
");
            var tokens = Lexer.Lex(src);
            var ast = Parser.ParseProgram(tokens);

            var symbolTable = new SymbolTable();
            symbolTable.GlobalScope.Define(new ConstSymbol(new Token(new Position(), TokenType.Identifier, "i32")));

            new DeclareSymbols(symbolTable).Declare(ast);
            TypeChecker.CheckType(ast);
        }
    }
}
