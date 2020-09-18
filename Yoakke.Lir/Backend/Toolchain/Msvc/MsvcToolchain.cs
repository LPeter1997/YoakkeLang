using System.Collections.Generic;
using System.IO;
using System.Linq;
using Yoakke.Lir.Backend.Backends;

namespace Yoakke.Lir.Backend.Toolchain.Msvc
{
    /// <summary>
    /// The MSVC toolchain.
    /// </summary>
    public class MsvcToolchain : IToolchain
    {
        public IBackend Backend { get; } = new MasmX86Backend();
        public IEnumerable<ITool> Tools
        {
            get
            {
                yield return Assembler;
                yield return Linker;
                yield return Archiver;
            }
        }

        public readonly IAssembler Assembler;
        public readonly ILinker Linker;
        public readonly IArchiver Archiver;

        public string Version { get; }

        public MsvcToolchain(string version, string vcVarsAllPath)
        {
            Version = version;
            Assembler = new MsvcAssembler(version, vcVarsAllPath);
            Linker = new MsvcLinker(version, vcVarsAllPath);
            Archiver = new MsvcArchiver(version, vcVarsAllPath);
        }

        public int Compile(Build build)
        {
            if (build.Assemblies.Count == 0)
            {
                // TODO: Warn user?
                return -1;
            }

            build.Metrics.StartTime("Overall");

            Directory.CreateDirectory(build.IntermediatesDirectory);

            // We translate the IR assemblies to the given backend
            build.Metrics.StartTime("Translation to x86 assembly");
            var assemblyFiles = new List<string>();
            build.Extra["assemblyFiles"] = assemblyFiles;
            foreach (var asm in build.Assemblies)
            {
                var outFile = Path.Combine(build.IntermediatesDirectory, $"{asm.Name}.asm");
                Backend.Compile(asm, outFile);
                assemblyFiles.Add(outFile);
            }
            build.Metrics.EndTime();

            // Then we assemble each file
            build.Metrics.StartTime("Assembly");
            var errCode = Assembler.Assemble(build);
            build.Metrics.EndTime();
            if (errCode != 0)
            {
                build.Metrics.EndTime();
                return errCode;
            }

            // We append external binaries here
            build.Extra["externalBinaries"] = build.Assemblies
                .SelectMany(asm => asm.BinaryReferences)
                .ToList();

            // Invoke the linker (LINK) or the archiver (LIB)
            if (build.OutputKind == OutputKind.Executable || build.OutputKind == OutputKind.DynamicLibrary)
            {
                build.Metrics.StartTime("Linking");
                // We need to explicitly tell the linker to export everything public
                var publicSymbols = build.Assemblies
                    .SelectMany(asm => asm.Symbols)
                    .Where(sym => sym.Visibility == Visibility.Public);
                foreach (var sym in publicSymbols) build.Exports.Add(sym);
                // Invoke the linker
                errCode = Linker.Link(build);
                build.Metrics.EndTime();
            }
            else if (build.OutputKind == OutputKind.StaticLibrary)
            {
                build.Metrics.StartTime("Archiving");
                // We use the archiver
                errCode = Archiver.Archive(build);
                build.Metrics.EndTime();
            }
            else
            {
                // Object files
                errCode = 0;
            }
            build.Metrics.EndTime();
            return errCode;
        }

        public override string ToString() => $"msvc-{Version}";
    }
}
