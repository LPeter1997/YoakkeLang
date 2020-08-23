using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Backend.Toolchain
{
    /// <summary>
    /// Interface for assemblers.
    /// </summary>
    public interface IAssembler
    {
        /// <summary>
        /// The <see cref="TargetTriplet"/> the assembler targets.
        /// </summary>
        public TargetTriplet TargetTriplet { get; set; }

        /// <summary>
        /// Assembles a given file.
        /// </summary>
        /// <param name="sourcePath">The source file to assemble.</param>
        /// <param name="outputPath">The output path of the result.</param>
        /// <returns>The error code. 0 if succeeded.</returns>
        public int Assemble(string sourcePath, string outputPath);
    }
}
