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
    public class MsvcAssembler : MsvcToolBase, IAssembler
    {
        public override TargetTriplet TargetTriplet { get; set; }
        public override IList<string> SourceFiles { get; } = new List<string>();
        public OutputKind OutputKind { get; set; } = OutputKind.Object;

        public MsvcAssembler(string vcVarsAllPath) 
            : base(vcVarsAllPath)
        {
        }

        public override int Execute(string outputPath)
        {
            // Escape file names
            var files = string.Join(' ', SourceFiles.Select(f => $"\"{f}\""));
            // The actual file name to invoke
            var ml = TargetTriplet.CpuFamily == CpuFamily.X86 ? "ML" : "ML64";
            // Construct the command
            var command = $"{ml} /nologo /Fo \"{outputPath}\" {GetOutputKindFlag()} {files}";
            // Run it
            return InvokeWithEnvironment(command);
        }

        private string GetOutputKindFlag() => OutputKind switch
        {
            OutputKind.Executable => string.Empty,
            OutputKind.Object => "/c",
            _ => throw new NotSupportedException($"The output kind {OutputKind} is not supported by ML (MASM)!"),
        };
    }
}
