using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Backend.Toolchain
{
    /// <summary>
    /// Locates a given supported <see cref="Toolchain"/>.
    /// </summary>
    public interface IToolchainLocator
    {
        /// <summary>
        /// Tries to locate the <see cref="Toolchain"/>.
        /// </summary>
        /// <param name="toolchain">The located <see cref="Toolchain"/>.</param>
        /// <returns>True, if the <see cref="Toolchain"/> was located successfully.</returns>
        public bool TryLocate(out Toolchain? toolchain);

        /// <summary>
        /// Tries to locate the assembler.
        /// Most likely you'd want <see cref="TryLocate(out Toolchain?)"/> instead, only use this for
        /// building some custom toolchain.
        /// </summary>
        /// <param name="assembler">The found assembler.</param>
        /// <returns>True, if the assembler could be located.</returns>
        public bool TryLocateAssembler(out IAssembler? assembler);

        /// <summary>
        /// Tries to locate the linker.
        /// Most likely you'd want <see cref="TryLocate(out Toolchain?)"/> instead, only use this for
        /// building some custom toolchain.
        /// </summary>
        /// <param name="linker">The found linker.</param>
        /// <returns>True, if the linker could be located.</returns>
        public bool TryLocateLinker(out ILinker? linker);
    }
}
