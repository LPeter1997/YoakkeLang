using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Yoakke.Compiler.Syntax
{
    /// <summary>
    /// Represents an atom in the language's grammar and the lowest level element of parsing.
    /// </summary>
#pragma warning disable CS0659, CS0661
    public readonly struct Token : IEquatable<Token>
#pragma warning restore CS0661, CS0659
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
        /// The ending <see cref="Position"/> of this <see cref="Token"/>.
        /// </summary>
        public Position EndPosition => Position.Advance(Value.Length);

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

        public override bool Equals(object? obj) =>
            obj != null && obj is Token t && Equals(t);

        public bool Equals(Token other) =>
            // NOTE: This must uniquely identify the token
            Position == other.Position;

        public static bool operator ==(Token t1, Token t2) =>
            t1.Equals(t2);

        public static bool operator !=(Token t1, Token t2) =>
            !(t1 == t2);
    }

    /// <summary>
    /// The categories a <see cref="Token"/> can be.
    /// </summary>
    public enum TokenType
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
        /// Anything builtin by the compiler: '@[A-Za-z0-9_]*'.
        /// </summary>
        IntrinsicIdentifier,
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
        /// <summary>
        /// The keyword 'if'.
        /// </summary>
        KwIf,
        /// <summary>
        /// The keyword 'else'.
        /// </summary>
        KwElse,
        /// <summary>
        /// The keyword 'var'.
        /// </summary>
        KwVar,
    }
}
