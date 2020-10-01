using System;
using Yoakke.Lir.Status;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir
{
    /// <summary>
    /// A global value definition.
    /// </summary>
    public class Global : Value, ISymbol, IValidate
    {
        public override Type Type { get; }
        public string Name { get; set; }
        public Visibility Visibility { get; set; } = Visibility.Private;

        /// <summary>
        /// The actual value <see cref="Type"/> of this <see cref="Global"/>.
        /// </summary>
        public Type UnderlyingType => ((Type.Ptr)Type).Subtype;

        /// <summary>
        /// Initializes a new <see cref="Global"/>.
        /// </summary>
        /// <param name="name">The name of the global symbol.</param>
        /// <param name="type">The <see cref="Type"/> of the global symbol.</param>
        public Global(string name, Type type)
        {
            Name = name;
            Type = new Type.Ptr(type);
        }

        public override string ToValueString() => Name;
        public override string ToString() =>
            $"global {UnderlyingType} {Name} [visibility = {Visibility}]";
        public override bool Equals(Value? other) => ReferenceEquals(this, other);
        public override int GetHashCode() => HashCode.Combine(typeof(Global), Name);
        // NOTE: Makes no sense to clone this
        public override Value Clone() => this;

        public void Validate(BuildStatus status)
        {
            // No-op
        }
    }
}
