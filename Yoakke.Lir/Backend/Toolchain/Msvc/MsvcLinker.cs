using System;
using System.Collections.Generic;
using System.Linq;

namespace Yoakke.Lir.Backend.Toolchain.Msvc
{
    /// <summary>
    /// The MSVC LINK linker.
    /// </summary>
    public class MsvcLinker : MsvcToolBase, ILinker
    {
        public MsvcLinker(string version, string vcVarsAllPath) 
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
            // TODO
            string entryPoint = "";
            var entry = build.OutputKind == OutputKind.Executable ? $"/ENTRY:\"{entryPoint}\"" : string.Empty;
            // TODO
            var exports = "";//string.Join(' ', build.Exports.Select(e => $"/EXPORT:\"{e.Name}\""));
            var outputKindFlag = GetOutputKindFlag(build.OutputKind);
            var targetMachineId = GetTargetMachineId(build.TargetTriplet);
            var extraFiles = GetExtraFiles(build.OutputKind);
            var command = $"LINK /NOLOGO {outputKindFlag} {exports} /MACHINE:{targetMachineId} {entry} /OUT:\"{build.OutputPath}\" {objectFiles} {extraFiles} {extraBinaries}";
            // Run it
            InvokeWithEnvironment(command, build);
        }

        private static string GetExtraFiles(OutputKind outputKind) => outputKind switch
        {
            OutputKind.Executable => string.Empty,
            OutputKind.DynamicLibrary => "msvcrt.lib",
            _ => throw new NotSupportedException($"The output kind {outputKind} is not supported by LINK!"),
        };

        private static string GetOutputKindFlag(OutputKind outputKind) => outputKind switch
        {
            OutputKind.Executable => string.Empty,
            OutputKind.DynamicLibrary => "/DLL",
            _ => throw new NotSupportedException($"The output kind {outputKind} is not supported by LINK!"),
        };

        public override string ToString() => $"LINK-{Version}";
    }
}
