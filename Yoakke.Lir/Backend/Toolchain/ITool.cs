namespace Yoakke.Lir.Backend.Toolchain
{
    /// <summary>
    /// An interface for tools in the <see cref="IToolchain"/>.
    /// </summary>
    public interface ITool
    {
        /// <summary>
        /// A string that represents the version of this tool.
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// Checks, if the given <see cref="TargetTriplet"/> is supported by this tool.
        /// </summary>
        /// <param name="targetTriplet">The <see cref="TargetTriplet"/> to check support for.</param>
        /// <returns>True, if the <see cref="TargetTriplet"/> is supported.</returns>
        public bool IsSupported(TargetTriplet targetTriplet);

        /// <summary>
        /// Executes this tool.
        /// </summary>
        /// <param name="build">The <see cref="Build"/> definition for the compilation.</param>
        /// <returns>The exit code. 0 on success.</returns>
        public int Execute(Build build);
    }
}
