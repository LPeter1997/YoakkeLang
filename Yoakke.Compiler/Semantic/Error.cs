using System;
using System.Collections.Generic;
using System.Linq;
using Yoakke.Compiler.Syntax;
using Yoakke.Compiler.Utils;

namespace Yoakke.Compiler.Semantic
{
    /// <summary>
    /// A semantic compile error for undefined symbol references.
    /// </summary>
    public class UndefinedSymbolError : CompileError
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

    /// <summary>
    /// A semantic error representing a type mismatch.
    /// </summary>
    public class TypeError : CompileError
    {
        /// <summary>
        /// The first <see cref="Type"/> that participated in the unification.
        /// </summary>
        public Type First { get; set; }
        /// <summary>
        /// The second <see cref="Type"/> that participated in the unification.
        /// </summary>
        public Type Second { get; set; }

        /// <summary>
        /// Initializes a new <see cref="TypeError"/>.
        /// </summary>
        /// <param name="first">The first <see cref="Type"/> that participated in the unification.</param>
        /// <param name="second">The second <see cref="Type"/> that participated in the unification.</param>
        public TypeError(Type first, Type second)
        {
            First = first;
            Second = second;
        }

        public override void Show()
        {
            // TODO: It would be nice to mark where the types came from?
            Console.WriteLine($"Type mismatch between {First} and {Second}!");
        }
    }

    /// <summary>
    /// A semantic error when initializing struct fields.
    /// </summary>
    public class InitializationError : CompileError
    {
        /// <summary>
        /// The list of uninitialized struct fields, if any.
        /// </summary>
        public List<string>? UninitializedFields { get; set; }
        /// <summary>
        /// The unknown field that was initialized.
        /// </summary>
        public Token? UnknownField { get; set; }

        /// <summary>
        /// Initializes a new <see cref="InitializationError"/> with uninitialized fields.
        /// </summary>
        /// <param name="uninitializedFields">The list of uninitialized fields.</param>
        public InitializationError(List<string> uninitializedFields)
        {
            UninitializedFields = uninitializedFields;
        }

        /// <summary>
        /// Initializes a new <see cref="InitializationError"/> with an unknown field.
        /// </summary>
        /// <param name="unknownField">The unknown field.</param>
        public InitializationError(Token unknownField)
        {
            UnknownField = unknownField;
        }

        public override void Show()
        {
            if (UninitializedFields != null)
            {
                Console.WriteLine("Initialization error!");
                Console.WriteLine($"Uninitialized fields: {string.Join(", ", UninitializedFields.Select(x => $"'{x}'"))}.");
            }
            else
            {
                Assert.NonNull(UnknownField);
                var field = UnknownField.Value;
                Console.WriteLine($"Initialization error {field.Position}!");
                Console.WriteLine(Annotation.Annotate(field.Position));
                Console.WriteLine($"Unknown field '{field.Value}'!");
            }
        }
    }

    /// <summary>
    /// A semantic error when not all code paths return a value.
    /// </summary>
    public class NotAllPathsReturnError : CompileError
    {
        // TODO: Position?

        public override void Show()
        {
            Console.WriteLine("Semantic error!");
            Console.WriteLine("Not all code paths return a value!");
        }
    }
}
