using System.Collections.Generic;

namespace Yoakke.Lir.Backend
{
    /// <summary>
    /// All information required for a <see cref="IToolchain"/> to perform a build.
    /// </summary>
    public class Build
    {
        /// <summary>
        /// The <see cref="TargetTriplet"/> the build targets.
        /// </summary>
        public TargetTriplet TargetTriplet { get; set; }
        /// <summary>
        /// The <see cref="OutputKind"/> the build needs to produce.
        /// </summary>
        public OutputKind OutputKind { get; set; } = OutputKind.Executable;
        /// <summary>
        /// The directory the intermediate files should be stored in.
        /// </summary>
        public string IntermediatesDirectory { get; set; } = ".";
        /// <summary>
        /// The path of the output file.
        /// </summary>
        public string OutputPath { get; set; } = "a.out";
        /// <summary>
        /// The <see cref="Assembly"/>s that need to be compiled.
        /// </summary>
        public readonly IList<Assembly> Assemblies = new List<Assembly>();
        /// <summary>
        /// The <see cref="ISymbol"/>s that needs to be exported.
        /// </summary>
        public readonly ISet<ISymbol> Exports = new HashSet<ISymbol>();
        /// <summary>
        /// The entry point's name.
        /// </summary>
        public string EntryPoint { get; set; } = "main";
        /// <summary>
        /// Any extra information the build needs to carry.
        /// </summary>
        public readonly IDictionary<string, object> Extra = new Dictionary<string, object>();
        /// <summary>
        /// <see cref="Metrics"/> about the build.
        /// </summary>
        public readonly Metrics Metrics = new Metrics();
    }
}
