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
        // Helpers for operator precedence /////////////////////////////////////

        private enum Associativity
        {
            Left, Right,
        }

        private struct Precedence
        {
            public Associativity Associativity { get; set; }
            public HashSet<TokenType> Operators { get; set; }

            private Precedence(Associativity associativity, HashSet<TokenType> operators)
            {
                Associativity = associativity;
                Operators = operators;
            }

            public static Precedence Left(params TokenType[] operators) =>
                new Precedence(Associativity.Left, operators.ToHashSet());
            public static Precedence Right(params TokenType[] operators) =>
                new Precedence(Associativity.Right, operators.ToHashSet());
        }

        // Operator precedence table ///////////////////////////////////////////

        private static Precedence[] PrecedenceTable = new Precedence[]
        {
            Precedence.Right(TokenType.Assign),
            Precedence.Left(TokenType.Or),
            Precedence.Left(TokenType.And),
            Precedence.Left(TokenType.Equal, TokenType.NotEqual),
            Precedence.Left(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual),
            Precedence.Left(TokenType.Add, TokenType.Subtract),
            Precedence.Left(TokenType.Multiply, TokenType.Divide, TokenType.Modulo),
        };

        // Parser itself ///////////////////////////////////////////////////////

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

        // Declarations ////////////////////////////////////////////////////////

        public Declaration.File ParseFile()
        {
            var declarations = new List<Declaration>();
            while (true)
            {
                // We either want a 'var' or 'const' to have a definition
                // That's the only valid thing top-level apart from EOF
                // Unexpected token otherwise
                var peek = Peek();
                // EOF
                if (peek.Type == TokenType.End) break;
                // Declaration
                if (peek.Type == TokenType.KwConst || peek.Type == TokenType.KwVar)
                {
                    declarations.Add(ParseDefinition());
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

        private Declaration.Definition ParseDefinition()
        {
            var keyword = Expect(TokenType.KwConst, TokenType.KwVar);
            var doc = GetDocComment(keyword);
            var name = Expect(TokenType.Identifier);
            // : <type>
            Token? colon = null;
            Expression? type = null;
            if (Match(TokenType.Colon, out colon))
            {
                type = ParseExpression(ExprState.TypeOnly);
            }
            // = value
            Token? assign = null;
            Expression? value = null;
            if (Match(TokenType.Assign, out assign))
            {
                value = ParseExpression(ExprState.None);
            }
            // Semicolon and line comment
            var semicolon = Expect(TokenType.Semicolon);
            var lineComment = GetLineComment(semicolon);
            // We are done
            return new Declaration.Definition(
                doc, keyword, name, colon, type, assign, value, semicolon, lineComment
            );
        }

        // Statements //////////////////////////////////////////////////////////

        private Statement ParseStatement()
        {
            switch (Peek().Type)
            {
            case TokenType.KwConst:
            case TokenType.KwVar:
                return ParseDefinition();

            case TokenType.KwReturn:
                // TODO
                throw new NotImplementedException();

            case TokenType.OpenBrace:
            case TokenType.KwIf:
            case TokenType.KwWhile:
                // TODO: We need to greedily parse these here to be unambiguous and predictable
                throw new NotImplementedException();
            }

            // The only remaining possibility is an expression statement
            var expr = ParseExpression(ExprState.None);
            var semicolon = Expect(TokenType.Semicolon);
            return new Statement.Expression_(expr, semicolon);
        }

        // Expressions /////////////////////////////////////////////////////////

        [Flags]
        private enum ExprState
        {
            None = 0,
            TypeOnly = 1,
            NoBraced = 2,
        }

        private Expression ParseExpression(ExprState state) => ParseBinaryExpression(state);

        private Expression ParseBinaryExpression(ExprState state, int precedence = 0)
        {
            if (precedence >= PrecedenceTable.Length || state.HasFlag(ExprState.TypeOnly))
            {
                // Out of precedence table entries or we are parsing a type
                return ParsePrefixExpression(state);
            }

            var desc = PrecedenceTable[precedence];
            var result = ParseBinaryExpression(state, precedence + 1);

            if (desc.Associativity == Associativity.Left)
            {
                while (true)
                {
                    var op = Peek();
                    if (!desc.Operators.Contains(op.Type)) break;

                    op = Next();
                    var right = ParseBinaryExpression(state, precedence + 1);
                    result = new Expression.Binary(result, op, right);
                }
                return result;
            }
            else
            {
                var op = Peek();
                if (!desc.Operators.Contains(op.Type)) return result;

                op = Next();
                var right = ParseBinaryExpression(state, precedence);
                return new Expression.Binary(result, op, right);
            }
        }

        private Expression ParsePrefixExpression(ExprState state)
        {
            // NOTE: For now we just use this for extendability
            return ParsePostfixExpression(state);
        }

        private Expression ParsePostfixExpression(ExprState state)
        {
            var result = ParseAtomicExpression(state);
            while (true)
            {
                var peek = Peek();
                if (peek.Type == TokenType.OpenParen)
                {
                    // Call expression
                    // TODO
                    throw new NotImplementedException();
                }
                else if (!state.HasFlag(ExprState.TypeOnly)
                      && !state.HasFlag(ExprState.NoBraced)
                      && peek.Type == TokenType.OpenBrace)
                {
                    // Struct instantiation
                    // TODO
                    throw new NotImplementedException();
                }
                else if (peek.Type == TokenType.Dot)
                {
                    // Dot path
                    // TODO
                    throw new NotImplementedException();
                }
                else break;
            }
            return result;
        }

        private Expression ParseAtomicExpression(ExprState state)
        {
            var peek = Peek();
            switch (peek.Type)
            {
            case TokenType.KwProc: return ParseProcExpression(state);
            case TokenType.KwIf: return ParseIfExpression();
            case TokenType.KwWhile: return ParseWhileExpression();
            case TokenType.KwStruct: return ParseStructTypeExpression();

            case TokenType.Identifier:
            case TokenType.IntLiteral:
            case TokenType.StringLiteral:
            case TokenType.KwTrue:
            case TokenType.KwFalse:
                return new Expression.Literal(Next());

            case TokenType.OpenParen: return ParseParenthesized();

            case TokenType.OpenBrace:
                if (!state.HasFlag(ExprState.NoBraced))
                {
                    return ParseBlockExpression();
                }
                else
                {
                    // TODO: Error
                    // Or maybe this is a case where we still want to parse a braced block?
                    // Because... there's no other possibility
                    throw new NotImplementedException();
                }

            default:
                // TODO: Error
                throw new NotImplementedException();
            }
        }

        private Expression ParseProcExpression(ExprState state)
        {
            // TODO
            throw new NotImplementedException();
        }

        private Expression.ProcSignature ParseProcSignature()
        {
            // TODO
            throw new NotImplementedException();
        }

        private Expression.If ParseIfExpression()
        {
            // TODO
            throw new NotImplementedException();
        }

        private Expression.While ParseWhileExpression()
        {
            // TODO
            throw new NotImplementedException();
        }

        private Expression.StructType ParseStructTypeExpression()
        {
            // TODO
            throw new NotImplementedException();
        }

        private Expression ParseParenthesized()
        {
            // TODO
            throw new NotImplementedException();
        }

        private Expression ParseBlockExpression()
        {
            // TODO
            throw new NotImplementedException();
        }

        // Helpers /////////////////////////////////////////////////////////////

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

        private bool Match(TokenType tt, out Token? token)
        {
            var peek = Peek();
            if (peek.Type == tt)
            {
                token = Next();
                return true;
            }
            token = null;
            return false;
        }

        private Token Expect(params TokenType[] tts)
        {
            var t = Next();
            if (!tts.Contains(t.Type))
            {
                // TODO: Report error
                throw new NotImplementedException();
            }
            return t;
        }

        private Token Peek(int amount = 0)
        {
            for (int i = tokenIndex + 1; i + 1 < tokens.Count; ++i)
            {
                if (tokens[i].Type != TokenType.LineComment)
                {
                    if (amount == 0) return tokens[i];
                    else --amount;
                }
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
