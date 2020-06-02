using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.Syntax;

namespace Yoakke.Semantic
{
    class UndefinedSymbolError : CompileError
    {
        public Token Token { get; set; }

        public UndefinedSymbolError(Token token)
        {
            Token = token;
        }

        public override void Show()
        {
            Console.WriteLine($"Semantic error {Token.Position}!");
            Console.WriteLine(Annotation.Annotate(Token.Position));
            Console.WriteLine($"Undefined symbol '{Token.Value}'!");
        }
    }
}
