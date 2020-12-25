using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Yoakke.Syntax.Error;
using Yoakke.Syntax.ParseTree;
using Yoakke.Text;

namespace Yoakke.Syntax
{
    // TODO: We can rewrite with PeekBuffer!
    // TODO: Expect kinda has a lot of parameters, can we make it nicer?

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

        // Prefix and postfix operators ////////////////////////////////////////

        private static HashSet<TokenType> PrefixOperators = new HashSet<TokenType>
        { 
            // Ponoted
            TokenType.Add,
            // Negation
            TokenType.Subtract,
            // Pointer type
            TokenType.Multiply,
            // Address of
            TokenType.Bitand, 
            // Bitwise and bool negation
            TokenType.Not,
        };

        private static HashSet<TokenType> PostfixOperators = new HashSet<TokenType>
        {
            // Dereference
            TokenType.Bitnot,
        };

        // Operator precedence table ///////////////////////////////////////////

        private static Precedence[] PrecedenceTable = new Precedence[]
        {
            Precedence.Right(TokenType.Assign,
                TokenType.AddAssign, TokenType.SubtractAssign, 
                TokenType.MultiplyAssign, TokenType.DivideAssign, TokenType.ModuloAssign),
            Precedence.Left(TokenType.Or),
            Precedence.Left(TokenType.And),
            Precedence.Left(TokenType.Equals, TokenType.NotEquals),
            Precedence.Left(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual),
            Precedence.Left(TokenType.Add, TokenType.Subtract),
            Precedence.Left(TokenType.Multiply, TokenType.Divide, TokenType.Modulo),
        };

        // Parser itself ///////////////////////////////////////////////////////

        /// <summary>
        /// The <see cref="SyntaxStatus"/> this <see cref="Parser"/> reports to.
        /// </summary>
        public SyntaxStatus Status { get; }

