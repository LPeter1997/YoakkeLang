using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Backend.Toolchain
{
    /// <summary>
    /// Interface for linkers.
    /// </summary>
    public interface ILinker : ITool
    {
        /// <summary>
        /// The <see cref="OutputKind"/> the linker needs to produce.
        /// </summary>
        public OutputKind OutputKind { get; set; }

        /// <summary>
        /// Links the given source files.
        /// </summary>
        /// <param name="outputPath">The output path of the result.</param>
        /// <returns>The error code. 0 if succeeded.</returns>
        public int Link(string outputPath) => Execute(outputPath);
    }
}
