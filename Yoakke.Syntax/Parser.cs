using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Syntax.ParseTree;

namespace Yoakke.Syntax
{
    /// <summary>
    /// The parser that converts a sequence of tokens into a parse tree.
    /// </summary>
    public class Parser
    {
        /// <summary>
        /// The <see cref="SyntaxStatus"/> this <see cref="Parser"/> reports to.
        /// </summary>
        public SyntaxStatus Status { get; }

        private IReadOnlyList<Token> tokens;
        private int tokenIndex = -1;

        private List<CommentGroup> freeComments = new List<CommentGroup>();
        private List<Token> lastComments = new List<Token>();

        /// <summary>
        /// Initializes a new <see cref="Parser"/>.
        /// </summary>
        /// <param name="tokens">The sequence of <see cref="Token"/>s to parse.</param>
        /// <param name="status">The <see cref="SyntaxStatus"/> to report errors to.</param>
        public Parser(IEnumerable<Token> tokens, SyntaxStatus status)
        {
            this.tokens = tokens.ToArray();
            Status = status;
        }

        private Token Next()
        {
            while (true)
            {
                var t = NextPrimitive();
                if (t.Type == TokenType.LineComment)
                {
                    PushCommentToGroup(t);
                }
                else
                {
                    // TODO: Maybe push the last comment group here too in case lines separate this token from the last or
                    // something?
                    return t;
                }
            }
        }

        private Token NextPrimitive()
        {
            if (tokenIndex + 1 <tokens.Count) ++tokenIndex;
            return tokens[tokenIndex];
        }

        private void PushCommentToGroup(Token token)
        {
            if (lastComments.Count == 0)
            {
                // First comment in the new group
                lastComments.Add(token);
                return;
            }
            // Not the first comment in the group
            var last = lastComments.Last();
            if (last.Span.End.Line + 1 == token.Span.Start.Line)
            {
                // This token belongs to the last group
                lastComments.Add(token);
            }
            else
            {
                // The last group is a separate group, separate it out and start a new group
                freeComments.Add(new CommentGroup(token.Span.Source, lastComments.ToArray()));
                lastComments.Clear();
                lastComments.Add(token);
            }
        }
    }
}
