using System.Collections.Generic;
using System.Linq;
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
            /// The file documentation.
            /// </summary>
            public readonly CommentGroup? Doc;
            /// <summary>
            /// The list of <see cref="Declaration"/>s this <see cref="File"/> contains.
            /// </summary>
            public readonly IReadOnlyList<Declaration> Declarations;
            /// <summary>
            /// The list of free <see cref="CommentGroup"/>s that don't belong to any <see cref="Node"/>.
            /// </summary>
            public readonly IReadOnlyList<CommentGroup> Comments;

            /// <summary>
            /// The name of the file.
            /// </summary>
            public string Name => System.IO.Path.GetFileNameWithoutExtension(source?.Path ?? "unnamed");

            private readonly SourceText? source;

            public File(
                SourceText? source, 
                CommentGroup? doc,
                IReadOnlyList<Declaration> declarations, 
                IReadOnlyList<CommentGroup> comments)
            {
                this.source = source;
                Doc = doc;
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
            /// <summary>
            /// Inline comment.
            /// </summary>
            public readonly CommentGroup? LineComment;

            public Definition(
                CommentGroup? doc, 
                Token keyword, 
                Token name, 
                Token? colon, 
                Expression? type, 
                Token? assign, 
                Expression? value, 
                Token semicolon,
                CommentGroup? lineComment)
            {
                Doc = doc;
                Keyword = keyword;
                Name = name;
                Colon = colon;
                Type = type;
                Assign = assign;
                Value = value;
                Semicolon = semicolon;
                LineComment = lineComment;
            }
        }
    }
}
