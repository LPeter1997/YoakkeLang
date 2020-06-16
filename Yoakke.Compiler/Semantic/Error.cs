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
        /// The referenced <see cref="Symbol"/>s name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The <see cref="Position"/> of the reference, if any.
        /// </summary>
        public Position? Position { get; set; }

        /// <summary>
        /// Initialies a new <see cref="UndefinedSymbolError"/>.
        /// </summary>
        /// <param name="name">The name of the references <see cref="Symbol"/>.</param>
        public UndefinedSymbolError(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Initialies a new <see cref="UndefinedSymbolError"/>.
        /// </summary>
        /// <param name="token">The <see cref="Token"/> that referenced the <see cref="Symbol"/>.</param>
        public UndefinedSymbolError(Token token)
            : this(token.Value)
        {
            Position = token.Position;
        }

        public override void Show()
        {
            if (Position == null)
            {
                Console.Write($"Semantic error: ");
            }
            else
            {
                Console.WriteLine($"Semantic error {Position.Value}!");
                Console.WriteLine(Annotation.Annotate(Position.Value));
            }
            Console.WriteLine($"Undefined symbol '{Name}'!");
        }
    }
}
