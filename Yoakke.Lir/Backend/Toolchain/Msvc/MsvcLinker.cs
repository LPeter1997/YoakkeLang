using System;
using System.Collections.Generic;
using System.Linq;

namespace Yoakke.Lir.Backend.Toolchain.Msvc
{
    // TODO: Check if intermediates really go into the build directory
    /// <summary>
    /// The MSVC LINK linker.
    /// </summary>
    public class MsvcLinker : MsvcToolBase, ILinker
    {
        public override TargetTriplet TargetTriplet { get; set; }
        public override IList<string> SourceFiles { get; } = new List<string>();
        public OutputKind OutputKind { get; set; } = OutputKind.Executable;
        public string EntryPoint { get; set; } = "main";
        public IList<ISymbol> Exports { get; } = new List<ISymbol>();

        public MsvcLinker(string vcVarsAllPath) 
            : base(vcVarsAllPath)
        {
        }

        public override int Execute(string outputPath)
        {
            // Escape file names
            var files = string.Join(' ', SourceFiles.Select(f => $"\"{f}\""));
            var extraFiles = GetExtraFiles();
            // Construct the command
            var entry = OutputKind == OutputKind.Executable ? $"/ENTRY:\"{EntryPoint}\"" : string.Empty;
            var exports = string.Join(' ', Exports.Select(e => $"/EXPORT:\"{e.Name}\""));
            var command = $"LINK /NOLOGO {GetOutputKindFlag()} {exports} /MACHINE:{GetTargetMachineId()} {entry} /OUT:\"{outputPath}\" {files} {extraFiles}";
            // Run it
            return InvokeWithEnvironment(command);
        }

        private string GetExtraFiles() => OutputKind switch
        {
            OutputKind.Executable => string.Empty,
            OutputKind.DynamicLibrary => "msvcrt.lib",
            _ => throw new NotSupportedException($"The output kind {OutputKind} is not supported by LINK!"),
        };

        private string GetOutputKindFlag() => OutputKind switch
        {
            OutputKind.Executable => string.Empty,
            OutputKind.DynamicLibrary => "/DLL",
            _ => throw new NotSupportedException($"The output kind {OutputKind} is not supported by LINK!"),
        };
    }
}
