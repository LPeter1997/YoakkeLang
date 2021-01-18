using System;
using System.Collections.Generic;
using System.Linq;
using Yoakke.DataStructures;
using Yoakke.Syntax;

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
            /// The defining 'struct' keyword.
            /// </summary>
            public readonly Token KwStruct;
            /// <summary>
            /// The names to field <see cref="Type"/>s.
            /// </summary>
            public readonly IValueDictionary<string, DataStructures.Lazy<Type>> Fields;

            /// <summary>
            /// Initializes a new <see cref="Struct"/>.
            /// </summary>
            /// <param name="definedScope">The <see cref="Scope"/> this struct type defines.</param>
            /// <param name="kwStruct">The 'struct' <see cref="Token"/>.</param>
            /// <param name="fields">The names to field <see cref="Type"/>s dictionary.</param>
            public Struct(Scope definedScope, Token kwStruct, IDictionary<string, DataStructures.Lazy<Type>> fields)
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
