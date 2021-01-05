using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Yoakke.DataStructures;
using Yoakke.Text;

namespace Yoakke.C.Syntax
{
    // TODO: Remove the crappy sourcefile adapter pattern

    /// <summary>
    /// A C lexer that breaks up the source into <see cref="Token"/>s.
    /// Skips comments.
    /// </summary>
    public class Lexer : IEnumerator<Token>
    {
        /// <summary>
        /// Breaks up the given source into pre-processor <see cref="Token"/>s.
        /// </summary>
        /// <param name="sourceText">The source to break up.</param>
        /// <returns>The broken up <see cref="IEnumerable{Token}"/>.</returns>
        public static IEnumerable<Token> Lex(IEnumerable<PositionedChar> sourceText)
        {
            var lexer = new Lexer(sourceText);
            while (lexer.MoveNext()) yield return lexer.Current;
        }

        /// <summary>
        /// Same as <see cref="Lex(IEnumerable{PositionedChar})"/>.
        /// </summary>
        public static IEnumerable<Token> Lex(IEnumerable<char> sourceText) => Lex(Adapt(sourceText));

        /// <summary>
        /// Breaks up the given source into pre-processor <see cref="Token"/>s.
        /// Also assigns the <see cref="Token"/>s their <see cref="SourceFile"/>.
        /// </summary>
        /// <param name="sourceText">The source to break up.</param>
        /// <param name="sourceFile">The <see cref="SourceFile"/> to originate the <see cref="Token"/>s from.</param>
        /// <returns>The broken up <see cref="IEnumerable{Token}"/>.</returns>
        public static IEnumerable<Token> Lex(SourceFile sourceFile, IEnumerable<PositionedChar> sourceText) =>
            Lex(sourceText).Select(token =>
            {
                token.PhysicalSpan.Source = sourceFile;
                token.LogicalSpan.Source = sourceFile;
                return token;
            });

        /// <summary>
        /// Same as <see cref="Lex(SourceFile, IEnumerable{PositionedChar})"/>.
        /// </summary>
        public static IEnumerable<Token> Lex(SourceFile sourceFile, IEnumerable<char> sourceText) => 
            Lex(sourceFile, Adapt(sourceText));

        // Keep track of logical position
        private Cursor cursor = new Cursor();
        // We need physical positions too
        private PeekBuffer<PositionedChar> source;
        // These allow us to parse strings between <>, if we are after an include macro
        // The lastLine is the last token's line
        // The stateLine is the line the directive started
        // The stateCounter is 0 by default, 1 after # and 2 after include
        private int lastLine = -1;
        private int stateLine;
        private int stateCounter;

        private Token? current;

        public Token Current
        {
            get
            {
                if (current == null) throw new InvalidOperationException();
                return current;
            }
        }
        object IEnumerator.Current => Current;

        /// <summary>
        /// Initializes a new <see cref="Lexer"/>.
        /// </summary>
        /// <param name="source">The source <see cref="IEnumerable{PositionedChar}"/> that yields
        /// characters with physical positions.</param>
        public Lexer(IEnumerable<PositionedChar> source)
        {
            this.source = new PeekBuffer<PositionedChar>(source);
        }

        /// <summary>
        /// Initializes a new <see cref="Lexer"/>.
        /// </summary>
        /// <param name="source">The source to read, mapping physical positions to logical ones.</param>
        public Lexer(IEnumerable<char> source)
            : this(Adapt(source))
        {
        }

        public bool MoveNext()
        {
            if (current != null && current.Type == TokenType.End) return false;
            while (true)
            {
                var t = NextInternal();
                if (t != null)
                {
                    // Update the state
                    var newLastLine = t.LogicalSpan.End.Line;
                    if (lastLine != newLastLine) stateCounter = 0;
                    lastLine = newLastLine;
                    // Update the state counter
                    if (stateCounter == 0)
                    {
                        if (t.Type == TokenType.Hash)
                        {
                            stateLine = newLastLine;
                            stateCounter = 1;
                        }
                        else stateCounter = -1;
                    }
                    else if (stateCounter == 1 && t.Type == TokenType.Identifier && t.Value == "include") stateCounter = 2;
                    else stateCounter = 0;
                    // Assign current
                    current = t;
                    return true;
                }
            }
        }

        public void Reset() => throw new NotSupportedException();
        public void Dispose() { }

