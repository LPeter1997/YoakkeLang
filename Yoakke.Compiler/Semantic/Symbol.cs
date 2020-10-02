using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir.Values;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Semantic
{
    /// <summary>
    /// Base of every symbol in the program.
    /// </summary>
    public abstract partial class Symbol
    {
        /// <summary>
        /// The AST node where the <see cref="Symbol"/> was defined.
        /// </summary>
        public readonly Node? Definition;
        /// <summary>
        /// The name of the <see cref="Symbol"/>.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Initializes a new <see cref="Symbol"/>.
        /// </summary>
        /// <param name="definition">The AST node definition of the symbol.</param>
        /// <param name="name">The name of the symbol.</param>
        public Symbol(Node? definition, string name)
        {
            Definition = definition;
            Name = name;
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
            /// The calculated compile-time value.
            /// </summary>
            public Value? Value { get; set; }

            private Const(Node? definition, string name, Value? value)
                : base(definition, name)
            {
                Value = value;
            }

            /// <summary>
            /// Initializes a new <see cref="Const"/> by a constant definition.
            /// </summary>
            /// <param name="definition">The constant definition node.</param>
            public Const(Declaration.Const definition)
                : this(definition, definition.Name, null)
            {
            }

            /// <summary>
            /// Initializes a new <see cref="Const"/> by a name and <see cref="Value"/>.
            /// </summary>
            /// <param name="name">The name of the symbol.</param>
            /// <param name="value">The <see cref="Value"/> of the symbol.</param>
            public Const(string name, Value value)
                : this(null, name, value)
            {
            }
        }

        /// <summary>
        /// A variable <see cref="Symbol"/>.
        /// </summary>
        public class Var : Symbol
        {
            private static int unnamedCnt = 0;

            /// <summary>
            /// Initializes a new <see cref="Var"/>.
            /// </summary>
            /// <param name="param">The parameter definition.</param>
            public Var(Expression.ProcSignature.Parameter param)
                : base(param, param.Name ?? $"unnamed_{unnamedCnt++}")
            {
            }

            /// <summary>
            /// Initializes a new <see cref="Var"/>.
            /// </summary>
            /// <param name="definition">The variable definition statement.</param>
            public Var(Statement.Var definition)
                : base(definition, definition.Name)
            {
            }
        }
    }
}
