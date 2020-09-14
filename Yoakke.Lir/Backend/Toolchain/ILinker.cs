using System.Collections.Generic;

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
        /// The entry point of the binary.
        /// </summary>
        public string EntryPoint { get; set; }
        /// <summary>
        /// The list of <see cref="ISymbol"/>s the linker exports into a DLL.
        /// </summary>
        public IList<ISymbol> Exports { get; }

        /// <summary>
        /// Links the given source files.
        /// </summary>
        /// <param name="outputPath">The output path of the result.</param>
        /// <returns>The error code. 0 if succeeded.</returns>
        public int Link(string outputPath) => Execute(outputPath);
    }
}
