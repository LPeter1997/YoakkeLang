using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Backend.Toolchain.Msvc
{
    /// <summary>
    /// The MSVC LINK linker.
    /// </summary>
    public class MsvcLinker : MsvcToolBase, ILinker
    {
        public override TargetTriplet TargetTriplet { get; set; }
        public override IList<string> SourceFiles { get; } = new List<string>();
        public OutputKind OutputKind { get; set; } = OutputKind.Executable;
        public string EntryPoint { get; set; } = "main";

        public MsvcLinker(string vcVarsAllPath) 
            : base(vcVarsAllPath)
        {
        }

        public override int Execute(string outputPath)
        {
            // Escape file names
            var files = string.Join(' ', SourceFiles.Select(f => $"\"{f}\""));
            // Construct the command
            var entry = OutputKind == OutputKind.Executable ? $"/ENTRY:\"{EntryPoint}\"" : string.Empty;
            var command = $"LINK /NOLOGO {GetOutputKindFlag()} /MACHINE:{GetTargetMachineId()} {entry} /OUT:\"{outputPath}\" {files}";
            // Run it
            return InvokeWithEnvironment(command);
        }

        private string GetOutputKindFlag() => OutputKind switch
        {
            OutputKind.Executable => string.Empty,
            OutputKind.DynamicLibrary => "/DLL",
            _ => throw new NotSupportedException($"The output kind {OutputKind} is not supported by LINK!"),
        };
    }
}
