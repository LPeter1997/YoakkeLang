using System;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir
{
    /// <summary>
    /// Storage type for the VM.
    /// </summary>
    public class Register : Value
    {
        public override Type Type { get; }

        /// <summary>
        /// The register index.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Initializes a new <see cref="Register"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> this register stores.</param>
        /// <param name="index">The index of the <see cref="Register"/>.</param>
        public Register(Type type, int index)
        {
            Type = type;
            Index = index;
        }

        public override string ToValueString() => $"r{Index}";
        public override string ToString() => $"{Type.ToTypeString()} {ToValueString()}";
        public override bool Equals(Value? other) => ReferenceEquals(this, other);
        public override int GetHashCode() => HashCode.Combine(typeof(Register), Index);
        // NOTE: Makes no sense to clone this
        public override Value Clone() => this;
    }
}
