﻿using System.Collections.Generic;
using System.IO;
using System.Text;
using Yoakke.Syntax.Error;
using Yoakke.Text;

namespace Yoakke.Syntax
{
    // TODO: We can rewrite with PeekBuffer!

    /// <summary>
    /// The lexer that splits the input into <see cref="Token"/>s.
    /// </summary>
    public class Lexer
    {
        /// <summary>
        /// The called event handler type when a syntax error occurs.
        /// </summary>
        /// <param name="sender">The <see cref="Lexer"/> that produced the error.</param>
        /// <param name="syntaxError">The <see cref="ISyntaxError"/> describing the error.</param>
        public delegate void SyntaxErrorEventHandler(Lexer sender, ISyntaxError syntaxError);

        /// <summary>
        /// Happens, when a syntax error is detected.
        /// </summary>
        public event SyntaxErrorEventHandler? SyntaxError;

        private SourceFile source;
        private TextReader reader;
        private Cursor cursor = new Cursor();

        private StringBuilder peekBuffer = new StringBuilder();

        /// <summary>
        /// Initializes a new <see cref="Lexer"/>.
        /// </summary>
        /// <param name="source">The <see cref="SourceFile"/> to lex.</param>
        public Lexer(SourceFile source)
        {
            this.source = source;
            reader = new StringReader(source.Text);
        }

        // TODO: Has side-effects! Can't re-enumerate!
        /// <summary>
        /// Returns an <see cref="IEnumerable{Token}"/> that lexes until EOF is read.
        /// </summary>
        /// <returns>The <see cref="IEnumerable{Token}"/> of the lexed input.</returns>
        public IEnumerable<Token> Lex()
        {
            while (true)
            {
                var t = Next();
                yield return t;
                if (t.Type == TokenType.End) break;
            }
        }

        /// <summary>
        /// Reads in the next <see cref="Token"/> in the input.
        /// </summary>
        /// <returns>The read in <see cref="Token"/>.</returns>
        public Token Next()
        {
            while (true)
            {
                var t = NextInternal();
                if (t != null) return t;
            }
        }

