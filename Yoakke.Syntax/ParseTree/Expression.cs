using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Text;

namespace Yoakke.Syntax.ParseTree
{
    /// <summary>
    /// Base for every expression <see cref="Node"/>.
    /// </summary>
    public abstract partial class Expression : Node
    {
    }

    partial class Expression
    {
        /// <summary>
        /// A literal <see cref="Token"/>: identifier, string, int, bool, etc.
        /// </summary>
        public class Literal : Expression
        {
            public override Span Span => Token.Span;
            public override IEnumerable<IParseTreeElement> Children
            {
                get { yield return Token; }
            }

            /// <summary>
            /// The <see cref="Token"/>.
            /// </summary>
            public readonly Token Token;

            public Literal(Token token)
            {
                Token = token;
            }
        }

        /// <summary>
        /// A struct type definition.
        /// </summary>
        public class StructType : Expression
        {
            /// <summary>
            /// A single field inside a <see cref="StructType"/>.
            /// </summary>
            public class Field : Declaration
            {
                public override Span Span => new Span(Name.Span, Type.Span);
                public override IEnumerable<IParseTreeElement> Children
                {
                    get
                    {
                        yield return Name;
                        yield return Colon;
                        yield return Type;
                    }
                }

                /// <summary>
                /// The name of the field.
                /// </summary>
                public readonly Token Name;
                /// <summary>
                /// The ':'.
                /// </summary>
                public readonly Token Colon;
                /// <summary>
                /// The type of the field.
                /// </summary>
                public readonly Expression Type;

                public Field(Token name, Token colon, Expression type)
                {
                    Name = name;
                    Colon = colon;
                    Type = type;
                }
            }

            public override Span Span => new Span(Struct.Span, CloseBrace.Span);
            public override IEnumerable<IParseTreeElement> Children
            {
                get
                {
                    yield return Struct;
                    yield return OpenBrace;
                    foreach (var field in Fields) yield return field;
                    yield return CloseBrace;
                }
            }

            /// <summary>
            /// The 'struct' keyword.
            /// </summary>
            public readonly Token Struct;
            /// <summary>
            /// The '{'.
            /// </summary>
            public readonly Token OpenBrace;
            /// <summary>
            /// The <see cref="Field"/>s.
            /// </summary>
            public readonly IReadOnlyList<Field> Fields;
            /// <summary>
            /// The '}'.
            /// </summary>
            public readonly Token CloseBrace;

            public StructType(Token @struct, Token openBrace, IReadOnlyList<Field> fields, Token closeBrace)
            {
                Struct = @struct;
                OpenBrace = openBrace;
                Fields = fields;
                CloseBrace = closeBrace;
            }
        }

        /// <summary>
        /// A struct value initialization.
        /// </summary>
        public class StructValue : Expression
        {
            /// <summary>
            /// A single field initialization inside a <see cref="StructValue"/>.
            /// </summary>
            public class Field : Statement
            {
                public override Span Span => new Span(Name.Span, Value.Span);
                public override IEnumerable<IParseTreeElement> Children
                {
                    get
                    {
                        yield return Name;
                        yield return Assign;
                        yield return Value;
                    }
                }

                /// <summary>
                /// The name of the field.
                /// </summary>
                public readonly Token Name;
                /// <summary>
                /// The '='.
                /// </summary>
                public readonly Token Assign;
                /// <summary>
                /// The value of the field.
                /// </summary>
                public readonly Expression Value;

                public Field(Token name, Token assign, Expression value)
                {
                    Name = name;
                    Assign = assign;
                    Value = value;
                }
            }

            public override Span Span => new Span(Type.Span, CloseBrace.Span);
            public override IEnumerable<IParseTreeElement> Children
            {
                get
                {
                    yield return Type;
                    yield return OpenBrace;
                    foreach (var field in Fields) yield return field;
                    yield return CloseBrace;
                }
            }

            /// <summary>
            /// The struct type.
            /// </summary>
            public readonly Expression Type;
            /// <summary>
            /// The '{'.
            /// </summary>
            public readonly Token OpenBrace;
            /// <summary>
            /// The <see cref="Field"/>s.
            /// </summary>
            public readonly IReadOnlyList<Field> Fields;
            /// <summary>
            /// The '}'.
            /// </summary>
            public readonly Token CloseBrace;

            public StructValue(Expression type, Token openBrace, IReadOnlyList<Field> fields, Token closeBrace)
            {
                Type = type;
                OpenBrace = openBrace;
                Fields = fields;
                CloseBrace = closeBrace;
            }
        }
    }
}
