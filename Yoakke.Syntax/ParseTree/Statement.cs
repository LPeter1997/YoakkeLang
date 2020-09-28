using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Text;

namespace Yoakke.Syntax.ParseTree
{
    /// <summary>
    /// Base for every statement <see cref="Node"/>.
    /// </summary>
    public abstract partial class Statement : Node
    {
    }

    partial class Statement
    {
        /// <summary>
        /// A variable definition <see cref="Statement"/>.
        /// </summary>
        public class Var : Statement
        {
            public override Span Span => new Span(Var_.Span, Semicolon.Span);
            public override IEnumerable<IParseTreeElement> Children
            {
                get
                {
                    yield return Var_;
                    yield return Name;
                    if (Colon != null) yield return Colon;
                    if (Type != null) yield return Type;
                    yield return Assign;
                    yield return Value;
                    yield return Semicolon;
                }
            }

#pragma warning disable CS8618
            /// <summary>
            /// The documentation comment.
            /// </summary>
            public CommentGroup? Doc { get; init; }
            /// <summary>
            /// The 'var' keyword.
            /// </summary>
            public Token Var_ { get; init; }
            /// <summary>
            /// The name identifier.
            /// </summary>
            public Token Name { get; init; }
            /// <summary>
            /// The colon, if there was a type specifier.
            /// </summary>
            public Token? Colon { get; init; }
            /// <summary>
            /// The type specifier.
            /// </summary>
            public Expression? Type { get; init; }
            /// <summary>
            /// The assignment symbol.
            /// </summary>
            public Token Assign { get; init; }
            /// <summary>
            /// The assigned value.
            /// </summary>
            public Expression Value { get; init; }
            /// <summary>
            /// The ';' at the end.
            /// </summary>
            public Token Semicolon { get; init; }
#pragma warning restore CS8618
        }

        /// <summary>
        /// A return <see cref="Statement"/>.
        /// </summary>
        public class Return : Statement
        {
            public override Span Span => new Span(Return_.Span, Semicolon.Span);
            public override IEnumerable<IParseTreeElement> Children
            {
                get
                {
                    yield return Return_;
                    if (Value != null) yield return Value;
                    yield return Semicolon;
                }
            }

#pragma warning disable CS8618
            /// <summary>
            /// The documentation comment.
            /// </summary>
            public CommentGroup? Doc { get; init; }
            /// <summary>
            /// The 'return' keyword.
            /// </summary>
            public Token Return_ { get; init; }
            /// <summary>
            /// The returned value.
            /// </summary>
            public Expression? Value { get; init; }
            /// <summary>
            /// The ';' at the end.
            /// </summary>
            public Token Semicolon { get; init; }
#pragma warning restore CS8618
        }
    }
}
