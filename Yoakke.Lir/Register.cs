using Yoakke.Lir.Types;

namespace Yoakke.Lir
{
    /// <summary>
    /// Storage type for the VM.
    /// </summary>
    public record Register
    {
        /// <summary>
        /// The <see cref="Type"/> this <see cref="Register"/> stores.
        /// </summary>
        public Type Type { get; set; }
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

        public override string ToString() => $"{Type} r{Index}";
    }
}
