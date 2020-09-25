using Yoakke.Lir.Types;

namespace Yoakke.Lir
{
    /// <summary>
    /// Anything that has a name and can be exported in a DLL.
    /// </summary>
    public interface ISymbol : IValidate
    {
        /// <summary>
        /// The <see cref="Type"/> of this <see cref="ISymbol"/>.
        /// </summary>
        public Type Type { get; }
        /// <summary>
        /// The name of the <see cref="ISymbol"/>.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The <see cref="Visibility"/> of the <see cref="ISymbol"/>.
        /// </summary>
        public Visibility Visibility { get; set; }
    }
}
