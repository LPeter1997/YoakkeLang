namespace Yoakke.Lir.Backend.Toolchain
{
    /// <summary>
    /// Interface for assemblers.
    /// </summary>
    public interface IAssembler : ITool
    {
        /// <summary>
        /// Assembles the given source files.
        /// </summary>
        /// <param name="build">The <see cref="Build"/> definition for the compilation.</param>
        /// <returns>The error code. 0 if succeeded.</returns>
        public int Assemble(Build build) => Execute(build);
    }
}