        private Token? NextInternal()
        {
            var ch = Peek(0);
            // EOF
            if (ch == '\0') return MakeToken(TokenType.End, 0);
            // Whitespace
            if (char.IsWhiteSpace(ch) || char.IsControl(ch))
            {
                Consume(1);
                return null;
            }
            // Single-line comment
            if (Matches("//"))
            {
                char Map(char ch, char from, char to) => ch == from ? to : ch;

                int len = 2;
                // NOTE: A bit hairy logic but basically both EOF and \r become \n
                for (; Map(Peek(len, '\n'), '\r', '\n') != '\n'; ++len) ;
                return MakeToken(TokenType.LineComment, len);
            }

            // NOTE: For now I've removed multi-line comments as I rarely use them
            // for now single-line comments are sufficient

            // Punctuation and operators
            switch (ch)
            {
            case '(': return MakeToken(TokenType.OpenParen, 1);
            case ')': return MakeToken(TokenType.CloseParen, 1);
            case '{': return MakeToken(TokenType.OpenBrace, 1);
            case '}': return MakeToken(TokenType.CloseBrace, 1);
            case '[': return MakeToken(TokenType.OpenBracket, 1);
            case ']': return MakeToken(TokenType.CloseBracket, 1);
            case '.': return MakeToken(TokenType.Dot, 1);
            case ',': return MakeToken(TokenType.Comma, 1);
            case ':': return MakeToken(TokenType.Colon, 1);
            case ';': return MakeToken(TokenType.Semicolon, 1);
            case '-':
                if (Peek(1) == '=') return MakeToken(TokenType.SubtractAssign, 2);
                if (Peek(1) == '>') return MakeToken(TokenType.Arrow, 2);
                return MakeToken(TokenType.Subtract, 1);
            case '=':
                if (Peek(1) == '=') return MakeToken(TokenType.Equals, 2);
                return MakeToken(TokenType.Assign, 1);
            case '+':
                if (Peek(1) == '=') return MakeToken(TokenType.AddAssign, 2);
                return MakeToken(TokenType.Add, 1);
            case '*':
                if (Peek(1) == '=') return MakeToken(TokenType.MultiplyAssign, 2);
                return MakeToken(TokenType.Multiply, 1);
            case '/':
                if (Peek(1) == '=') return MakeToken(TokenType.DivideAssign, 2);
                return MakeToken(TokenType.Divide, 1);
            case '%':
                if (Peek(1) == '=') return MakeToken(TokenType.ModuloAssign, 2);
                return MakeToken(TokenType.Modulo, 1);
            case '~': return MakeToken(TokenType.Bitnot, 1);
            case '^': return MakeToken(TokenType.Bitxor, 1);
            case '>':
                if (Peek(1) == '=') return MakeToken(TokenType.GreaterEqual, 2);
                if (Peek(1) == '>') return MakeToken(TokenType.RightShift, 2);
                return MakeToken(TokenType.Greater, 1);
            case '<':
                if (Peek(1) == '=') return MakeToken(TokenType.LessEqual, 2);
                if (Peek(1) == '<') return MakeToken(TokenType.LeftShift, 2);
                return MakeToken(TokenType.Less, 1);
            case '!':
                if (Peek(1) == '=') return MakeToken(TokenType.NotEquals, 2);
                return MakeToken(TokenType.Not, 1);
            case '&':
                if (Peek(1) == '&') return MakeToken(TokenType.And, 2);
                return MakeToken(TokenType.Bitand, 1);
            case '|':
                if (Peek(1) == '|') return MakeToken(TokenType.Or, 2);
                return MakeToken(TokenType.Bitor, 1);
            }

            // Literals

            // Integer literal
            if (char.IsDigit(ch))
            {
                int len = 1;
                for (; char.IsDigit(Peek(len)); ++len) ;
                return MakeToken(TokenType.IntLiteral, len);
            }
            // Identifier
            if (IsIdent(ch) || ch == '@')
            {
                int len = 1;
                for (; IsIdent(Peek(len)); ++len) ;
                var ident = MakeToken(TokenType.Identifier, len);
                // Determine if keyword
                var tokenType = ident.Value switch
                {
                    "proc"   => TokenType.KwProc    ,
                    "const"  => TokenType.KwConst   ,
                    "struct" => TokenType.KwStruct  ,
                    "true"   => TokenType.KwTrue    ,
                    "false"  => TokenType.KwFalse   ,
                    "if"     => TokenType.KwIf      ,
                    "else"   => TokenType.KwElse    ,
                    "while"  => TokenType.KwWhile   ,
                    "var"    => TokenType.KwVar     ,
                    "return" => TokenType.KwReturn  ,
                    _        => TokenType.Identifier,
                };

                return new Token(ident.Span, tokenType, ident.Value);
            }
            // String literal
            if (ch == '"')
            {
                int len = 1;
                while (true)
                {
                    var peek = Peek(len);
                    if (peek == '\0' || peek == '\r' || peek == '\n')
                    {
                        // Unclosed token, report an error
                        var tok = MakeToken(TokenType.StringLiteral, len);
                        ReportSyntaxError(new UnterminatedTokenError(tok, "\""));
                        return tok;
                    }
                    if (peek == '"') break;
                    if (peek == '\\') len += 2;
                    else ++len;
                }
                // NOTE: i + 1 because the last quote is not counted!
                return MakeToken(TokenType.StringLiteral, len + 1);
            }

            return MakeToken(TokenType.Unknown, 1);
        }

        private void ReportSyntaxError(ISyntaxError syntaxError) =>
            SyntaxError?.Invoke(this, syntaxError);

        private Token MakeToken(TokenType tokenType, int len)
        {
            var pos = cursor.Position;
            var content = Consume(len);
            var span = new Span(source, pos, cursor.Position);
            return new Token(span, tokenType, content);
        }

        private string Consume(int len)
        {
            var peekContent = peekBuffer.ToString().Substring(0, len);
            peekBuffer.Remove(0, len);
            cursor.Append(peekContent);
            return peekContent;
        }

        private bool Matches(string str)
        {
            Peek(str.Length);
            for (int i = 0; i < str.Length; ++i)
            {
                if (peekBuffer[i] != str[i]) return false;
            }
            return true;
        }

        private char Peek(int amount, char eof = '\0')
        {
            while (peekBuffer.Length <= amount)
            {
                var code = reader.Read();
                var ch = code == -1 ? eof : (char)code;
                peekBuffer.Append(ch);
            }
            return peekBuffer[amount];
        }

        private static bool IsIdent(char ch) =>
            char.IsLetterOrDigit(ch) || ch == '_';
    }
}
