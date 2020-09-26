using System;
using Yoakke.DataStructures;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir
{
    /// <summary>
    /// A struct definition.
    /// </summary>
    public class StructDef : IEquatable<StructDef>
    {
        /// <summary>
        /// The name of this <see cref="StructDef"/>.
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// The field types this <see cref="StructDef"/> consists of.
        /// </summary>
        public readonly IValueList<Type> Fields = new ValueList<Type>();

        /// <summary>
        /// Initializes a new <see cref="StructDef"/>.
        /// </summary>
        /// <param name="name">The name of this struct defitition.</param>
        public StructDef(string name)
        {
            Name = name;
        }

        public override string ToString() => $"struct {Name} {{ {string.Join(", ", Fields)} }}";
        // NOTE: We don't consider the name for equality or hashing on purpose, so we can filter equivalent definitions!
        public bool Equals(StructDef? other) => other != null && Fields == other.Fields;
        public override int GetHashCode() => Fields.GetHashCode();
    }
}
