using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Yoakke.Lir.Backend.Toolchain.Msvc
{
    /// <summary>
    /// The MSVC MASM assembler.
    /// </summary>
    public class MsvcAssembler : MsvcToolBase, IAssembler
    {
        public MsvcAssembler(string version, string vcVarsAllPath) 
            : base(version, vcVarsAllPath)
        {
        }

        public override int Execute(Build build)
        {
            // File names
            var assemblyFiles = (IList<string>)build.Extra["assemblyFiles"];
            // The actual file name to invoke
            var ml = build.TargetTriplet.CpuFamily == CpuFamily.X86 ? "ML" : "ML64";
            // Command constructor function for each output file
            Func<string, string, string> makeCommand = (outputPath, assemblyFile) =>
                $"{ml} /nologo /Fo \"{outputPath}\" /c \"{assemblyFile}\"";
            // Compile each assembly file
            var objectFiles = new List<string>();
            build.Extra["objectFiles"] = objectFiles;
            foreach (var assemblyFile in assemblyFiles)
            {
                string? outputPath = Path.ChangeExtension(assemblyFile, ".o");
                Debug.Assert(outputPath != null);
                var command = makeCommand(outputPath, assemblyFile);
                // Execute
                var errCode = InvokeWithEnvironment(command, build.TargetTriplet);
                if (errCode != 0) return errCode;
                // Append it to object files
                objectFiles.Add(outputPath);
            }
            return 0;
        }

        public override string ToString() => $"ML-{Version}";
    }
}
