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
    public interface ILinker
    {
        /// <summary>
        /// The <see cref="TargetTriplet"/> the linker targets.
        /// </summary>
        public TargetTriplet TargetTriplet { get; set; }
        /// <summary>
        /// The files that need to be linked.
        /// </summary>
        public IList<string> Files { get; }
        /// <summary>
        /// The <see cref="OutputKind"/> the linker needs to produce.
        /// </summary>
        public OutputKind OutputKind { get; set; }

        /// <summary>
        /// Links the given <see cref="Files"/>.
        /// </summary>
        /// <param name="outputPath">The output path of the result.</param>
        public void Link(string outputPath);
    }
}
