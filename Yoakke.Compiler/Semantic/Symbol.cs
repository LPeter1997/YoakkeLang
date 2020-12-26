using Yoakke.Lir.Values;
using Yoakke.Syntax.Ast;
using Type = Yoakke.Compiler.Semantic.Types.Type;

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
            /// The <see cref="Type"/> of the value.
            /// </summary>
            public Type? Type { get; set; }
            /// <summary>
            /// The calculated compile-time value.
            /// </summary>
            public Value? Value { get; set; }

            private Const(Node? definition, string name, Type? type, Value? value)
                : base(definition, name)
            {
                Type = type;
                Value = value;
            }

            /// <summary>
            /// Initializes a new <see cref="Const"/> by a constant definition.
            /// </summary>
            /// <param name="definition">The constant definition node.</param>
            public Const(Declaration.Const definition)
                : this(definition, definition.Name, null, null)
            {
            }

            /// <summary>
            /// Initializes a new <see cref="Const"/> by a name and <see cref="Value"/>.
            /// </summary>
            /// <param name="name">The name of the symbol.</param>
            /// <param name="type">The <see cref="Type"/> of the symbol.</param>
            /// <param name="value">The <see cref="Value"/> of the symbol.</param>
            public Const(string name, Type type, Value value)
                : this(null, name, type, value)
            {
            }
        }

        /// <summary>
        /// The kinds of <see cref="Var"/> <see cref="Symbol"/>s.
        /// </summary>
        public enum VarKind
        {
            /// <summary>
            /// A local variable.
            /// </summary>
            Local,
            /// <summary>
            /// A parameter variable.
            /// </summary>
            Param,
            /// <summary>
            /// A global variable.
            /// </summary>
            Global,
        }

        /// <summary>
        /// A variable <see cref="Symbol"/>.
        /// </summary>
        public class Var : Symbol
        {
            private static int unnamedCnt = 0;

            /// <summary>
            /// The <see cref="VarKind"/> of this <see cref="Var"/>.
            /// </summary>
            public readonly VarKind Kind;
            /// <summary>
            /// The <see cref="Type"/> of this <see cref="Var"/>.
            /// </summary>
            public Type? Type { get; set; }

            // TODO: Doc
            public Var(Node? definition, string name, VarKind kind)
                : base(definition, name)
            {
                Kind = kind;
            }

            /// <summary>
            /// Initializes a new parameter <see cref="Var"/>.
            /// </summary>
            /// <param name="param">The parameter definition.</param>
            public Var(Expression.ProcSignature.Parameter param)
                : this(param, param.Name ?? $"unnamed_{unnamedCnt++}", VarKind.Param)
            {
            }

            /// <summary>
            /// Initializes a new <see cref="Var"/>.
            /// </summary>
            /// <param name="definition">The variable definition statement.</param>
            /// <param name="isGlobal">True, if this is a global variable.</param>
            public Var(Statement.Var definition, bool isGlobal)
                : this(definition, definition.Name, isGlobal ? VarKind.Global : VarKind.Local)
            {
            }
        }
    }
}
