using System;
using System.Collections.Generic;
using System.Linq;

namespace Yoakke.Lir.Backend.Toolchain
{
    /// <summary>
    /// Abstraction over a chain of tools required to compile from the backend representation to a binary.
    /// </summary>
    public interface IToolchain
    {
        /// <summary>
        /// The code generator backend of this <see cref="IToolchain"/>.
        /// </summary>
        public IBackend Backend { get; }
        /// <summary>
        /// The <see cref="ITool"/>s of this <see cref="IToolchain"/>.
        /// </summary>
        public IEnumerable<ITool> Tools { get; }
        /// <summary>
        /// The <see cref="TargetTriplet"/> the toolchain targets.
        /// </summary>
        public TargetTriplet TargetTriplet 
        { 
            get => Tools.First().TargetTriplet; 
            set 
            {
                // TODO: Better error
                if (!IsSupported(value)) throw new NotSupportedException();
                foreach (var tool in Tools) tool.TargetTriplet = value; 
            }
        }
        /// <summary>
        /// The <see cref="Assembly"/>s that need to be compiled.
        /// </summary>
        public IList<Assembly> Assemblies { get; }
        /// <summary>
        /// The <see cref="OutputKind"/> the needed to produce.
        /// </summary>
        public OutputKind OutputKind { get; set; }
        /// <summary>
        /// The directory the intermediate files should be stored in.
        /// </summary>
        public string BuildDirectory { get; set; }

        /// <summary>
        /// The first <see cref="IAssembler"/> in this toolchain.
        /// </summary>
        public IAssembler Assembler => 
            // NOTE: Cast returned IAssembler? for some reason
            Tools.Where(t => t is IAssembler).Select(t => (IAssembler)t).First();
        /// <summary>
        /// The first <see cref="ILinker"/> in this toolchain.
        /// </summary>
        public ILinker Linker =>
            // NOTE: Cast returned ILinker? for some reason
            Tools.Where(t => t is ILinker).Select(t => (ILinker)t).First();
        /// <summary>
        /// The first <see cref="IArchiver"/> in this toolchain.
        /// </summary>
        public IArchiver Archiver =>
            // NOTE: Cast returned IArchiver? for some reason
            Tools.Where(t => t is IArchiver).Select(t => (IArchiver)t).First();

        /// <summary>
        /// Checks, if the given <see cref="TargetTriplet"/> is supported by this toolchain.
        /// </summary>
        /// <param name="targetTriplet">The <see cref="TargetTriplet"/> to check support for.</param>
        /// <returns>True, if the <see cref="TargetTriplet"/> is supported.</returns>
        public bool IsSupported(TargetTriplet targetTriplet);

        /// <summary>
        /// Compiles the given <see cref="Files"/>.
        /// </summary>
        /// <param name="outputPath">The resulting binary's path.</param>
        /// <returns>The error code. 0 if succeeded.</returns>
        public int Compile(string outputPath);
    }
}
