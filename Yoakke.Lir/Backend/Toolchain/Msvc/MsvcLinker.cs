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
        public TargetTriplet TargetTriplet { get; set; }
        public IList<string> SourceFiles { get; } = new List<string>();
        public OutputKind OutputKind { get; set; } = OutputKind.Executable;

        public MsvcLinker(string vcVarsAllPath) 
            : base(vcVarsAllPath)
        {
        }

        public int Execute(string outputPath)
        {
            // Escape file names
            var files = string.Join(' ', SourceFiles.Select(f => $"\"{f}\""));
            // Construct the command
            var command = $"LINK /NOLOGO {GetOutputKindFlag()} /OUT:\"{outputPath}\" {files}";
            var proc = InvokeWithEnvironment(command, TargetTriplet);
            // Execute
            proc.Start();
            var err = proc.StandardError.ReadToEnd();
            proc.WaitForExit();
            Console.Error.Write(err);
            return proc.ExitCode;
        }

        private string GetOutputKindFlag() => OutputKind switch
        {
            OutputKind.Executable => string.Empty,
            OutputKind.DynamicLibrary => "/DLL",
            _ => throw new NotImplementedException(),
        };
    }
}
