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
        public void Assemble(Build build) => Execute(build);
    }
}
