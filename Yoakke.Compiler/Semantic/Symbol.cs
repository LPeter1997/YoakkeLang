using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Yoakke.Compiler.Ast;
using Yoakke.Compiler.Syntax;
using Yoakke.Compiler.Utils;

namespace Yoakke.Compiler.Semantic
{
    /// <summary>
    /// The base for every kind of symbol in the program.
    /// </summary>
    public abstract partial class Symbol
    {
        /// <summary>
        /// The name of the <see cref="Symbol"/>.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The <see cref="Position"/> of the <see cref="Symbol"/>, if any. Can be null, if this is a
        /// built-in symbol.
        /// </summary>
        public Position? Position { get; }

        /// <summary>
        /// Initializes a new <see cref="Symbol"/>.
        /// </summary>
        /// <param name="name">The name of this symbol.</param>
        public Symbol(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new <see cref="Symbol"/>.
        /// </summary>
        /// <param name="token">The defining <see cref="Token"/> of this symbol.</param>
        public Symbol(Token token)
            : this(token.Value)
        {
            Position = token.Position;
        }
    }

    partial class Symbol
    {
        /// <summary>
        /// A compile-time constant <see cref="Symbol"/>.
        /// </summary>
        public class Const : Symbol
        {
            /// <summary>
            /// The corresponding <see cref="ConstDefinition"/> that has created this <see cref="Const"/>.
            /// </summary>
            public readonly Declaration.ConstDef? Definition;
            /// <summary>
            /// The constant <see cref="Value"/> assigned to this <see cref="Const"/>.
            /// </summary>
            public Value? Value { get; set; }

            public Const(string name, Value value)
                : base(name)
            {
                Value = value;
            }

            public Const(Declaration.ConstDef definition)
                : base(definition.Name)
            {
                Definition = definition;
            }

            /// <summary>
            /// Retrieves the <see cref="Value"/> associated with this constant.
            /// If it's not calculated yet, enforces the calculation.
            /// </summary>
            /// <returns>The <see cref="Value"/> of this constant.</returns>
            public Value GetValue()
            {
                if (Value == null)
                {
                    Assert.NonNull(Definition);
                    ConstEval.Evaluate(Definition);
                }
                Assert.NonNull(Value);
                return Value;
            }
        }

        /// <summary>
        /// A user-defined variable <see cref="Symbol"/>. 
        /// </summary>
        public class Variable : Symbol
        {
            /// <summary>
            /// The inferred <see cref="Type"/> for this variable.
            /// </summary>
            public readonly Type Type = new Type.Variable();

            /// <summary>
            /// Initializes a new <see cref="Variable"/>.
            /// </summary>
            /// <param name="name">The <see cref="Token"/> that named this variable at definition.</param>
            public Variable(Token name)
                : base(name)
            {
            }
        }
    }
}