        private Token? NextInternal()
        {
            char Map(char ch, char from, char to) => ch == from ? to : ch;

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

            // Integer or float literal
            if (char.IsDigit(ch) || (ch == '.' && char.IsDigit(Peek(1))))
            {
                static bool IsE(char ch) => ch == 'e' || ch == 'E';
                static bool IsP(char ch) => ch == 'p' || ch == 'P';
                static bool IsX(char ch) => ch == 'x' || ch == 'X';
                static bool IsLlu(char ch) => ch == 'l' || ch == 'L' || ch == 'u' || ch == 'U';
                static bool IsSign(char ch) => ch == '+' || ch == '-';
                static bool IsFs(char ch) => ch == 'l' || ch == 'L' || ch == 'f' || ch == 'F';
                static bool IsHex(char ch)
                {
                    ch = char.ToLower(ch);
                    return char.IsDigit(ch) || (ch >= 'a' && ch <= 'f');
                }

                bool isFloat = ch == '.';
                int len = isFloat ? 2 : 1;
                if (isFloat)
                {
                    // We are past the first digit
                    for (; char.IsDigit(Peek(len)); ++len) ;
                    if (IsE(Peek(len)))
                    {
                        // Exponent
                        ++len;
                        if (IsSign(Peek(len))) ++len;
                        bool hasExponentDigits = false;
                        for (; char.IsDigit(Peek(len)); ++len) hasExponentDigits = true;
                        if (!hasExponentDigits)
                        {
                            // TODO
                            throw new NotImplementedException("Expected exponent digits!");
                        }
                    }
                    if (IsFs(Peek(len))) ++len;
                }
                else
                {
                    // NOTE: We do nothing exact here, we just consume everything correct
                    bool hex = false;
                    if (ch == '0' && IsX(Peek(1)))
                    {
                        hex = true;
                        ++len;
                    }
                    for (; hex ? IsHex(Peek(len)) : char.IsDigit(Peek(len)); ++len) ;
                    // LLU, ULL, U, LL, ...
                    if (IsLlu(Peek(len)))
                    {
                        // At most 3 chars
                        for (int i = 0; i < 3 && IsLlu(Peek(len)); ++i, ++len) ;
                    }
                    else
                    {
                        if (Peek(len) == '.')
                        {
                            isFloat = true;
                            ++len;
                            for (; hex ? IsHex(Peek(len)) : char.IsDigit(Peek(len)); ++len) ;
                        }
                        if (hex ? IsP(Peek(len)) : IsE(Peek(len)))
                        {
                            // Exponent
                            isFloat = true;
                            ++len;
                            if (IsSign(Peek(len))) ++len;
                            bool hasExponentDigits = false;
                            for (; char.IsDigit(Peek(len)); ++len) hasExponentDigits = true;
                            if (!hasExponentDigits)
                            {
                                // TODO
                                throw new NotImplementedException("Expected exponent digits!");
                            }
                        }
                        if (IsFs(Peek(len)))
                        {
                            isFloat = true;
                            ++len;
                        }
                    }
                }
                return MakeToken(isFloat ? TokenType.FloatLiteral : TokenType.IntLiteral, len);
            }

            // String literal
            if (ch == '"' || (ch == 'L' && Peek(1) == '"')
             // For include
             || (stateCounter == 2 && stateLine == cursor.Position.Line && ch == '<'))
            {
                var close = ch == '<' ? '>' : '\"';
                int len = ch == 'L' ? 2 : 1;
                while (true)
                {
                    var peek = Map(Peek(len, '\n'), '\r', '\n');
                    if (peek == '\n')
                    {
                        // TODO
                        throw new NotImplementedException("Unclosed string literal!");
                    }
                    if (peek == close) break;
                    if (peek == '\\') len += 2;
                    else ++len;
                }
                // NOTE: len + 1 because the last quote is not counted!
                return MakeToken(TokenType.StringLiteral, len + 1);
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
            case ':':
                if (Peek(1) == '>') return MakeToken(TokenType.CloseBracket, 2);
                return MakeToken(TokenType.Colon, 1);
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
                if (Peek(1) == '>') return MakeToken(TokenType.CloseBrace, 2);
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
                if (Peek(1) == '%') return MakeToken(TokenType.OpenBrace, 2);
                if (Peek(1) == ':') return MakeToken(TokenType.OpenBracket, 2);
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

            // Char literal
            if (ch == '\'' || (ch == 'L' && Peek(1) == '\''))
            {
                // NOTE: We just consume everything, even multi-char literals
                int len = ch == '\'' ? 1 : 2;
                while (true)
                {
                    if (Peek(len) == '\\') ++len;
                    // Consume character inside
                    ++len;
                    if (Peek(len) == '\'') break;
                }
                // NOTE: len + 1 because the last quote is not counted!
                return MakeToken(TokenType.CharLiteral, len + 1);
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
                    "auto" => TokenType.KwAuto,
                    "_Bool" => TokenType.KwBool,
                    "break" => TokenType.KwBreak,
                    "case" => TokenType.KwCase,
                    "char" => TokenType.KwChar,
                    "_Complex" => TokenType.KwComplex,
                    "const" => TokenType.KwConst,
                    "continue" => TokenType.KwContinue,
                    "default" => TokenType.KwDefault,
                    "do" => TokenType.KwDo,
                    "double" => TokenType.KwDouble,
                    "else" => TokenType.KwElse,
                    "enum" => TokenType.KwEnum,
                    "extern" => TokenType.KwExtern,
                    "float" => TokenType.KwFloat,
                    "for" => TokenType.KwFor,
                    "goto" => TokenType.KwGoto,
                    "if" => TokenType.KwIf,
                    "_Imaginary" => TokenType.KwImaginary,
                    "inline" => TokenType.KwInline,
                    "int" => TokenType.KwInt,
                    "long" => TokenType.KwLong,
                    "register" => TokenType.KwRegister,
                    "restrict" => TokenType.KwRestrict,
                    "return" => TokenType.KwReturn,
                    "short" => TokenType.KwShort,
                    "signed" => TokenType.KwSigned,
                    "sizeof" => TokenType.KwSizeof,
                    "static" => TokenType.KwStatic,
                    "struct" => TokenType.KwStruct,
                    "switch" => TokenType.KwSwitch,
                    "typedef" => TokenType.KwTypedef,
                    "union" => TokenType.KwUnion,
                    "unsigned" => TokenType.KwUnsigned,
                    "void" => TokenType.KwVoid,
                    "volatile" => TokenType.KwVolatile,
                    "while" => TokenType.KwWhile,
                    _ => TokenType.Identifier,
                };

                return new Token(ident.PhysicalSpan, ident.LogicalSpan, tokenType, ident.Value);
            }

            return MakeToken(TokenType.Unknown, 1);
        }

