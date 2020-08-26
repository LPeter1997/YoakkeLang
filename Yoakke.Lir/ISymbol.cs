using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir
{
    /// <summary>
    /// Anything that has a name and can be exported in a DLL.
    /// </summary>
    public interface ISymbol
    {
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
