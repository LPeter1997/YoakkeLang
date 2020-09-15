using System;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir
{
    /// <summary>
    /// An external symbol.
    /// </summary>
    public class Extern : Value, ISymbol
    {
        public override Type Type { get; }
        public string Name { get; }
        public Visibility Visibility { get; set; } = Visibility.Public;

        /// <summary>
        /// The path of the binary the symbol originates from.
        /// </summary>
        public readonly string Path;

        /// <summary>
        /// Initializes a new <see cref="Extern"/>.
        /// </summary>
        /// <param name="name">The name of the external symbol.</param>
        /// <param name="type">The <see cref="Type"/> of the external symbol.</param>
        /// <param name="path">The path of the binary the symbol originates from.</param>
        public Extern(string name, Type type, string path)
        {
            Name = name;
            Type = type;
            Path = path;
        }

        public override string ToValueString() => Name;
        public override string ToString() =>
            $"extern {Type} {Name} [source = \"{Path}\"]";
        public override bool Equals(Value? other) => ReferenceEquals(this, other);
        public override int GetHashCode() => HashCode.Combine(typeof(Extern), Name);
        // NOTE: Makes no sense to clone this
        public override Value Clone() => this;
    }
}
