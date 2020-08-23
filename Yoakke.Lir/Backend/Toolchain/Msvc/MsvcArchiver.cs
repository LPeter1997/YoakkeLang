using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Backend.Toolchain.Msvc
{
    /// <summary>
    /// The MSVC LIB tool.
    /// </summary>
    public class MsvcArchiver : MsvcToolBase, IArchiver
    {
        public OutputKind OutputKind { get; set; } = OutputKind.StaticLibrary;
        public TargetTriplet TargetTriplet { get; set; }
        public IList<string> SourceFiles { get; } = new List<string>();

        public MsvcArchiver(string vcVarsAllPath) 
            : base(vcVarsAllPath)
        {
        }

        public int Execute(string outputPath)
        {
            throw new NotImplementedException();
        }
    }
}
