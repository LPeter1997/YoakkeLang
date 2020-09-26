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
        public void Link(Build build) => Execute(build);
    }
}