        private Token MakeToken(TokenType tokenType, int len)
        {
            var pos = cursor.Position;
            var (physicalSpan, content) = Consume(len);
            var logicalSpan = new Span(null, pos, cursor.Position);
            return new Token(physicalSpan, logicalSpan, tokenType, content);
        }

        private (Span, string) Consume(int len)
        {
            // Convert the peek to string
            var peekContent = string.Concat(source.Buffer.Take(len).Select(e => e.Char));
            // Get the starting physical position
            var position = PeekPositioned(0).Position;
            // Remove it
            source.Consume(len);
            // Advance our cursor
            cursor.Append(peekContent);
            // Peek one more for the physical ending position
            var endPosition = PeekPositioned(0).Position;
            return (new Span(null, position, endPosition), peekContent);
        }

        private void ConsumeComment(int len)
        {
            // NOTE: Just for safety we keep the comments in the logical representation
            // The original 2 lines were substituted
            //peekBuffer.RemoveRange(0, len);
            //cursor.Append(' ');
            Consume(len);
        }

        private bool Matches(string str)
        {
            Peek(str.Length);
            return source.Buffer
                .Select(pc => pc.Char)
                .Take(str.Length)
                .SequenceEqual(str);
        }

        private char Peek(int amount, char eof = '\0') => PeekPositioned(amount, eof).Char;
        private Position PeekDefaultPosition => 
            source.PrevOrDefault(new PositionedChar()).Position;

        private PositionedChar PeekPositioned(int amount, char eof = '\0') => 
            // TODO: Source file?
            source.PeekOrDefault(amount, new PositionedChar(null, PeekDefaultPosition, eof));

        private static bool IsIdent(char ch) =>
            char.IsLetterOrDigit(ch) || ch == '_';

        // Adapters for sources ////////////////////////////////////////////////

        private static IEnumerable<PositionedChar> Adapt(IEnumerable<char> chars)
        {
            var cursor = new Cursor();
            foreach (var ch in chars)
            {
                // TODO: Source file?
                yield return new PositionedChar(null, cursor.Position, ch);
                cursor.Append(ch);
            }
        }
    }
}
