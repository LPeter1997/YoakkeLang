using System;
using System.Collections.Generic;
using System.Linq;
using Yoakke.DataStructures;
using Yoakke.Syntax;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Semantic.Types
{
    partial class Type
    {
        /// <summary>
        /// A struct <see cref="Type"/>.
        /// </summary>
        public class Struct : Type
        {
            /// <summary>
            /// A single struct field.
            /// </summary>
            public class Field : IEquatable<Field>
            {
                /// <summary>
                /// The <see cref="Type"/> of this <see cref="Field"/>.
                /// </summary>
                public readonly DataStructures.Lazy<Type> Type;
                /// <summary>
                /// The definition in the AST.
                /// </summary>
                public readonly Expression.StructType.Field? Definition;

                // TODO: Doc
                public Field(DataStructures.Lazy<Type> type, Expression.StructType.Field? def)
                {
                    Type = type;
                    Definition = def;
                }

                public override bool Equals(object? obj) => obj is Field f && Equals(f);
                // NOTE: We don'"t consider definition on purpose
                public bool Equals(Field? other) => other != null && other.Type.Equals(Type);
                public override int GetHashCode() => HashCode.Combine(Type);
                public override string ToString() => Type.Value.ToString();
            }

            /// <summary>
            /// The defining 'struct' keyword.
            /// </summary>
            public readonly Token KwStruct;
            /// <summary>
            /// The names to field <see cref="Type"/>s.
            /// </summary>
            public readonly IValueDictionary<string, Field> Fields;

            /// <summary>
            /// Initializes a new <see cref="Struct"/>.
            /// </summary>
            /// <param name="definedScope">The <see cref="Scope"/> this struct type defines.</param>
            /// <param name="kwStruct">The 'struct' <see cref="Token"/>.</param>
            /// <param name="fields">The names to field <see cref="Type"/>s dictionary.</param>
            public Struct(Scope definedScope, Token kwStruct, IDictionary<string, Field> fields)
                : base(definedScope)
            {
                KwStruct = kwStruct;
                Fields = fields.AsValueDictionary();
            }

            protected override bool EqualsExact(Type? other) =>
                other is Struct s
                && KwStruct.Equals(s.KwStruct)
                && Fields.Equals(s.Fields);
            public override int GetHashCode() =>
                HashCode.Combine(typeof(Struct), KwStruct, Fields);
            public override string ToString() =>
                $"struct {{ {string.Join("; ", Fields.Select(kv => $"{kv.Key}: {kv.Value}"))} }}";
        }
    }
}
