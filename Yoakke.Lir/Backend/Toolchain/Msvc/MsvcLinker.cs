using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Yoakke.Lir.Backend.Toolchain.Msvc
{
    /// <summary>
    /// The MSVC LINK linker.
    /// </summary>
    public class MsvcLinker : MsvcToolBase, ILinker
    {
        public MsvcLinker(string version, string msvcSdk, string windowsSdk, string windowsSdkVer)
            : base(version, msvcSdk, windowsSdk, windowsSdkVer)
        {
        }

        public override void Execute(Build build)
        {
            Debug.Assert(build.CheckedAssembly != null);
            // Construct the command
            /*var entry = build.OutputKind == OutputKind.Executable 
                ? $"/ENTRY:\"{build.CheckedAssembly.EntryPoint.Name}\"" 
                : string.Empty;*/
            var publicSymbols = (IReadOnlyList<string>)build.Extra["publicSymbolNames"];
            var exports = string.Join(' ', publicSymbols.Select(sym => $"/EXPORT:\"{sym}\""));
            var outputKindFlag = GetOutputKindFlag(build.OutputKind);
            var targetMachineId = GetTargetMachineId(build.TargetTriplet);

            // Binaries to pass
            var extraFiles = GetExtraFiles(build.OutputKind);
            var objectFile = $"\"{(string)build.Extra["objectFile"]}\"";
            var extraBinaries = string.Join(' ', 
                build.CheckedAssembly.BinaryReferences.Concat(build.ExternalBinaries).Select(r => $"\"{r}\""));
            var allFiles = $"{objectFile} {extraFiles} {extraBinaries}";

            // Library paths
            var msvcLibPath = Path.Combine(MsvcSdk, "lib", targetMachineId);
            var winumLibPath = Path.Combine(WindowsSdk, "Lib", WindowsSdkVersion, "um", targetMachineId);
            var winucrtLibPath = Path.Combine(WindowsSdk, "Lib", WindowsSdkVersion, "ucrt", targetMachineId);
            var allLibPaths = $"/LIBPATH:\"{msvcLibPath}\" /LIBPATH:\"{winumLibPath}\" /LIBPATH:\"{winucrtLibPath}\"";

            var arguments = $"/NOLOGO /SUBSYSTEM:CONSOLE {allLibPaths} {outputKindFlag} {exports} /MACHINE:{targetMachineId} /OUT:\"{build.OutputPath}\" {allFiles}";
            // Run it
            InvokeWithEnvironment("LINK.exe", arguments, build);
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
