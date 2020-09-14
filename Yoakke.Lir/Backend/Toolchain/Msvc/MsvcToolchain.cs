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
        public IList<Assembly> Assemblies { get; } = new List<Assembly>();
        public OutputKind OutputKind { get => Linker.OutputKind; set => Linker.OutputKind = value; }
        public string BuildDirectory { get; set; } = ".";

        public readonly IAssembler Assembler;
        public readonly ILinker Linker;
        public readonly IArchiver Archiver;

        private string version;

        public MsvcToolchain(string version, string vcVarsAllPath)
        {
            this.version = version;
            Assembler = new MsvcAssembler(vcVarsAllPath);
            Linker = new MsvcLinker(vcVarsAllPath);
            Archiver = new MsvcArchiver(vcVarsAllPath);
        }

        public bool IsSupported(TargetTriplet t) =>
            t.CpuFamily == CpuFamily.X86 && t.OperatingSystem == OperatingSystem.Windows;

        public int Compile(string outputPath)
        {
            Directory.CreateDirectory(BuildDirectory);
            if (Assemblies.Count == 0) return 0;
            // We translate the IR assemblies to the given backend
            var backendFiles = new List<string>();
            foreach (var asm in Assemblies)
            {
                var outFile = Path.Combine(BuildDirectory, $"{asm.Name}.asm");
                Backend.Compile(asm, outFile);
                backendFiles.Add(outFile);
            }
            // Then we assemble each file
            var assembledFiles = new List<string>();
            foreach (var file in backendFiles)
            {
                var outFile = Path.ChangeExtension(file, ".o");
                var err = Assembler.Assemble(file, outFile);
                if (err != 0) return err;
                assembledFiles.Add(outFile);
            }
            // We append external binaries here
            foreach (var externalBinary in Assemblies.SelectMany(asm => asm.BinaryReferences))
            {
                assembledFiles.Add(externalBinary);
            }
            // Invoke the linker (LINK) or the archiver (LIB)
            if (OutputKind == OutputKind.Executable || OutputKind == OutputKind.DynamicLibrary)
            {
                // We need to explicitly tell the linker to export everything public
                foreach (var sym in Assemblies.SelectMany(asm => asm.Symbols).Where(sym => sym.Visibility == Visibility.Public))
                {
                    Linker.Exports.Add(sym);
                }
                // We use the linker
                foreach (var f in assembledFiles) Linker.SourceFiles.Add(f);
                return Linker.Link(outputPath);
            }
            else if (OutputKind == OutputKind.StaticLibrary)
            {
                // We use the archiver
                foreach (var f in assembledFiles) Archiver.SourceFiles.Add(f);
                return Archiver.Archive(outputPath);
            }
            return 0;
        }

        public override string ToString() => $"msvc-{version}";
    }
}
