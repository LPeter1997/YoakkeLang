using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.C.Syntax.Cpp;
using Yoakke.Text;

namespace Yoakke.C.Syntax
{
    /// <summary>
    /// A C lexer that breaks up the source into <see cref="Token"/>s.
    /// </summary>
    public class Lexer
    {
        private SourceFile source;
        private CppTextReader reader;
        private Cursor cursor = new Cursor();

        // NOTE: Since we don't have proper peeking in CppTextReader, we just save positions for peeks now
        private List<(Position, char)> peekBuffer = new List<(Position, char)>();

        /// <summary>
        /// Returns an <see cref="IEnumerable{Token}"/> for the given <see cref="SourceFile"/>, that lexes
        /// until EOF is read.
        /// </summary>
        /// <param name="source">The <see cref="SourceFile"/> to lex.</param>
        /// <returns>The <see cref="IEnumerable{Token}"/> of the lexed input.</returns>
        public static IEnumerable<Token> Lex(SourceFile source)
        {
            var lexer = new Lexer(source);

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
        /// <param name="source">The <see cref="SourceFile"/> to lex.</param>
        public Lexer(SourceFile source)
        {
            this.source = source;
            reader = new CppTextReader(new StringReader(source.Text));
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
            char Map(char ch, char from, char to) => ch == from ? to : from;

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
                // NOTE: A bit hairy logic but basically both EOF and \r become \n
                for (; Map(Peek(len, '\n'), '\r', '\n') != '\n'; ++len) ;
                // NOTE: The GCC doc says that comments should logically become single spaces
                ConsumeComment(len);
                return null;
            }
            // Multi-line (block) comment
            if (Matches("/*"))
            {
                int len = 2;
                while (true)
                {
                    if (Peek(len) == '\0')
                    {
                        // TODO: Unexpected eof
                        throw new NotImplementedException("Unexpected EOF in block comment!");
                    }
                    else if (Peek(len) == '*' && Peek(len + 1) == '/')
                    {
                        // End of block comment
                        len += 2;
                        break;
                    }
                    else
                    {
                        // Any other character, skip it
                        ++len;
                    }
                }
                ConsumeComment(len);
                return null;
            }

            // Punctuation and operators
            switch (ch)
            {
            case '(': return MakeToken(TokenType.OpenParen, 1);
            case ')': return MakeToken(TokenType.CloseParen, 1);
            case '{': return MakeToken(TokenType.OpenBrace, 1);
            case '}': return MakeToken(TokenType.CloseBrace, 1);
            case '[': return MakeToken(TokenType.OpenBracket, 1);
            case ']': return MakeToken(TokenType.CloseBracket, 1);
            case '.': 
                if (Matches("...")) return MakeToken(TokenType.Ellipsis, 3);
                return MakeToken(TokenType.Dot, 1);
            case ',': return MakeToken(TokenType.Comma, 1);
            case ':': return MakeToken(TokenType.Colon, 1);
            case ';': return MakeToken(TokenType.Semicolon, 1);
            case '?': return MakeToken(TokenType.QuestionMark, 1);
            case '#': 
                if (Peek(1) == '#') return MakeToken(TokenType.HashHash, 2);
                return MakeToken(TokenType.Hash, 1);
            case '=':
                if (Peek(1) == '=') return MakeToken(TokenType.Equal, 2);
                return MakeToken(TokenType.Assign, 1);
            case '+':
                if (Peek(1) == '+') return MakeToken(TokenType.Increment, 2);
                if (Peek(1) == '=') return MakeToken(TokenType.AddAssign, 2);
                return MakeToken(TokenType.Add, 1);
            case '-':
                if (Peek(1) == '-') return MakeToken(TokenType.Decrement, 2);
                if (Peek(1) == '=') return MakeToken(TokenType.SubtractAssign, 2);
                return MakeToken(TokenType.Subtract, 1);
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
            case '^':
                if (Peek(1) == '=') return MakeToken(TokenType.BitxorAssign, 2);
                return MakeToken(TokenType.Bitxor, 1);
            case '>':
                if (Matches(">>=")) return MakeToken(TokenType.RightShiftAssign, 3);
                if (Peek(1) == '=') return MakeToken(TokenType.GreaterEqual, 2);
                if (Peek(1) == '>') return MakeToken(TokenType.RightShift, 2);
                return MakeToken(TokenType.Greater, 1);
            case '<':
                if (Matches("<<=")) return MakeToken(TokenType.LeftShiftAssign, 3);
                if (Peek(1) == '=') return MakeToken(TokenType.LessEqual, 2);
                if (Peek(1) == '<') return MakeToken(TokenType.LeftShift, 2);
                return MakeToken(TokenType.Less, 1);
            case '!':
                if (Peek(1) == '=') return MakeToken(TokenType.NotEqual, 2);
                return MakeToken(TokenType.Not, 1);
            case '&':
                if (Peek(1) == '=') return MakeToken(TokenType.BitandAssign, 2);
                if (Peek(1) == '&') return MakeToken(TokenType.And, 2);
                return MakeToken(TokenType.Bitand, 1);
            case '|':
                if (Peek(1) == '=') return MakeToken(TokenType.BitorAssign, 2);
                if (Peek(1) == '|') return MakeToken(TokenType.Or, 2);
                return MakeToken(TokenType.Bitor, 1);
            }

            // Literals

            // Integer or float literal
            if (char.IsDigit(ch))
            {
                // TODO
                int len = 1;
                for (; char.IsDigit(Peek(len)); ++len) ;
                return MakeToken(TokenType.IntLiteral, len);
            }
            // Identifier
            if (IsIdent(ch))
            {
                int len = 1;
                for (; IsIdent(Peek(len)); ++len) ;
                var ident = MakeToken(TokenType.Identifier, len);
                // Determine if keyword
                var tokenType = ident.Value switch
                {
                    "auto"       => TokenType.KwAuto     ,
                    "_Bool"      => TokenType.KwBool     ,
                    "break"      => TokenType.KwBreak    ,
                    "case"       => TokenType.KwCase     ,
                    "char"       => TokenType.KwChar     ,
                    "_Complex"   => TokenType.KwComplex  ,
                    "const"      => TokenType.KwConst    ,
                    "continue"   => TokenType.KwContinue ,
                    "default"    => TokenType.KwDefault  ,
                    "do"         => TokenType.KwDo       ,
                    "double"     => TokenType.KwDouble   ,
                    "else"       => TokenType.KwElse     ,
                    "enum"       => TokenType.KwEnum     ,
                    "extern"     => TokenType.KwExtern   ,
                    "float"      => TokenType.KwFloat    ,
                    "for"        => TokenType.KwFor      ,
                    "goto"       => TokenType.KwGoto     ,
                    "if"         => TokenType.KwIf       ,
                    "_Imaginary" => TokenType.KwImaginary,
                    "inline"     => TokenType.KwInline   ,
                    "int"        => TokenType.KwInt      ,
                    "long"       => TokenType.KwLong     ,
                    "register"   => TokenType.KwRegister ,
                    "restrict"   => TokenType.KwRestrict ,
                    "return"     => TokenType.KwReturn   ,
                    "short"      => TokenType.KwShort    ,
                    "signed"     => TokenType.KwSigned   ,
                    "sizeof"     => TokenType.KwSizeof   ,
                    "static"     => TokenType.KwStatic   ,
                    "struct"     => TokenType.KwStruct   ,
                    "switch"     => TokenType.KwSwitch   ,
                    "typedef"    => TokenType.KwTypedef  ,
                    "union"      => TokenType.KwUnion    ,
                    "unsigned"   => TokenType.KwUnsigned ,
                    "void"       => TokenType.KwVoid     ,
                    "volatile"   => TokenType.KwVolatile ,
                    "while"      => TokenType.KwWhile    ,
                    _            => TokenType.Identifier ,
                };

                return new Token(ident.PhysicalSpan, ident.LogicalSpan, tokenType, ident.Value);
            }
            // String literal
            if (ch == '"')
            {
                int len = 1;
                while (true)
                {
                    var peek = Map(Peek(len, '\n'), '\r', '\n');
                    if (peek == '\n')
                    {
                        // TODO
                        throw new NotImplementedException("Unclosed string literal!");
                    }
                    if (peek == '"') break;
                    if (peek == '\\') len += 2;
                    else ++len;
                }
                // NOTE: i + 1 because the last quote is not counted!
                return MakeToken(TokenType.StringLiteral, len + 1);
            }

            // TODO: Char literal

            return MakeToken(TokenType.Unknown, 1);
        }

