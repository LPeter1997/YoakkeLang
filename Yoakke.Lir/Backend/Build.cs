using System;
using System.Collections.Generic;
using System.IO;
using Yoakke.Lir.Passes;
using Yoakke.Lir.Status;

namespace Yoakke.Lir.Backend
{
    /// <summary>
    /// All information required for a <see cref="IToolchain"/> to perform a build.
    /// </summary>
    public class Build
    {
        // TODO: Doc
        public delegate void BuildErrorEventHandler(Build build, IBuildError buildError);
        // TODO: Doc
        public delegate void BuildWarningEventHandler(Build build, IBuildWarning buildWarning);

        // TODO: Doc
        public event BuildErrorEventHandler? BuildError;
        // TODO: Doc
        public event BuildWarningEventHandler? BuildWarning;

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

        private UncheckedAssembly? assembly;
        /// <summary>
        /// The <see cref="UncheckedAssembly"/> that need to be compiled.
        /// </summary>
        public UncheckedAssembly? Assembly 
        { 
            get => assembly; 
            set
            {
                // Propagate validation errors
                if (assembly != null) assembly.ValidationError -= OnValidationError;
                assembly = value;
                if (assembly != null) assembly.ValidationError += OnValidationError;
            }
        }
        /// <summary>
        /// The already checked <see cref="Assembly"/>.
        /// </summary>
        public Assembly? CheckedAssembly { get; set; }
        /// <summary>
        /// The <see cref="ICodePass"/> to perform on the checked assembly.
        /// </summary>
        public ICodePass? CodePass { get; set; }
        /// <summary>
        /// The external binaries to link.
        /// </summary>
        public readonly ISet<string> ExternalBinaries = new HashSet<string>();
        /// <summary>
        /// Any extra information the build needs to carry.
        /// </summary>
        public readonly IDictionary<string, object> Extra = new Dictionary<string, object>();

        /// <summary>
        /// <see cref="Metrics"/> about the build.
        /// </summary>
        public readonly Metrics Metrics = new Metrics();

        // TODO: Doc
        public bool HasErrors { get; private set; }

        public Build()
        {
            BuildError += (s, e) => HasErrors = true;
        }

        public void Report(IBuildWarning warning) => BuildWarning?.Invoke(this, warning);
        public void Report(IBuildError error) => BuildError?.Invoke(this, error);

        private void OnValidationError(ValidationContext context, ValidationError validationError)
        {
            Report(validationError);
        }
    }
}
