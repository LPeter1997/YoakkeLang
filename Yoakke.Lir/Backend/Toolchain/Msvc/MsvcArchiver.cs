using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Yoakke.Lir.Backend.Toolchain.Msvc
{
    /// <summary>
    /// The MSVC LIB tool.
    /// </summary>
    public class MsvcArchiver : MsvcToolBase, IArchiver
    {
        public MsvcArchiver(string version, string vcVarsAllPath) 
            : base(version, vcVarsAllPath)
        {
        }

        public override void Execute(Build build)
        {
            Debug.Assert(build.CheckedAssembly != null);
            var objectFile = (string)build.Extra["objectFile"];
            // Escape extra binaries
            var extraBinaries = string.Join(' ', build.CheckedAssembly.BinaryReferences.Select(r => $"\"{r}\""));
            // Construct the command
            var targetMachineId = GetTargetMachineId(build.TargetTriplet);
            var command = $"LIB /NOLOGO /MACHINE:{targetMachineId} /OUT:\"{build.OutputPath}\" \"{objectFile}\" {extraBinaries}";
            // Run it
            InvokeWithEnvironment(command, build);
        }

        public override string ToString() => $"LIB-{Version}";
    }
}
