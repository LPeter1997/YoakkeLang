using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.DataStructures;
using Yoakke.Syntax;

namespace Yoakke.Compiler.Semantic
{
    /// <summary>
    /// Base class for every type representation.
    /// </summary>
    public abstract partial class Type : IEquatable<Type>
    {
        public override bool Equals(object? obj) => obj is Type ty && Equals(ty);

        public abstract bool Equals(Type? other);
        public abstract override int GetHashCode();
    }

    partial class Type
    {
        /// <summary>
        /// A primitive Lir <see cref="Type"/> reference.
        /// </summary>
        public class Prim : Type
        {
            /// <summary>
            /// The Lir <see cref="Type"/>.
            /// </summary>
            public readonly Lir.Types.Type Type;

            /// <summary>
            /// Initializes a new <see cref="Prim"/>.
            /// </summary>
            /// <param name="type">The Lir <see cref="Type"/> to wrap.</param>
            public Prim(Lir.Types.Type type)
            {
                Type = type;
            }

            public override bool Equals(Type? other) => other is Prim p && Type.Equals(p.Type);
            public override int GetHashCode() => HashCode.Combine(typeof(Prim), Type);
        }

        /// <summary>
        /// A struct <see cref="Type"/>.
        /// </summary>
        public class Struct : Type
        {
            /// <summary>
            /// The defining 'struct' keyword.
            /// </summary>
            public Token KwStruct;
            /// <summary>
            /// The names to field <see cref="Type"/>s.
            /// </summary>
            public IValueDictionary<string, Type> Fields;

            /// <summary>
            /// Initializes a new <see cref="Struct"/>.
            /// </summary>
            /// <param name="kwStruct">The 'struct' <see cref="Token"/>.</param>
            /// <param name="fields">The names to field <see cref="Type"/>s dictionary.</param>
            public Struct(Token kwStruct, IValueDictionary<string, Type> fields)
            {
                KwStruct = kwStruct;
                Fields = fields;
            }

            public override bool Equals(Type? other) =>
                other is Struct s
                && KwStruct.Equals(s.KwStruct)
                && Fields.Equals(s.Fields);
            public override int GetHashCode() =>
                HashCode.Combine(typeof(Struct), KwStruct, Fields);
        }
    }
}
