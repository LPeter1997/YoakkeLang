using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Backend.Backends
{
    /// <summary>
    /// Backend for NASM on X86.
    /// </summary>
    public class NasmX86Backend : IBackend
    {
        public Toolchain Toolchain { get; set; }

        /// <summary>
        /// Initializes a new <see cref="NasmX86Backend"/>.
        /// </summary>
        /// <param name="toolchain">The <see cref="Toolchain"/> to be used by the NASM backend.</param>
        public NasmX86Backend(Toolchain toolchain)
        {
            Toolchain = toolchain;
        }

        public bool IsSupported(TargetTriplet t) =>
            t.CpuFamily == CpuFamily.X86 && t.OperatingSystem == OperatingSystem.Windows;

        public string Compile(TargetTriplet targetTriplet, Assembly assembly)
        {
            if (!IsSupported(targetTriplet))
            {
                throw new NotSupportedException("The given target triplet is not supported by this backend!");
            }
            throw new NotImplementedException();
        }
    }
}
