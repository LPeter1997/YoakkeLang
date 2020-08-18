using Yoakke.Lir.Types;

namespace Yoakke.Lir.Values
{
    /// <summary>
    /// Base for every value.
    /// </summary>
    public abstract partial record Value 
    { 
        /// <summary>
        /// The type of this <see cref="Value"/>.
        /// </summary>
        public abstract Type Type { get; }

        public abstract override string ToString();
    }
}
