namespace Yoakke.Lir.Backend.Toolchain
{
    /// <summary>
    /// Interface for assemblers.
    /// </summary>
    public interface IAssembler : ITool
    {
        /// <summary>
        /// The <see cref="OutputKind"/> the assembler needs to produce.
        /// </summary>
        public OutputKind OutputKind { get; set; }

        /// <summary>
        /// Assembles the given source files.
        /// </summary>
        /// <param name="outputPath">The output path of the result.</param>
        /// <returns>The error code. 0 if succeeded.</returns>
        public int Assemble(string outputPath) => Execute(outputPath);

        /// <summary>
        /// Assembles the given source file.
        /// </summary>
        /// <param name="sourcePath">The source file to assemble.</param>
        /// <param name="outputPath">The output path of the result.</param>
        /// <returns>The error code. 0 if succeeded.</returns>
        public int Assemble(string sourcePath, string outputPath)
        {
            SourceFiles.Clear();
            SourceFiles.Add(sourcePath);
            return Assemble(outputPath);
        }
    }
}
