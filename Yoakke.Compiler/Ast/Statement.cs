using Yoakke.Compiler.Semantic;
using Yoakke.Compiler.Syntax;

namespace Yoakke.Compiler.Ast
{
    partial class Statement
    {
        /// <summary>
        /// A variable definition.
        /// </summary>
        public class VarDef : Statement
        {
            /// <summary>
            /// The name of the defined variable.
            /// </summary>
            public Token Name { get; set; }
            /// <summary>
            /// The type of the defined variable.
            /// </summary>
            public Expression? Type { get; set; }
            /// <summary>
            /// The initial value of the variable.
            /// </summary>
            public Expression? Value { get; set; }

            /// <summary>
            /// The <see cref="Symbol"/> this variable defines.
            /// </summary>
            public Symbol.Variable? Symbol { get; set; }

            /// <summary>
            /// Initializes a new <see cref="VarDef"/>.
            /// </summary>
            /// <param name="name">The name of the defined variable.</param>
            /// <param name="type">The type of the defined variable.</param>
            /// <param name="value">The initial value of the variable.</param>
            public VarDef(Token name, Expression? type, Expression? value)
            {
                Name = name;
                Type = type;
                Value = value;
            }

            public override Statement Clone() =>
                new VarDef(Name, Type?.Clone(), Value?.Clone());
        }

        /// <summary>
        /// An explicit return from a procedure.
        /// Syntax:
        /// ```
        /// return <Value>;
        /// ```
        /// </summary>
        public class Return : Statement
        {
            /// <summary>
            /// The value the statement returns.
            /// </summary>
            public Expression? Value { get; set; }

            /// <summary>
            /// Initializes a new <see cref="Return"/>.
            /// </summary>
            /// <param name="value">The value the statement returns.</param>
            public Return(Expression? value)
            {
                Value = value;
            }

            public override Statement Clone() =>
                new Return(Value?.Clone());
        }

        /// <summary>
        /// An <see cref="Expression"/> that has been wrapped up in a <see cref="Statement"/>, so it can
        /// appear in statement position.
        /// </summary>
        public class Expression_ : Statement
        {
            /// <summary>
            /// The wrapped up <see cref="Expression"/>.
            /// </summary>
            public Expression Expression { get; set; }
            /// <summary>
            /// True, if the <see cref="Expression"/> is followed by a semicolon.
            /// </summary>
            public bool HasSemicolon { get; set; }

            /// <summary>
            /// Initializes a new <see cref="Expression_"/>.
            /// </summary>
            /// <param name="expression">The <see cref="Expression"/> to wrap up.</param>
            /// <param name="hasSemicolon">True, if the <see cref="Expression"/> is followed by a semicolon.</param>
            public Expression_(Expression expression, bool hasSemicolon)
            {
                Expression = expression;
                HasSemicolon = hasSemicolon;
            }

            public override Statement Clone() =>
                new Expression_(Expression.Clone(), HasSemicolon);
        }
    }
}
