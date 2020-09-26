using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Debug.Assert(build.CheckedAssembly != null);
            // Escape file names
            var objectFile = (string)build.Extra["objectFile"];
            // Escape extra binaries
            var extraBinaries = string.Join(' ', build.CheckedAssembly.BinaryReferences.Select(r => $"\"{r}\""));
            // Construct the command
            var entry = build.OutputKind == OutputKind.Executable 
                ? $"/ENTRY:\"{build.CheckedAssembly.EntryPoint.Name}\"" 
                : string.Empty;
            var publicSymbols = build.CheckedAssembly.Symbols.Where(sym => sym.Visibility == Visibility.Public);
            var exports = string.Join(' ', publicSymbols.Select(sym => $"/EXPORT:\"{sym.Name}\""));
            var outputKindFlag = GetOutputKindFlag(build.OutputKind);
            var targetMachineId = GetTargetMachineId(build.TargetTriplet);
            var extraFiles = GetExtraFiles(build.OutputKind);
            var command = $"LINK /NOLOGO {outputKindFlag} {exports} /MACHINE:{targetMachineId} {entry} /OUT:\"{build.OutputPath}\" {objectFile} {extraFiles} {extraBinaries}";
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
