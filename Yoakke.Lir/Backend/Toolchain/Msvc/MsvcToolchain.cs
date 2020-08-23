using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Backend.Toolchain.Msvc
{
    /// <summary>
    /// The MSVC toolchain.
    /// </summary>
    public class MsvcToolchain : IToolchain
    {
        public IEnumerable<ITool> Tools
        {
            get
            {
                yield return Assembler;
                yield return Linker;
                yield return Archiver;
            }
        }
        public IList<string> SourceFiles { get; } = new List<string>();
        public OutputKind OutputKind { get => Linker.OutputKind; set => Linker.OutputKind = value; }

        public readonly IAssembler Assembler;
        public readonly ILinker Linker;
        public readonly IArchiver Archiver;

        public MsvcToolchain(string vcVarsAllPath)
        {
            Assembler = new MsvcAssembler(vcVarsAllPath);
            Linker = new MsvcLinker(vcVarsAllPath);
            Archiver = new MsvcArchiver(vcVarsAllPath);
        }

        public int Compile(string outputPath)
        {
            // First we assemble each file
            var assembledFiles = new List<string>();
            foreach (var file in SourceFiles)
            {
                var outFile = Path.ChangeExtension(file, ".o");
                var err = Assembler.Assemble(file, outFile);
                if (err != 0) return err;
                assembledFiles.Add(outFile);
            }
            if (OutputKind == OutputKind.Executable || OutputKind == OutputKind.DynamicLibrary)
            {
                // We use the linker
                foreach (var f in assembledFiles) Linker.SourceFiles.Add(f);
                Linker.Link(outputPath);
            }
            else if (OutputKind == OutputKind.StaticLibrary)
            {
                // We use the archiver
                foreach (var f in assembledFiles) Archiver.SourceFiles.Add(f);
                Archiver.Archive(outputPath);
            }
            return 0;
        }
    }
}
