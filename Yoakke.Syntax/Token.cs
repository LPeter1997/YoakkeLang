using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Text;

namespace Yoakke.Syntax
{
    /// <summary>
    /// Represents an atom in the language's grammar as the lowest level element of parsing.
    /// </summary>
#pragma warning disable CS0660, CS0661 // No reason to override Equals or GetHashCode
    public readonly struct Token
#pragma warning restore CS0660, CS0661
    {
        /// <summary>
        /// The source <see cref="Span"/> of this <see cref="Token"/>.
        /// </summary>
        public readonly Span Span;
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
        /// <param name="span">The <see cref="Span"/> of the token.</param>
        /// <param name="type">The <see cref="TokenType"/> of the token.</param>
        /// <param name="value">The textual value of the token.</param>
        public Token(Span span, TokenType type, string value)
        {
            Span = span;
            Type = type;
            Value = value;
        }

        public static bool operator ==(Token t1, Token t2) => t1.Equals(t2);
        public static bool operator !=(Token t1, Token t2) => !(t1 == t2);
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

        // Comments

        /// <summary>
        /// A comment that starts at a '//' and ends at the end of line.
        /// </summary>
        LineComment,

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
        /// Anything that matches the regex '[@A-Za-z_][A-Za-z0-9_]*'.
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
        /// <summary>
        /// The keyword 'if'.
        /// </summary>
        KwIf,
        /// <summary>
        /// The keyword 'else'.
        /// </summary>
        KwElse,
        /// <summary>
        /// The keyword 'while'.
        /// </summary>
        KwWhile,
        /// <summary>
        /// The keyword 'var'.
        /// </summary>
        KwVar,
        /// <summary>
        /// The keyword 'return'.
        /// </summary>
        KwReturn,
    }
}
