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
        public void Archive(Build build) => Execute(build);
    }
}
