using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Text;

namespace Yoakke.Syntax.ParseTree
{
    /// <summary>
    /// A sequence of comments without newlines in between.
    /// </summary>
    public class CommentGroup : Node
    {
        /// <summary>
        /// The comment <see cref="Token"/>s.
        /// </summary>
        public readonly IReadOnlyList<Token> Comments;

        public override Span Span => Comments.Count == 0
            ? new Span(source)
            : new Span(source, Comments.First().Span.Start, Comments.Last().Span.End);
        
        public override IEnumerable<IParseTreeElement> Children => Comments;

        private readonly SourceFile source;

        /// <summary>
        /// Initializes a new <see cref="CommentGroup"/>.
        /// </summary>
        /// <param name="source">The <see cref="SourceFile"/> the comments originate from.</param>
        /// <param name="comments">The list of comment <see cref="Token"/>s this group consists of.</param>
        public CommentGroup(SourceFile source, IReadOnlyList<Token> comments)
        {
            this.source = source;
            Comments = comments;
        }
    }
}
