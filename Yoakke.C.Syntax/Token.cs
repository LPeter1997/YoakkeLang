using Yoakke.Text;

namespace Yoakke.C.Syntax
{
    /// <summary>
    /// A single C pre-processor token that also represents a regular C token.
    /// </summary>
    public class Token
    {
        /// <summary>
        /// The physical <see cref="Span"/> in the file.
        /// </summary>
        public Span PhysicalSpan;
        /// <summary>
        /// The logical <see cref="Span"/> with line-continuations.
        /// </summary>
        public Span LogicalSpan;
        /// <summary>
        /// The <see cref="TokenType"/> of this <see cref="Token"/>.
        /// </summary>
        public readonly TokenType Type;
        /// <summary>
        /// The text this <see cref="Token"/> was parsed from.
        /// </summary>
        public readonly string Value;

        public Token(Span physicalSpan, Span logicalSpan, TokenType type, string value)
        {
            PhysicalSpan = physicalSpan;
            LogicalSpan = logicalSpan;
            Type = type;
            Value = value;
        }

        /// <summary>
        /// Derives another <see cref="Token"/> from this one, keeping the spans.
        /// </summary>
        /// <param name="type">The <see cref="TokenType"/> of the derived <see cref="Token"/>.</param>
        /// <param name="value">The textual value of the derived <see cref="Token"/>.</param>
        /// <returns>The derived <see cref="Token"/>.</returns>
        public Token Derive(TokenType type, string value) =>
            new Token(PhysicalSpan, LogicalSpan, type, value);

        // TODO: Debug
        public override string ToString() => $"'{Value}'";
    }

    /// <summary>
    /// The different C <see cref="Token"/> types.
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
        /// ';'.
        /// </summary>
        Semicolon,
        /// <summary>
        /// ':'.
        /// </summary>
        Colon,
        /// <summary>
        /// '?'.
        /// </summary>
        QuestionMark,
        /// <summary>
        /// '...'.
        /// </summary>
        Ellipsis,
        /// <summary>
        /// '#'.
        /// </summary>
        Hash,
        /// <summary>
        /// '##'.
        /// </summary>
        HashHash,

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
        /// <summary>
        /// '++'.
        /// </summary>
        Increment,
        /// <summary>
        /// '--'.
        /// </summary>
        Decrement,
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
        /// <summary>
        /// '<<='.
        /// </summary>
        LeftShiftAssign,
        /// <summary>
        /// '>>='.
        /// </summary>
        RightShiftAssign,
        /// <summary>
        /// '&='.
        /// </summary>
        BitandAssign,
        /// <summary>
        /// '|='.
        /// </summary>
        BitorAssign,
        /// <summary>
        /// '^='.
        /// </summary>
        BitxorAssign,

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
        /// Some floating point number.
        /// </summary>
        FloatLiteral,
        /// <summary>
        /// Between double quotes, optional escape characters.
        /// </summary>
        StringLiteral,
        /// <summary>
        /// Between single quotes either an unescaped or escaped character, or character code.
        /// </summary>
        CharLiteral,

        // Keywords

        /// <summary>
        /// The keyword 'auto'.
        /// </summary>
        KwAuto,
        /// <summary>
        /// The keyword '_Bool'.
        /// </summary>
        KwBool,
        /// <summary>
        /// The keyword 'break'.
        /// </summary>
        KwBreak,
        /// <summary>
        /// The keyword 'case'.
        /// </summary>
        KwCase,
        /// <summary>
        /// The keyword 'char'.
        /// </summary>
        KwChar,
        /// <summary>
        /// The keyword '_Complex'.
        /// </summary>
        KwComplex,
        /// <summary>
        /// The keyword 'const'.
        /// </summary>
        KwConst,
        /// <summary>
        /// The keyword 'continue'.
        /// </summary>
        KwContinue,
        /// <summary>
        /// The keyword 'default'.
        /// </summary>
        KwDefault,
        /// <summary>
        /// The keyword 'do'.
        /// </summary>
        KwDo,
        /// <summary>
        /// The keyword 'double'.
        /// </summary>
        KwDouble,
        /// <summary>
        /// The keyword 'else'.
        /// </summary>
        KwElse,
        /// <summary>
        /// The keyword 'enum'.
        /// </summary>
        KwEnum,
        /// <summary>
        /// The keyword 'extern'.
        /// </summary>
        KwExtern,
        /// <summary>
        /// The keyword 'float'.
        /// </summary>
        KwFloat,
        /// <summary>
        /// The keyword 'for'.
        /// </summary>
        KwFor,
        /// <summary>
        /// The keyword 'goto'.
        /// </summary>
        KwGoto,
        /// <summary>
        /// The keyword 'if'.
        /// </summary>
        KwIf,
        /// <summary>
        /// The keyword '_Imaginary'.
        /// </summary>
        KwImaginary,
        /// <summary>
        /// The keyword 'inline'.
        /// </summary>
        KwInline,
        /// <summary>
        /// The keyword 'int'.
        /// </summary>
        KwInt,
        /// <summary>
        /// The keyword 'long'.
        /// </summary>
        KwLong,
        /// <summary>
        /// The keyword 'register'.
        /// </summary>
        KwRegister,
        /// <summary>
        /// The keyword 'restrict'.
        /// </summary>
        KwRestrict,
        /// <summary>
        /// The keyword 'return'.
        /// </summary>
        KwReturn,
        /// <summary>
        /// The keyword 'short'.
        /// </summary>
        KwShort,
        /// <summary>
        /// The keyword 'signed'.
        /// </summary>
        KwSigned,
        /// <summary>
        /// The keyword 'sizeof'.
        /// </summary>
        KwSizeof,
        /// <summary>
        /// The keyword 'static'.
        /// </summary>
        KwStatic,
        /// <summary>
        /// The keyword 'struct'.
        /// </summary>
        KwStruct,
        /// <summary>
        /// The keyword 'switch'.
        /// </summary>
        KwSwitch,
        /// <summary>
        /// The keyword 'typedef'.
        /// </summary>
        KwTypedef,
        /// <summary>
        /// The keyword 'union'.
        /// </summary>
        KwUnion,
        /// <summary>
        /// The keyword 'unsigned'.
        /// </summary>
        KwUnsigned,
        /// <summary>
        /// The keyword 'void'.
        /// </summary>
        KwVoid,
        /// <summary>
        /// The keyword 'volatile'.
        /// </summary>
        KwVolatile,
        /// <summary>
        /// The keyword 'while'.
        /// </summary>
        KwWhile,
    }
}
