namespace Yoakke.Lir.Backend.Toolchain
{
    /// <summary>
    /// Interface for linkers.
    /// </summary>
    public interface ILinker : ITool
    {
        /// <summary>
        /// Links the given source files.
        /// </summary>
        /// <param name="build">The <see cref="Build"/> definition for the compilation.</param>
        /// <returns>The error code. 0 if succeeded.</returns>
        public int Link(Build build) => Execute(build);
    }
}
