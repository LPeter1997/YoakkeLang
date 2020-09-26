using System;

namespace Yoakke.Lir.Types
{
    partial class Type
    {
        /// <summary>
        /// Struct type. Basically referrs to the definition.
        /// </summary>
        public class Struct : Type
        {
            public readonly StructDef Definition;

            public Struct(StructDef def)
            {
                Definition = def;
            }

            public override string ToString() => Definition.Name;
            public override bool Equals(Type? other) =>
                other is Struct s && Definition.Equals(s.Definition);
            public override int GetHashCode() => HashCode.Combine(typeof(Struct), Definition);
        }
    }
}
