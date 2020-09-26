using System.Collections.Generic;
using Yoakke.Lir.Status;

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
        /// The <see cref="UncheckedAssembly"/> that need to be compiled.
        /// </summary>
        public UncheckedAssembly? Assembly { get; set; }
        /// <summary>
        /// Any extra information the build needs to carry.
        /// </summary>
        public readonly IDictionary<string, object> Extra = new Dictionary<string, object>();

        /// <summary>
        /// <see cref="Metrics"/> about the build.
        /// </summary>
        public readonly Metrics Metrics = new Metrics();
        /// <summary>
        /// The <see cref="BuildStatus"/> for this build.
        /// </summary>
        public readonly BuildStatus Status = new BuildStatus();

        public bool HasErrors => Status.Errors.Count != 0;

        public void Report(IBuildWarning warning) => Status.Warnings.Add(warning);
        public void Report(IBuildError error) => Status.Errors.Add(error);
    }
}
