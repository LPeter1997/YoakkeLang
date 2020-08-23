using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Backend.Toolchain
{
    /// <summary>
    /// Interface for archivers.
    /// </summary>
    public interface IArchiver : ITool
    {
        /// <summary>
        /// The <see cref="OutputKind"/> the archiver needs to produce.
        /// </summary>
        public OutputKind OutputKind { get; set; }

        /// <summary>
        /// Archives the given source files.
        /// </summary>
        /// <param name="outputPath">The output path of the result.</param>
        /// <returns>The error code. 0 if succeeded.</returns>
        public int Archive(string outputPath) => Execute(outputPath);
    }
}
