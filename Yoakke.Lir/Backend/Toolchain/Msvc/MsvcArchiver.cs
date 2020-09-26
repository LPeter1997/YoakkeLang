using System.Collections.Generic;
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
            // Escape file names
            var unescapedObjectFiles = (IList<string>)build.Extra["objectFiles"];
            var objectFiles = string.Join(' ', unescapedObjectFiles.Select(f => $"\"{f}\""));
            // Escape extra binaries
            var unescapedExtraBinaries = (IList<string>)build.Extra["externalBinaries"];
            var extraBinaries = string.Join(' ', unescapedExtraBinaries.Select(f => $"\"{f}\""));
            // Construct the command
            var targetMachineId = GetTargetMachineId(build.TargetTriplet);
            var command = $"LIB /NOLOGO /MACHINE:{targetMachineId} /OUT:\"{build.OutputPath}\" {objectFiles} {extraBinaries}";
            // Run it
            InvokeWithEnvironment(command, build);
        }

        public override string ToString() => $"LIB-{Version}";
    }
}
