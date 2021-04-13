using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic.Types;

namespace Yoakke.Compiler.Symbols
{
    /// <summary>
    /// Symbols that have a specific type.
    /// </summary>
    public interface ITypedSymbol : ISymbol
    {
        /// <summary>
        /// The type of the symbol.
        /// </summary>
        public Type Type { get; }
    }
}