        private SourceFile? source;
        private IReadOnlyList<Token> tokens;
        private int tokenIndex = -1;
        private Token? lastToken;

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
            source = this.tokens.First().Span.Source;
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
                Status.Report(new ExpectedTokenError(
                    new TokenType[] { TokenType.KwVar, TokenType.KwConst },
                    lastToken,
                    Peek()) { Context = "top level declaration" });
                Next();
            }
            // We need to append any remaining free comments
            if (lastComments.Count > 0)
            {
                freeComments.Add(CloseLastComments());
            }
            // If the first free comment started at the first line, we assume it's file documentation
            CommentGroup? doc = null;
            if (freeComments.Count > 0 && freeComments.First().Span.Start.Line == 0)
            {
                doc = freeComments.First();
                freeComments.RemoveAt(0);
            }
            // We are done
            return new Declaration.File(
                source, 
                doc,
                declarations.ToArray(), 
                freeComments.ToArray());
        }

        private Declaration.Definition ParseDefinition()
        {
            var keyword = Expect(null, null, TokenType.KwConst, TokenType.KwVar);
            var doc = GetDocComment(keyword);
            var name = Expect("declaration statement", null, TokenType.Identifier);
            // : <type>
            Expression? type = null;
            if (Match(TokenType.Colon, out var colon))
            {
                type = ParseExpression(ExprState.TypeOnly);
            }
            // = value
            Expression? value = null;
            if (Match(TokenType.Assign, out var assign))
            {
                value = ParseExpression(ExprState.None);
            }
            else if (keyword.Type == TokenType.KwConst)
            {
                // TODO: COnstants must have a value assigned
                throw new NotImplementedException();
            }
            // Semicolon and line comment
            var semicolon = Expect("declaration statement", null, TokenType.Semicolon);
            var lineComment = GetLineComment(semicolon);
            // We are done
            return new Declaration.Definition(
                doc, keyword, name, colon, type, assign, value, semicolon, lineComment
            );
        }

        // Statements //////////////////////////////////////////////////////////

        private object ParseStatementOrExpression()
        {
            switch (Peek().Type)
            {
            case TokenType.KwConst:
            case TokenType.KwVar:
                return ParseDefinition();

            case TokenType.KwReturn:
                return ParseReturnStatement();

            // Greedy consumption to avoid keeping these as expressions
            case TokenType.OpenBrace:
                return new Statement.Expression_(ParseBlockExpression(), null);
            case TokenType.KwIf:
                return new Statement.Expression_(ParseIfExpression(), null);
            case TokenType.KwWhile:
                return new Statement.Expression_(ParseWhileExpression(), null);
            }

            // The only remaining possibility is an expression
            return ParseExpression(ExprState.None);
        }

        private Statement ParseReturnStatement()
        {
            var ret = Expect(null, null, TokenType.KwReturn);
            Expression? value = null;
            if (Peek().Type != TokenType.Semicolon)
            {
                value = ParseExpression(ExprState.None);
            }
            var semicolon = Expect("return statement", null, TokenType.Semicolon);
            return new Statement.Return(ret, value, semicolon);
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
            var peek = Peek();
            if (PrefixOperators.Contains(peek.Type))
            {
                var op = Next();
                var operand = ParsePrefixExpression(state);
                return new Expression.Prefix(op, operand);
            }
            else if (peek.Type == TokenType.OpenBracket)
            {
                var openBracket = Expect(null, null, TokenType.OpenBracket);
                // Array type expression
                var length = ParseExpression(ExprState.None);
                var closeBracket = Expect("array type expression", openBracket, TokenType.CloseBracket);
                var elementType = ParsePrefixExpression(state);
                return new Expression.ArrayType(openBracket, length, closeBracket, elementType);
            }
            else
            {
                return ParsePostfixExpression(state);
            }
        }

        private Expression ParsePostfixExpression(ExprState state)
        {
            var result = ParseAtomicExpression(state);
            while (true)
            {
                var peek = Peek();
                if (PostfixOperators.Contains(peek.Type))
                {
                    var op = Next();
                    result = new Expression.Postfix(result, op);
                }
                else if (peek.Type == TokenType.OpenParen)
                {
                    var openParen = Expect(null, null, TokenType.OpenParen);
                    // Call expression
                    var args = new List<WithComma<Expression>>();
                    while (Peek().Type != TokenType.CloseParen)
                    {
                        var arg = ParseExpression(ExprState.None);
                        var hasComma = Match(TokenType.Comma, out var comma);
                        args.Add(new WithComma<Expression>(arg, comma));
                        if (!hasComma) break;
                    }
                    var closeParen = Expect("call expression", openParen, TokenType.CloseParen);
                    result = new Expression.Call(result, openParen, args, closeParen);
                }
                else if (peek.Type == TokenType.OpenBracket)
                {
                    var openBracket = Expect(null, null, TokenType.OpenBracket);
                    // Subscript expression
                    var index = ParseExpression(ExprState.None);
                    var closeBracket = Expect("subscript expression", openBracket, TokenType.CloseBracket);
                    result = new Expression.Subscript(result, openBracket, index, closeBracket);
                }
                else if (!state.HasFlag(ExprState.TypeOnly)
                      && !state.HasFlag(ExprState.NoBraced)
                      && peek.Type == TokenType.OpenBrace)
                {
                    // Struct instantiation
                    var openBrace = Expect(null, null, TokenType.OpenBrace);
                    var fields = new List<Expression.StructValue.Field>();
                    while (Peek().Type != TokenType.CloseBrace)
                    {
                        fields.Add(ParseStructValueField());
                    }
                    var closeBrace = Expect("struct instantiation", openBrace, TokenType.CloseBrace);
                    result = new Expression.StructValue(result, openBrace, fields, closeBrace);
                }
                else if (peek.Type == TokenType.Dot)
                {
                    // Dot path
                    var dot = Expect(null, null, TokenType.Dot);
                    var ident = Expect("dot path expression", null, TokenType.Identifier);
                    result = new Expression.DotPath(result, dot, ident);
                }
                else break;
            }
            return result;
        }

        private Expression.StructValue.Field ParseStructValueField()
        {
            var name = Expect("struct initializer value", null, TokenType.Identifier);
            var assign = Expect("struct initializer value", null, TokenType.Assign);
            var value = ParseExpression(ExprState.None);
            var semicolon = Expect("struct initializer value", null, TokenType.Semicolon);
            return new Expression.StructValue.Field(name, assign, value, semicolon);
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
            // Either a signature as a type def. or a signature + body, which is a full procedure definition
            var signature = ParseProcSignature();
            if (!state.HasFlag(ExprState.TypeOnly) && Peek().Type == TokenType.OpenBrace)
            {
                var body = ParseBlockExpression();
                return new Expression.Proc(signature, body);
            }
            return signature;
        }

        private Expression.ProcSignature ParseProcSignature()
        {
            var proc = Expect(null, null, TokenType.KwProc);
            var openParen = Expect("procedure signature", null, TokenType.OpenParen);
            // Parameters
            var parameters = new List<WithComma<Expression.ProcSignature.Parameter>>();
            while (Peek().Type != TokenType.CloseParen)
            {
                var param = ParseProcParameter();
                var hasComma = Match(TokenType.Comma, out var comma);
                parameters.Add(new WithComma<Expression.ProcSignature.Parameter>(param, comma));
                if (!hasComma) break;
            }
            var closeParen = Expect("procedure signature", openParen, TokenType.CloseParen);
            // Return type
            Expression? ret = null;
            if (Match(TokenType.Arrow, out var arrow))
            {
                ret = ParseExpression(ExprState.TypeOnly);
            }
            // We are done
            return new Expression.ProcSignature(
                proc, openParen, parameters.ToArray(), closeParen, arrow, ret
            );
        }

        private Expression.ProcSignature.Parameter ParseProcParameter()
        {
            Token? name = null;
            Token? colon = null;
            if (Peek(1).Type == TokenType.Colon)
            {
                // This parameter has a name
                name = Expect("parameter", null, TokenType.Identifier);
                colon = Expect("parameter", null, TokenType.Colon);
            }
            var type = ParseExpression(ExprState.TypeOnly);
            return new Expression.ProcSignature.Parameter(name, colon, type);
        }

        private Expression.If ParseIfExpression()
        {
            var iff = Expect(null, null, TokenType.KwIf);
            var condition = ParseExpression(ExprState.NoBraced);
            var then = ParseBlockExpression();
            // Else-ifs and else
            var elifs = new List<Expression.If.ElseIf>();
            Token? elseKw = null;
            Expression.Block? elseBlock = null;
            while (true)
            {
                if (Match(TokenType.KwElse, out elseKw))
                {
                    // We have an else or an else if
                    if (Match(TokenType.KwIf, out var elifIfKw))
                    {
                        // It's an else-if
                        var elifElse = elseKw;
                        elseKw = null;
                        var elifCondition = ParseExpression(ExprState.NoBraced);
                        var elifThen = ParseBlockExpression();
                        elifs.Add(new Expression.If.ElseIf(elifElse, elifIfKw, elifCondition, elifThen));
                    }
                    else
                    {
                        // It was an else
                        elseBlock = ParseBlockExpression();
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            return new Expression.If(iff, condition, then, elifs.ToArray(), elseKw, elseBlock);
        }

        private Expression.While ParseWhileExpression()
        {
            var whileKw = Expect(null, null, TokenType.KwWhile);
            var condition = ParseExpression(ExprState.NoBraced);
            var body = ParseBlockExpression();
            return new Expression.While(whileKw, condition, body);
        }

        private Expression.StructType ParseStructTypeExpression()
        {
            var structKw = Expect(null, null, TokenType.KwStruct);
            var openBrace = Expect("struct type expression", null, TokenType.OpenBrace);

            var declarations = new List<Declaration>();
            var fields = new List<Expression.StructType.Field>();

            Token? closeBrace = null;
            while (!Match(TokenType.CloseBrace, out closeBrace))
            {
                var peek = Peek();
                if (peek.Type == TokenType.KwConst || peek.Type == TokenType.KwVar)
                {
                    declarations.Add(ParseDefinition());
                    continue;
                }
                else
                {
                    fields.Add(ParseStructTypeField());
                }
            }

            return new Expression.StructType(structKw, openBrace, fields.ToArray(), declarations, closeBrace);
        }

        private Expression.StructType.Field ParseStructTypeField()
        {
            var name = Expect("stuct field", null, TokenType.Identifier);
            var doc = GetDocComment(name);
            var colon = Expect("stuct field", null, TokenType.Colon);
            var type = ParseExpression(ExprState.TypeOnly);
            var semicolon = Expect("stuct field", null, TokenType.Semicolon);
            var lineComment = GetLineComment(semicolon);

            return new Expression.StructType.Field(doc, name, colon, type, semicolon, lineComment);
        }

        private Expression ParseParenthesized()
        {
            var openParen = Expect(null, null, TokenType.OpenParen);
            var inside = ParseExpression(ExprState.None);
            var closeParen = Expect(null, openParen, TokenType.CloseParen);
            return new Expression.Parenthesized(openParen, inside, closeParen);
        }

        private Expression.Block ParseBlockExpression()
        {
            var openBrace = Expect(null, null, TokenType.OpenBrace);
            var statements = new List<Statement>();
            Expression? value = null;
            while (true)
            {
                if (Peek().Type == TokenType.CloseBrace) break;

                var element = ParseStatementOrExpression();
                if (element is Statement stmt)
                {
                    statements.Add(stmt);
                }
                else
                {
                    // It's an expression. It can become a statement with a semicolon.
                    // Otherwise we treat is as the evaluation value
                    var expr = (Expression)element;
                    if (Match(TokenType.Semicolon, out var semicolon))
                    {
                        // It's an expression statement
                        statements.Add(new Statement.Expression_(expr, semicolon));
                    }
                    else
                    {
                        // Treat it as value
                        value = expr;
                        break;
                    }
                }
            }
            var closeBrace = Expect("block", openBrace, TokenType.CloseBrace);
            // We are done
            return new Expression.Block(openBrace, statements, value, closeBrace);
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

        private bool Match(TokenType tt, [MaybeNullWhen(false)] out Token token)
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

        private Token Expect(string? context, Token? pair, params TokenType[] tts)
        {
            var t = Peek();
            if (!tts.Contains(t.Type))
            {
                // NOTE: For now we just return the first kind of token expected here
                // Later we might want to use smarter strategies
                var result = new Token(t.Span, tts[0], t.Value);
                Status.Report(new ExpectedTokenError(tts, lastToken, t) { Context = context, Starting = pair });
                return result;
            }
            else
            {
                Next();
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
                    lastToken = t;
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
