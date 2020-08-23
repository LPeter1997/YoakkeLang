using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Backend.Toolchain.Msvc
{
    /// <summary>
    /// The MSVC MASM assembler.
    /// </summary>
    public class MsvcAssembler : IAssembler
    {
        public TargetTriplet TargetTriplet { get; set; }

        public int Assemble(string sourcePath, string outputPath)
        {
            throw new NotImplementedException();
        }
    }
}
