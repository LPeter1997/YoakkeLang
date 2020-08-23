using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Backend.Toolchain
{
    /// <summary>
    /// Abstraction over a chain of tools required to compile from the backend representation to a binary.
    /// </summary>
    public interface IToolchain
    {
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
            set { foreach (var tool in Tools) tool.TargetTriplet = value; }
        }
        /// <summary>
        /// The files that need to be compiled.
        /// </summary>
        public IList<string> SourceFiles { get; }
        /// <summary>
        /// The <see cref="OutputKind"/> the needed to produce.
        /// </summary>
        public OutputKind OutputKind { get; set; }

        /// <summary>
        /// The first <see cref="IAssembler"/> in this toolchain.
        /// </summary>
        public IAssembler Assembler => Tools.Where(t => t is IAssembler).Cast<IAssembler>().First();
        /// <summary>
        /// The first <see cref="ILinker"/> in this toolchain.
        /// </summary>
        public ILinker Linker => Tools.Where(t => t is ILinker).Cast<ILinker>().First();
        /// <summary>
        /// The first <see cref="IArchiver"/> in this toolchain.
        /// </summary>
        public IArchiver Archiver => Tools.Where(t => t is IArchiver).Cast<IArchiver>().First();

        /// <summary>
        /// Compiles the given <see cref="Files"/>.
        /// </summary>
        /// <param name="outputPath">The resulting binary's path.</param>
        /// <returns>The error code. 0 if succeeded.</returns>
        public int Compile(string outputPath);
    }
}
