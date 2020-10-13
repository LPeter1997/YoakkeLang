using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Yoakke.Syntax.ParseTree;
using Yoakke.Text;

namespace Yoakke.Syntax
{
    /// <summary>
    /// Represents an atom in the language's grammar as the lowest level element of parsing.
    /// </summary>
    public class Token : IEquatable<Token>, IParseTreeElement
    {
        public Span Span { get; }
        public IEnumerable<Token> Tokens
        {
            get
            {
                yield return this;
            }
        }

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

        public override bool Equals(object? obj) => obj is Token t && Equals(t);

        public bool Equals(Token? other) =>
               other is not null 
            && Span.Equals(other.Span)
            && Type == other.Type 
            && Value == other.Value;

        public override int GetHashCode() => HashCode.Combine(Span, Type, Value);
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
        Equals,
        /// <summary>
        /// '!='.
        /// </summary>
        NotEquals,
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
        /// <summary>
        /// '&'.
        /// </summary>
        Bitand,
        /// <summary>
        /// '|'.
        /// </summary>
        Bitor,
        /// <summary>
        /// '^'.
        /// </summary>
        Bitxor,
        /// <summary>
        /// '~'.
        /// </summary>
        Bitnot,
        /// <summary>
        /// '<<'.
        /// </summary>
        LeftShift,
        /// <summary>
        /// '>>'.
        /// </summary>
        RightShift,

        // Compound assignment operators

        /// <summary>
        /// '+='.
        /// </summary>
        AddAssign,
        /// <summary>
        /// '-='.
        /// </summary>
        SubtractAssign,
        /// <summary>
        /// '*='.
        /// </summary>
        MultiplyAssign,
        /// <summary>
        /// '/='.
        /// </summary>
        DivideAssign,
        /// <summary>
        /// '%='.
        /// </summary>
        ModuloAssign,

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
