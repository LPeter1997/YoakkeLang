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

            /// <summary>
            /// The documentation comment.
            /// </summary>
            public readonly CommentGroup? Doc;
            /// <summary>
            /// The 'return' keyword.
            /// </summary>
            public readonly Token Return_;
            /// <summary>
            /// The returned value.
            /// </summary>
            public readonly Expression? Value;
            /// <summary>
            /// The ';' at the end.
            /// </summary>
            public readonly Token Semicolon;

            public Return(CommentGroup? doc, Token return_, Expression? value, Token semicolon)
            {
                Doc = doc;
                Return_ = return_;
                Value = value;
                Semicolon = semicolon;
            }
        }
    }
}
