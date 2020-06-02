using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.Syntax;

namespace Yoakke.Semantic
{
    /// <summary>
    /// A semantic compile error for undefined symbol references.
    /// </summary>
    class UndefinedSymbolError : CompileError
    {
        /// <summary>
        /// The <see cref="Token"/> that referenced the symbol.
        /// </summary>
        public Token Token { get; set; }

        /// <summary>
        /// Initialies a new <see cref="UndefinedSymbolError"/>.
        /// </summary>
        /// <param name="token">The <see cref="Token"/> that referenced the symbol.</param>
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
