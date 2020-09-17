namespace Yoakke.Lir.Backend.Toolchain
{
    /// <summary>
    /// Interface for archivers.
    /// </summary>
    public interface IArchiver : ITool
    {
        /// <summary>
        /// Archives the given source files.
        /// </summary>
        /// <param name="build">The <see cref="Build"/> definition for the compilation.</param>
        /// <returns>The error code. 0 if succeeded.</returns>
        public int Archive(Build build) => Execute(build);
    }
}
