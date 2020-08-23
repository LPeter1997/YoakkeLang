using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Backend.Toolchain.Msvc
{
    /// <summary>
    /// The MSVC toolchain.
    /// </summary>
    public class MsvcToolchain : IToolchain
    {
        public IEnumerable<ITool> Tools
        {
            get
            {
                yield return Assembler;
                yield return Linker;
                yield return Archiver;
            }
        }
        public TargetTriplet TargetTriplet 
        { 
            get => Assembler.TargetTriplet; 
            set { foreach (var tool in Tools) tool.TargetTriplet = value; }
        }
        public IList<string> SourceFiles { get; } = new List<string>();
        public OutputKind OutputKind { get => Linker.OutputKind; set => Linker.OutputKind = value; }

        public readonly IAssembler Assembler;
        public readonly ILinker Linker;
        public readonly IArchiver Archiver;

        public MsvcToolchain(string vcVarsAllPath)
        {
            Assembler = new MsvcAssembler(vcVarsAllPath);
            Linker = new MsvcLinker(vcVarsAllPath);
            Archiver = new MsvcArchiver(vcVarsAllPath);
        }

        public int Compile(string outputPath)
        {
            throw new NotImplementedException();
        }
    }
}
