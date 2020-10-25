using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            public readonly IValueDictionary<string, Type> Fields;

            public override Scope? DefinedScope { get; }

            /// <summary>
            /// Initializes a new <see cref="Struct"/>.
            /// </summary>
            /// <param name="kwStruct">The 'struct' <see cref="Token"/>.</param>
            /// <param name="fields">The names to field <see cref="Type"/>s dictionary.</param>
            /// <param name="definedScope">The <see cref="Scope"/> this struct type defines.</param>
            public Struct(Token kwStruct, IDictionary<string, Type> fields, Scope? definedScope)
            {
                KwStruct = kwStruct;
                Fields = fields.AsValueDictionary();
                DefinedScope = definedScope;
            }

            public override bool Equals(Type? other) =>
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
