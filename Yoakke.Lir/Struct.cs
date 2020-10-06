using System;
using System.Linq;
using Yoakke.DataStructures;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir
{
    /// <summary>
    /// A struct definition.
    /// </summary>
    public class Struct : Type
    {
        /// <summary>
        /// The name of this <see cref="Struct"/>.
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// The field types this <see cref="Struct"/> consists of.
        /// </summary>
        public readonly IValueList<Type> Fields = new ValueList<Type>();

        /// <summary>
        /// Initializes a new <see cref="Struct"/>.
        /// </summary>
        /// <param name="name">The name of this struct defitition.</param>
        public Struct(string name)
        {
            Name = name;
        }

        public override string ToTypeString() => Name;
        public override string ToString() => $"struct {Name} {{ {string.Join(", ", Fields.Select(f => f.ToTypeString()))} }}";
        // NOTE: We don't consider the name for equality or hashing on purpose, so we can filter equivalent definitions!
        public override bool Equals(Type? other) => other is Struct s && Fields.Equals(s.Fields);
        public override int GetHashCode() => HashCode.Combine(typeof(Struct), Fields);
    }
}
