using System;

namespace Yoakke.Compiler.Semantic.Types
{
    partial class Type
    {
        /// <summary>
        /// A primitive Lir <see cref="Type"/> reference.
        /// </summary>
        public class Prim : Type
        {
            /// <summary>
            /// The name identifier. This is to differentiate different primitives with the same backing Lir type.
            /// </summary>
            public readonly string Name;
            /// <summary>
            /// The Lir <see cref="Type"/>.
            /// </summary>
            public readonly Lir.Types.Type Type;

            /// <summary>
            /// Initializes a new <see cref="Prim"/>.
            /// </summary>
            /// <param name="name">The name identifier.</param>
            /// <param name="type">The Lir <see cref="Type"/> to wrap.</param>
            public Prim(string name, Lir.Types.Type type)
                : base(new Scope(ScopeKind.Struct, null))
            {
                Name = name;
                Type = type;
            }

            public override bool Equals(Type? other) =>
                other is Prim p && Name == p.Name && Type.Equals(p.Type);
            public override int GetHashCode() => HashCode.Combine(typeof(Prim), Name, Type);
            public override string ToString() => Name;
        }
    }
}
