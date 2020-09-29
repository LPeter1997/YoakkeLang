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
        /// A return <see cref="Statement"/>.
        /// </summary>
        public class Return : Statement
        {
            public override Span Span => new Span(KwReturn.Span, Semicolon.Span);
            public override IEnumerable<IParseTreeElement> Children
            {
                get
                {
                    yield return KwReturn;
                    if (Value != null) yield return Value;
                    yield return Semicolon;
                }
            }

            /// <summary>
            /// The 'return' keyword.
            /// </summary>
            public readonly Token KwReturn;
            /// <summary>
            /// The returned value.
            /// </summary>
            public readonly Expression? Value;
            /// <summary>
            /// The ';' at the end.
            /// </summary>
            public readonly Token Semicolon;

            public Return(Token kwReturn, Expression? value, Token semicolon)
            {
                KwReturn = kwReturn;
                Value = value;
                Semicolon = semicolon;
            }
        }

        /// <summary>
        /// An <see cref="Expression"/> wrapped into a <see cref="Statement"/>.
        /// </summary>
        public class Expression_ : Statement
        {
            public override Span Span => new Span(Expression.Span, Semicolon?.Span ?? Expression.Span);
            public override IEnumerable<IParseTreeElement> Children
            {
                get
                {
                    yield return Expression;
                    if (Semicolon != null) yield return Semicolon;
                }
            }

            /// <summary>
            /// The <see cref="Expression"/> in <see cref="Statement"/> placement.
            /// </summary>
            public readonly Expression Expression;
            /// <summary>
            /// The ';'.
            /// </summary>
            public readonly Token? Semicolon;

            public Expression_(
                Expression expression, 
                Token? semicolon)
            {
                Expression = expression;
                Semicolon = semicolon;
            }
        }
    }
}
