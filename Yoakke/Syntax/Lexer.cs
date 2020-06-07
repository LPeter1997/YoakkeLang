using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Text;

namespace Yoakke.Syntax
{
    /// <summary>
    /// Splits up the source text into <see cref="Token"/>s to simplify parsing.
    /// </summary>
    class Lexer
    {
        private int index;
        private Position position;

        private bool IsEnd => index >= position.Source.Text.Length;

        /// <summary>
        /// Lexes the whole source code.
        /// </summary>
        /// <param name="source">The <see cref="Source"/> to lex.</param>
        /// <returns>An <see cref="IEnumerable{Token}"/> over all <see cref="Token"/>s.</returns>
        public static IEnumerable<Token> Lex(Source source)
        {
            var lexer = new Lexer(source);
            while (true)
            {
                var t = lexer.Next();
                yield return t;
                if (t.Type == TokenType.End) break;
            }
        }

        private Lexer(Source source)
        {
            this.position = new Position(source, line: 0, column: 0);
        }

        private Token Next()
        {
            while (true)
            {
                var token = NextInternal();
                if (token != null) return token.Value;
            }
        }

        private Token? NextInternal()
        {
            if (IsEnd) return MakeToken(TokenType.End, 0);

            var ch = Peek(0);

            // Whitespace
            if (char.IsWhiteSpace(ch) || char.IsControl(ch))
            {
                Consume(1);
                return null;
            }
            // Single-line comment
            if (Matches("//"))
            {
                for (; Peek(0, '\n') != '\n'; Consume(1)) ;
                return null;
            }
            // Multi-line comment
            var startPosition = position;
            if (Matches("/*"))
            {
                var prevPosition = position;
                int depth = 1;
                while (depth > 0)
                {
                    if (IsEnd) throw new UnclosedCommentError(startPosition, prevPosition);

                    if (Matches("/*"))
                    {
                        ++depth;
                        prevPosition = position;
                    }
                    else if (Matches("*/"))
                    {
                        --depth;
                        prevPosition = position;
                    }
                    else
                    {
                        var consumed = Consume(1);
                        if (!char.IsControl(consumed[0])) prevPosition = position;
                    }
                }
                return null;
            }

            // Punctuation and operators
            switch (ch)
            {
            case '(': return MakeToken(TokenType.OpenParen   , 1);
            case ')': return MakeToken(TokenType.CloseParen  , 1);
            case '{': return MakeToken(TokenType.OpenBrace   , 1);
            case '}': return MakeToken(TokenType.CloseBrace  , 1);
            case '[': return MakeToken(TokenType.OpenBracket , 1);
            case ']': return MakeToken(TokenType.CloseBracket, 1);
            case '.': return MakeToken(TokenType.Dot         , 1);
            case ',': return MakeToken(TokenType.Comma       , 1);
            case ':': return MakeToken(TokenType.Colon       , 1);
            case ';': return MakeToken(TokenType.Semicolon   , 1);
            case '-':
                if (Peek(1) == '>') return MakeToken(TokenType.Arrow, 2);
                return MakeToken(TokenType.Subtract, 1);
            case '=':
                if (Peek(1) == '=') return MakeToken(TokenType.Equal, 2);
                return MakeToken(TokenType.Assign  , 1);
            case '+': return MakeToken(TokenType.Add     , 1);
            case '*': return MakeToken(TokenType.Multiply, 1);
            case '/': return MakeToken(TokenType.Divide  , 1);
            case '%': return MakeToken(TokenType.Modulo  , 1);
            case '>':
                if (Peek(1) == '=') return MakeToken(TokenType.GreaterEqual, 2);
                return MakeToken(TokenType.Greater, 1);
            case '<':
                if (Peek(1) == '=') return MakeToken(TokenType.LessEqual, 2);
                return MakeToken(TokenType.Less, 1);
            case '!':
                if (Peek(1) == '=') return MakeToken(TokenType.NotEqual, 2);
                return MakeToken(TokenType.Not, 1);
            case '&':
                if (Peek(1) == '&') return MakeToken(TokenType.And, 2);
                break; // TODO: Bitand?
            case '|':
                if (Peek(1) == '|') return MakeToken(TokenType.Or, 2);
                break; // TODO: Bitor?
            }

            // Literals

            // Integer literal
            if (char.IsDigit(ch))
            {
                int i = 1;
                for (; char.IsDigit(Peek(i)); ++i) ;
                return MakeToken(TokenType.IntLiteral, i);
            }
            // Identifier
            if (IsIdent(ch) || ch == '@')
            {
                int i = 1;
                for (; IsIdent(Peek(i)); ++i) ;
                var ident = MakeToken(TokenType.Identifier, i);
                // Determine if keyword
                var tokenType = ident.Value switch
                {
                    "proc" => TokenType.KwProc,
                    "const" => TokenType.KwConst,
                    "struct" => TokenType.KwStruct,
                    "true" => TokenType.KwTrue,
                    "false" => TokenType.KwFalse,
                    _ => TokenType.Identifier,
                };
                // Builtin
                if (ident.Value.StartsWith('@')) tokenType = TokenType.IntrinsicIdentifier;

                return new Token(ident.Position, tokenType, ident.Value);
            }
            // TODO: String literal

            return MakeToken(TokenType.Unknown, 1);
        }

        private Token MakeToken(TokenType type, int length) =>
            new Token(position, type, Consume(length));

        private bool Matches(string str)
        {
            for (int i = 0; i < str.Length; ++i)
            {
                if (str[i] != Peek(i)) return false;
            }
            Consume(str.Length);
            return true;
        }

        private char Peek(int forward, char def = '\0')
        {
            var finalIndex = index + forward;
            return finalIndex < position.Source.Text.Length ? position.Source.Text[finalIndex] : def;
        }

        private string Consume(int amount)
        {
            var builder = new StringBuilder();
            int i = 0;
            while (i < amount)
            {
                char ch = Peek(i++);
                if (ch == '\0') break;
                if (ch == '\n')
                {
                    position = position.Newline();
                    builder.Append('\n');
                    continue;
                }
                if (!char.IsControl(ch)) position = position.Advance();
                builder.Append(ch);
            }
            index += i;
            return builder.ToString();
        }

        private static bool IsIdent(char ch) => 
            char.IsLetterOrDigit(ch) || ch == '_';
    }
}
