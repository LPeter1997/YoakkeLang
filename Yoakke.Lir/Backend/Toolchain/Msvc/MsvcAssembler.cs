using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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

        public override void Execute(Build build)
        {
            // File names
            var assemblyFile = (string)build.Extra["assemblyFile"];
            // The output path
            var outputPath = Path.ChangeExtension(assemblyFile, ".o");
            build.Extra["objectFile"] = outputPath;
            // The actual file name to invoke
            var ml = build.TargetTriplet.CpuFamily == CpuFamily.X86 ? "ML" : "ML64";
            // Create the command
            var command = $"{ml} /nologo /Fo \"{outputPath}\" /c \"{assemblyFile}\"";
            // Call it
            InvokeWithEnvironment(command, build);
        }

        public override string ToString() => $"ML-{Version}";
    }
}
