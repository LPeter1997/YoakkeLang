using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Backend.Toolchain
{
    /// <summary>
    /// A structure describing the tools needed to compile the backends to binaries.
    /// </summary>
    public class Toolchain
    {
        /// <summary>
        /// The assembler that creates object files from the backend outputs.
        /// </summary>
        public IAssembler? Assembler { get; set; }
        /// <summary>
        /// The linker that creates the final binary from the assembled object files.
        /// </summary>
        public ILinker? Linker { get; set; }
    }
}
