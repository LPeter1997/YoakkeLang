using Yoakke.Lir.Types;
using Yoakke.Lir.Values;

namespace Yoakke.Lir
{
    /// <summary>
    /// Storage type for the VM.
    /// </summary>
    public record Register : Value
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

        public override string ToString() => $"{Type} {ToValueString()}";
    }
}
