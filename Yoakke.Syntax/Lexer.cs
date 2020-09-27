using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Text;

namespace Yoakke.Syntax
{
    // TODO: We could factor out a SourceReader that wraps around a TextReader to track position?

    /// <summary>
    /// The lexer that splits the input into <see cref="Token"/>s.
    /// </summary>
    public class Lexer
    {
        private TextReader reader;
        private Cursor cursor = new Cursor();


        /// <summary>
        /// Returns an <see cref="IEnumerable{Token}"/> for the given <see cref="TextReader"/>, that lexes
        /// until EOF is read.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> to read from.</param>
        /// <returns>The <see cref="IEnumerable{Token}"/> of the lexed input.</returns>
        public static IEnumerable<Token> Lex(TextReader reader)
        {
            var lexer = new Lexer(reader);
            while (true)
            {
                var t = lexer.Next();
                yield return t;
                if (t.Type == TokenType.End) break;
            }
        }

        /// <summary>
        /// Initializes a new <see cref="Lexer"/>.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> to read from.</param>
        public Lexer(TextReader reader)
        {
            this.reader = reader;
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
                if (t != null) return t.Value;
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
                int len = 2;
                for (; Peek(len, '\n') != '\n'; ++len) ;
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
                if (Peek(1) == '>') return MakeToken(TokenType.Arrow, 2);
                return MakeToken(TokenType.Subtract, 1);
            case '=':
                if (Peek(1) == '=') return MakeToken(TokenType.Equal, 2);
                return MakeToken(TokenType.Assign, 1);
            case '+': return MakeToken(TokenType.Add, 1);
            case '*': return MakeToken(TokenType.Multiply, 1);
            case '/': return MakeToken(TokenType.Divide, 1);
            case '%': return MakeToken(TokenType.Modulo, 1);
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
                    "proc" => TokenType.KwProc,
                    "const" => TokenType.KwConst,
                    "struct" => TokenType.KwStruct,
                    "true" => TokenType.KwTrue,
                    "false" => TokenType.KwFalse,
                    "if" => TokenType.KwIf,
                    "else" => TokenType.KwElse,
                    "while" => TokenType.KwWhile,
                    "var" => TokenType.KwVar,
                    "return" => TokenType.KwReturn,
                    _ => TokenType.Identifier,
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
                    if (peek == '\0')
                    {
                        // TODO: Error
                        //var start = position;
                        //Consume(i - 1);
                        //throw new UnclosedDelimeterError(start, position, "string literal");
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

        private Token MakeToken(TokenType tokenType, int len)
        {
            var pos = cursor.Position;
            var content = Consume(len);
            var span = new Span(pos, cursor.Position);
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
