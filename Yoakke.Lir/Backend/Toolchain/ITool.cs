using System.Collections.Generic;

namespace Yoakke.Lir.Backend.Toolchain
{
    /// <summary>
    /// An interface for tools in the <see cref="IToolchain"/>.
    /// </summary>
    public interface ITool
    {
        /// <summary>
        /// The <see cref="TargetTriplet"/> for the tool.
        /// </summary>
        public TargetTriplet TargetTriplet { get; set; }
        /// <summary>
        /// The files that the tool needs.
        /// </summary>
        public IList<string> SourceFiles { get; }

        /// <summary>
        /// Executes this tool.
        /// </summary>
        /// <param name="outputPath">The output file's path.</param>
        /// <returns>The exit code. 0 on success.</returns>
        public int Execute(string outputPath);
    }
}