        private Token MakeToken(TokenType tokenType, int len)
        {
            var pos = cursor.Position;
            var (physicalSpan, content) = Consume(len);
            var logicalSpan = new Span(source, pos, cursor.Position);
            return new Token(physicalSpan, logicalSpan, tokenType, content);
        }

        private (Span, string) Consume(int len)
        {
            // Convert the peek to string
            var peekContent = string.Concat(peekBuffer.Take(len).Select(e => e.Item2));
            // Get the starting physical position
            var position = peekBuffer.First().Item1;
            // Remove it
            peekBuffer.RemoveRange(0, len);
            // Advance out cursor
            cursor.Append(peekContent);
            // Peek one more for the physical ending position
            Peek(0);
            var endPosition = peekBuffer.First().Item1;
            return (new Span(source, position, endPosition), peekContent);
        }

        private void ConsumeComment(int len)
        {
            peekBuffer.RemoveRange(0, len);
            cursor.Append(' ');
        }

        private bool Matches(string str)
        {
            Peek(str.Length);
            for (int i = 0; i < str.Length; ++i)
            {
                if (peekBuffer[i].Item2 != str[i]) return false;
            }
            return true;
        }

        private char Peek(int amount, char eof = '\0')
        {
            while (peekBuffer.Count <= amount)
            {
                var pos = reader.Position;
                var ch = reader.Next() ?? eof;
                peekBuffer.Add((pos, ch));
            }
            return peekBuffer[amount].Item2;
        }

        private static bool IsIdent(char ch) =>
            char.IsLetterOrDigit(ch) || ch == '_';
    }
}
