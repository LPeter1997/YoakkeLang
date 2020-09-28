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
            ? new Span()
            : new Span(Comments.First().Span.Start, Comments.Last().Span.End);
        
        public override IEnumerable<IParseTreeElement> Children => Comments;

        /// <summary>
        /// Initializes a new <see cref="CommentGroup"/>.
        /// </summary>
        /// <param name="comments">The list of comment <see cref="Token"/>s this group consists of.</param>
        public CommentGroup(IReadOnlyList<Token> comments)
        {
            Comments = comments;
        }
    }
}
