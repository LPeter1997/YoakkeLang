using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Syntax.ParseTree;
using Yoakke.Text;

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

        private SourceFile source;
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
            Debug.Assert(this.tokens.Count > 0);
            source = this.tokens[0].Span.Source;
            Status = status;
        }

        public Declaration.File ParseFile()
        {
            var declarations = new List<Declaration>();
            while (true)
            {
                // We either want a 'var' or 'const' to have a definition
                // That's the only valid thing top-level apart from EOF
                // Unexpected token otherwise
                var token = Next();
                // EOF
                if (token.Type == TokenType.End) break;
                // Declaration
                if (token.Type == TokenType.KwConst || token.Type == TokenType.KwVar)
                {
                    declarations.Add(ParseDefinition(token));
                    continue;
                }
                // Error, unexpected
                // TODO: Report error
                throw new NotImplementedException();
            }
            // We need to append any remaining free comments
            if (lastComments.Count > 0)
            {
                freeComments.Add(CloseLastComments());
            }
            // We are done
            return new Declaration.File(
                source, 
                declarations.ToArray(), 
                freeComments.ToArray());
        }

        private Declaration.Definition ParseDefinition(Token keyword)
        {
            var doc = GetDocComment(keyword);
            var name = Expect(TokenType.Identifier);
            // : <type>
            Token? colon = null;
            Expression? type = null;
            if (Peek().Type == TokenType.Colon)
            {
                colon = Expect(TokenType.Colon);
                type = ParseExpression();
            }
            // = value
            Token? assign = null;
            Expression? value = null;
            if (Peek().Type == TokenType.Assign)
            {
                assign = Expect(TokenType.Assign);
                value = ParseExpression();
            }
            // Semicolon and line comment
            var semicolon = Expect(TokenType.Semicolon);
            var lineComment = GetLineComment(semicolon);
            // We are done
            return new Declaration.Definition(
                doc, keyword, name, colon, type, assign, value, semicolon, lineComment
            );
        }

        private Expression ParseExpression()
        {
            // TODO
            throw new NotImplementedException();
        }

        private CommentGroup? GetDocComment(Token t)
        {
            // No comments
            if (lastComments.Count == 0) return null;

            var last = lastComments.Last();
            if (last.Span.End.Line + 1 == t.Span.Start.Line)
            {
                // The comment belongs to this token
                return CloseLastComments();
            }
            else
            {
                // The comment is separate
                freeComments.Add(CloseLastComments());
                return null;
            }
        }

        private CommentGroup? GetLineComment(Token t)
        {
            var comment = NextPrimitive();
            if (comment.Type == TokenType.LineComment && comment.Span.Start.Line == t.Span.End.Line)
            {
                // Same line, belongs to the token
                var group = new CommentGroup(source, new Token[] { comment });
                return group;
            }
            else
            {
                // We can safely step back, as no comments were eaten
                PrevPrimitive();
                return null;
            }
        }

        private Token Expect(TokenType tt)
        {
            var t = Next();
            if (t.Type != tt)
            {
                // TODO: Report error
                throw new NotImplementedException();
            }
            return t;
        }

        private Token Peek()
        {
            for (int i = tokenIndex + 1; i + 1 < tokens.Count; ++i)
            {
                if (tokens[i].Type != TokenType.LineComment) return tokens[i];
            }
            return tokens.Last();
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
                    return t;
                }
            }
        }

        private Token NextPrimitive()
        {
            if (tokenIndex + 1 <tokens.Count) ++tokenIndex;
            return tokens[tokenIndex];
        }

        private void PrevPrimitive()
        {
            --tokenIndex;
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
                freeComments.Add(CloseLastComments());
                lastComments.Add(token);
            }
        }

        private CommentGroup CloseLastComments()
        {
            Debug.Assert(lastComments.Count > 0);
            var result = new CommentGroup(source, lastComments.ToArray());
            lastComments.Clear();
            return result;
        }
    }
}
