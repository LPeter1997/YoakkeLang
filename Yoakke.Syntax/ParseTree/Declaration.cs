using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.DataStructures;
using Yoakke.Text;

namespace Yoakke.Syntax.ParseTree
{
    /// <summary>
    /// Base for every declaration <see cref="Statement"/>.
    /// </summary>
    public abstract partial class Declaration : Statement
    {
    }

    partial class Declaration
    {
        /// <summary>
        /// A source file parsed.
        /// </summary>
        public class File : Declaration
        {
            public override Span Span => GetSpan();
            public override IEnumerable<IParseTreeElement> Children => Declarations
                .Select(d => (IParseTreeElement)d)
                .OrderedMerge(Comments, e => e.Span.Start);

            /// <summary>
            /// The list of <see cref="Declaration"/>s this <see cref="File"/> contains.
            /// </summary>
            public readonly IReadOnlyList<Declaration> Declarations;
            /// <summary>
            /// The list of free <see cref="CommentGroup"/>s that don't belong to any <see cref="Node"/>.
            /// </summary>
            public readonly IReadOnlyList<CommentGroup> Comments;

            private readonly SourceFile source;

            /// <summary>
            /// Initializes a new <see cref="File"/>.
            /// </summary>
            /// <param name="source">The <see cref="SourceFile"/> the comments originate from.</param>
            /// <param name="declarations">The <see cref="Declaration"/>s the file contains.</param>
            /// <param name="comments">The free <see cref="CommentGroup"/>s that don't belong to any <see cref="Node"/>.</param>
            public File(SourceFile source, IReadOnlyList<Declaration> declarations, IReadOnlyList<CommentGroup> comments)
            {
                this.source = source;
                Declarations = declarations;
                Comments = comments;
            }

            private Span GetSpan()
            {
                var start = new Position();
                var end = new Position();
                if (Declarations.Count > 0)
                {
                    start = Declarations.First().Span.Start;
                    end = Declarations.Last().Span.End;
                }
                if (Comments.Count > 0)
                {
                    var start2 = Comments.First().Span.Start;
                    var end2 = Comments.Last().Span.End;
                    start = start < start2 ? start : start2;
                    end = end > end2 ? end : end2;
                }
                return new Span(source, start, end);
            }
        }

        /// <summary>
        /// A constant or variable definition.
        /// </summary>
        public class Definition : Declaration
        {
            public override Span Span => new Span(Keyword.Span, Semicolon.Span);
            public override IEnumerable<IParseTreeElement> Children
            {
                get
                {
                    yield return Keyword;
                    yield return Name;
                    if (Colon != null) yield return Colon;
                    if (Type != null) yield return Type;
                    if (Assign != null) yield return Assign;
                    if (Value != null) yield return Value;
                    yield return Semicolon;
                }
            }

            /// <summary>
            /// The documentation comment.
            /// </summary>
            public readonly CommentGroup? Doc;
            /// <summary>
            /// The 'const' or 'var' keyword.
            /// </summary>
            public readonly Token Keyword;
            /// <summary>
            /// The name identifier.
            /// </summary>
            public readonly Token Name;
            /// <summary>
            /// The colon, if there was a type specifier.
            /// </summary>
            public readonly Token? Colon;
            /// <summary>
            /// The type specifier.
            /// </summary>
            public readonly Expression? Type;
            /// <summary>
            /// The assignment symbol, if there was a value assigned.
            /// </summary>
            public readonly Token? Assign;
            /// <summary>
            /// The assigned value.
            /// </summary>
            public readonly Expression? Value;
            /// <summary>
            /// The ';' at the end.
            /// </summary>
            public readonly Token Semicolon;

            public Definition(
                CommentGroup? doc, 
                Token keyword, 
                Token name, 
                Token? colon, 
                Expression? type, 
                Token assign, 
                Expression value, 
                Token semicolon)
            {
                Doc = doc;
                Keyword = keyword;
                Name = name;
                Colon = colon;
                Type = type;
                Assign = assign;
                Value = value;
                Semicolon = semicolon;
            }
        }
    }
}
