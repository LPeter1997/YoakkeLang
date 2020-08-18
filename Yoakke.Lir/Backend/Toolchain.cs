using System.Collections.Generic;

namespace Yoakke.Lir.Backend
{
    /// <summary>
    /// A structure describing the tools to compile the backends to binary.
    /// </summary>
    public class Toolchain
    {
        /// <summary>
        /// The compiler, if any.
        /// </summary>
        public string? Compiler { get; set; }
        /// <summary>
        /// The assembler, if any.
        /// </summary>
        public string? Assembler { get; set; }
        /// <summary>
        /// The linker, if any.
        /// </summary>
        public string? Linker { get; set; }
        /// <summary>
        /// Other tools that the <see cref="Toolchain"/> might need.
        /// </summary>
        public readonly IDictionary<string, object> Other = new Dictionary<string, object>();
    }
}
