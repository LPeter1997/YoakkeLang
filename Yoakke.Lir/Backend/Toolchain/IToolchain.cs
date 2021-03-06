﻿using System.Collections.Generic;
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
        /// The first <see cref="IAssembler"/> in this toolchain.
        /// </summary>
        public IAssembler Assembler => 
            Tools.OfType<IAssembler>().First();
        /// <summary>
        /// The first <see cref="ILinker"/> in this toolchain.
        /// </summary>
        public ILinker Linker =>
            Tools.OfType<ILinker>().First();
        /// <summary>
        /// The first <see cref="IArchiver"/> in this toolchain.
        /// </summary>
        public IArchiver Archiver =>
            Tools.OfType<IArchiver>().First();

        /// <summary>
        /// A string that represents the version of this toolchain.
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// Checks, if the given <see cref="TargetTriplet"/> is supported by this toolchain.
        /// </summary>
        /// <param name="targetTriplet">The <see cref="TargetTriplet"/> to check support for.</param>
        /// <returns>True, if the <see cref="TargetTriplet"/> is supported.</returns>
        public bool IsSupported(TargetTriplet targetTriplet) =>
            Tools.All(t => t.IsSupported(targetTriplet));

        /// <summary>
        /// Compiles the given <see cref="Build"/>.
        /// </summary>
        /// <param name="build">The <see cref="Build"/> definition for the compilation.</param>
        public void Compile(Build build);
    }
}
