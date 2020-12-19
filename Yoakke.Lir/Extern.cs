using System;
using Yoakke.Lir.Status;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir
{
    /// <summary>
    /// An external symbol.
    /// </summary>
    public class Extern : Value, ISymbol, IValidate
    {
        public override Type Type { get; }
        public string Name { get; }
        public Visibility Visibility { get; set; } = Visibility.Public;

        /// <summary>
        /// The actual linked symbol name that the linker will search for.
        /// This is different from <see cref="Name"/>, as that is for identifying the external in the IR code.
        /// </summary>
        public readonly string? LinkName;
        // TODO: Do we even want this nullable?
        // We did it because of @extern
        /// <summary>
        /// The path of the binary the symbol originates from.
        /// </summary>
        public readonly string? Path;

        /// <summary>
        /// Initializes a new <see cref="Extern"/>.
        /// </summary>
        /// <param name="name">The name of the external symbol.</param>
        /// <param name="type">The <see cref="Type"/> of the external symbol.</param>
        /// <param name="linkName">The actual linked symbol name that the linker will search for.</param>
        /// <param name="path">The path of the binary the symbol originates from.</param>
        public Extern(string name, Type type, string linkName, string? path)
        {
            Name = name;
            Type = type;
            LinkName = linkName;
            Path = path;
        }

        public override string ToValueString() => Name;
        public override string ToString() =>
            $"extern [link_name = \"{LinkName}\"] " +
            $"{Type.ToTypeString()} {Name}{(Path == null ? string.Empty : $" [source = \"{Path}\"]")}";
        public override bool Equals(Value? other) => ReferenceEquals(this, other);
        public override int GetHashCode() => HashCode.Combine(typeof(Extern), Name);
        // NOTE: Makes no sense to clone this
        public override Value Clone() => this;

        public void Validate(BuildStatus status)
        {
            // No-op
        }
    }
}
