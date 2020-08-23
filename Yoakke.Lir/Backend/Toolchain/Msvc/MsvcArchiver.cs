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
        public override TargetTriplet TargetTriplet { get; set; }
        public override IList<string> SourceFiles { get; } = new List<string>();
        public OutputKind OutputKind { get; set; } = OutputKind.StaticLibrary;

        public MsvcArchiver(string vcVarsAllPath) 
            : base(vcVarsAllPath)
        {
        }

        public override int Execute(string outputPath)
        {
            // Escape file names
            var files = string.Join(' ', SourceFiles.Select(f => $"\"{f}\""));
            // Construct the command
            var command = $"LIB /NOLOGO /MACHINE:{GetTargetMachineId()} /OUT:\"{outputPath}\" {files}";
            // Run it
            return InvokeWithEnvironment(command);
        }
    }
}
