using Yoakke.Lir.Instructions;
using Yoakke.Lir.Types;

namespace Yoakke.Lir.Values
{
    /// <summary>
    /// Base for every value.
    /// </summary>
    public abstract partial record Value : IInstrArg
    { 
        /// <summary>
        /// The type of this <see cref="Value"/>.
        /// </summary>
        public abstract Type Type { get; }

        /// <summary>
        /// Converts this <see cref="Value"/> to it's string representation.
        /// </summary>
        /// <returns>The string representation of this <see cref="Value"/>.</returns>
        public abstract string ToValueString();

        public override string ToString() => ToValueString();
    }
}
