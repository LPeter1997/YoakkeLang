using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.Syntax
{
    /// <summary>
    /// Position in the souce text.
    /// </summary>
    readonly struct Position
    {
        /// <summary>
        /// The <see cref="Source"/> this <see cref="Position"/> belongs to.
        /// </summary>
        public readonly Source Source;
        /// <summary>
        /// Zero-based line index.
        /// </summary>
        public readonly int Line;
        /// <summary>
        /// Zero-based column index.
        /// </summary>
        public readonly int Column;

        /// <summary>
        /// Initializes a new <see cref="Position"/>.
        /// </summary>
        /// <param name="source">The <see cref="Source"/> this position belongs to.</param>
        /// <param name="line">The zero-based line index.</param>
        /// <param name="column">The zero-based column index.</param>
        public Position(Source source, int line, int column)
        {
            Source = source;
            Line = line;
            Column = column;
        }

        /// <summary>
        /// Creates a <see cref="Position"/> that's advanced in the current line by the given amount.
        /// </summary>
        /// <param name="amount">The amount to advance in the current line.</param>
        /// <returns>The position in the same line, advanced by columns.</returns>
        public Position Advance(int amount = 1) =>
            new Position(source: Source, line: Line, column: Column + amount);

        /// <summary>
        /// Creates a <see cref="Position"/> that points to the first character of the next line.
        /// </summary>
        /// <returns>A position in the next line's first character.</returns>
        public Position Newline() =>
            new Position(source: Source, line: Line + 1, column: 0);

        public override string ToString() =>
            $"line {Line}, column {Column}";
    }

    /// <summary>
    /// Represents an atom in the language's grammar and the lowest level element of parsing.
    /// </summary>
    readonly struct Token
    {
        /// <summary>
        /// The <see cref="Position"/> of this <see cref="Token"/>.
        /// </summary>
        public readonly Position Position;
        /// <summary>
        /// The <see cref="TokenType"/> of this <see cref="Token"/>.
        /// </summary>
        public readonly TokenType Type;
        /// <summary>
        /// The text this <see cref="Token"/> was parsed from.
        /// </summary>
        public readonly string Value;

        /// <summary>
        /// Initializes a new <see cref="Token"/> instance.
        /// </summary>
        /// <param name="position">The <see cref="Position"/> of the token.</param>
        /// <param name="type">The <see cref="TokenType"/> of the token.</param>
        /// <param name="value">The textual value of the token.</param>
        public Token(Position position, TokenType type, string value)
        {
            Position = position;
            Type = type;
            Value = value;
        }
    }

    /// <summary>
    /// The categories a <see cref="Token"/> can be.
    /// </summary>
    enum TokenType
    {
        // Special cases

        /// <summary>
        /// Unknwon token, probably an error.
        /// </summary>
        Unknown,
        /// <summary>
        /// End of source.
        /// </summary>
        End,

        // Punctuation

        /// <summary>
        /// '('.
        /// </summary>
        OpenParen,
        /// <summary>
        /// ')'.
        /// </summary>
        CloseParen,
        /// <summary>
        /// '{'.
        /// </summary>
        OpenBrace,
        /// <summary>
        /// '}'.
        /// </summary>
        CloseBrace,
        /// <summary>
        /// '['.
        /// </summary>
        OpenBracket,
        /// <summary>
        /// ']'.
        /// </summary>
        CloseBracket,
        /// <summary>
        /// '.'.
        /// </summary>
        Dot,
        /// <summary>
        /// ','.
        /// </summary>
        Comma,
        /// <summary>
        /// ':'.
        /// </summary>
        Colon,
        /// <summary>
        /// ';'.
        /// </summary>
        Semicolon,
        /// <summary>
        /// '->'.
        /// </summary>
        Arrow,

        // Operators

        /// <summary>
        /// '='.
        /// </summary>
        Assign,
        /// <summary>
        /// '+'.
        /// </summary>
        Add,
        /// <summary>
        /// '-'.
        /// </summary>
        Subtract,
        /// <summary>
        /// '*'.
        /// </summary>
        Multiply,
        /// <summary>
        /// '/'.
        /// </summary>
        Divide,
        /// <summary>
        /// '%'.
        /// </summary>
        Modulo,
        /// <summary>
        /// '>'.
        /// </summary>
        Greater,
        /// <summary>
        /// '<'.
        /// </summary>
        Less,
        /// <summary>
        /// '>='.
        /// </summary>
        GreaterEqual,
        /// <summary>
        /// '<='.
        /// </summary>
        LessEqual,
        /// <summary>
        /// '=='.
        /// </summary>
        Equal,
        /// <summary>
        /// '!='.
        /// </summary>
        NotEqual,
        /// <summary>
        /// '&&'.
        /// </summary>
        And,
        /// <summary>
        /// '||'.
        /// </summary>
        Or,
        /// <summary>
        /// '!'.
        /// </summary>
        Not,

        // Literal values

        /// <summary>
        /// Anything that matches the regex '[A-Za-z_][A-Za-z0-9_]*'.
        /// </summary>
        Identifier,
        /// <summary>
        /// Anything that matches the regex '[0-9]+'.
        /// </summary>
        IntLiteral,
        /// <summary>
        /// Between double quotes, optional escape characters.
        /// </summary>
        StringLiteral,

        // Keywords

        /// <summary>
        /// The keyword 'proc'.
        /// </summary>
        KwProc,
        /// <summary>
        /// The keyword 'const'.
        /// </summary>
        KwConst,
        /// <summary>
        /// The keyword 'struct'.
        /// </summary>
        KwStruct,
        /// <summary>
        /// The keyword 'true'.
        /// </summary>
        KwTrue,
        /// <summary>
        /// The keyword 'false'.
        /// </summary>
        KwFalse,
    }
}
