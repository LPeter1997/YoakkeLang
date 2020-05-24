using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Text;

namespace Yoakke.Syntax
{
    // TODO: Document

    class Lexer
    {
        private string source;
        private int index;
        private Position position;

        public bool IsEnd => index >= source.Length;

        public static IEnumerable<Token> Lex(string source)
        {
            var lexer = new Lexer(source);
            while (true)
            {
                var t = lexer.Next();
                yield return t;
                if (t.Type == TokenType.End) break;
            }
        }

        public Lexer(string source)
        {
            this.source = NormalizeNewline(source);
        }

        public Token Next()
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
                Consume(2);
                for (; Peek(0, '\n') != '\n'; Consume(1)) ;
                return null;
            }
            // Multi-line comment
            if (Matches("/*"))
            {
                Consume(2);
                int depth = 1;
                while (depth > 0)
                {
                    if (IsEnd) throw new NotImplementedException("Unclosed nested comment!");
                    if (Matches("/*"))
                    {
                        ++depth;
                        Consume(2);
                        continue;
                    }
                    if (Matches("*/"))
                    {
                        Consume(2);
                        --depth;
                        continue;
                    }
                    Consume(1);
                }
            }

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
            if (char.IsDigit(ch))
            {
                int i = 1;
                for (; char.IsDigit(Peek(i)); ++i) ;
                return MakeToken(TokenType.IntLiteral, i);
            }
            if (IsIdent(ch))
            {
                int i = 1;
                for (; IsIdent(Peek(i)); ++i) ;
                var ident = MakeToken(TokenType.Identifier, i);
                var tokenType = ident.Value switch
                {
                    "proc" => TokenType.KwProc,
                    "const" => TokenType.KwConst,
                    "struct" => TokenType.KwStruct,
                    "true" => TokenType.KwTrue,
                    "false" => TokenType.KwFalse,
                    _ => TokenType.Identifier,
                };
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
            return true;
        }

        private char Peek(int forward, char def = '\0')
        {
            var finalIndex = index + forward;
            return finalIndex < source.Length ? source[finalIndex] : def;
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

        private static string NormalizeNewline(string source) =>
            source.Replace("\r\n", "\n").Replace("\r", "\n");
    }
}
