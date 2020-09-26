﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir.Status;

namespace Yoakke.Lir.Backend.Toolchain
{
    /// <summary>
    /// A base for any standard toolchain that doesn't deviate from the standard flow.
    /// </summary>
    public abstract class StandardToolchain : IToolchain
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

        public abstract IBackend Backend { get; }
        public abstract IAssembler Assembler { get; }
        public abstract ILinker Linker { get; }
        public abstract IArchiver Archiver { get; }

        public abstract string Version { get; }

        public void Compile(Build build)
        {
            // There's no reason to continue, if there's no assembly
            if (build.Assembly == null)
            {
                build.Report(new EmptyBuild());
                return;
            }

            build.Metrics.StartTime("Overall");

            // First we validate
            build.Metrics.StartTime("Validation");
            var asm = build.Assembly.Check(build.Status);
            build.Metrics.EndTime();

            if (build.HasErrors)
            {
                build.Metrics.EndTime();
                return;
            }
            
            // Make sure our intermediates directory exists
            Directory.CreateDirectory(build.IntermediatesDirectory);

            // We translate the IR assemblies to the given backend
            build.Metrics.StartTime("Translation to backend code");
            var assemblyFile = Path.Combine(build.IntermediatesDirectory, $"{asm.Name}.asm");
            build.Extra["assemblyFile"] = assemblyFile;
            Backend.Compile(build);
            build.Metrics.EndTime();

            if (build.HasErrors)
            {
                build.Metrics.EndTime();
                return;
            }

            // Then we assemble each file
            build.Metrics.StartTime("Assembly");
            Assembler.Assemble(build);
            build.Metrics.EndTime();

            if (build.HasErrors)
            {
                build.Metrics.EndTime();
                return;
            }

            // We append external binaries here
            build.Extra["externalBinaries"] = asm.BinaryReferences.ToList();

            // Invoke the linker or the archiver
            if (build.OutputKind == OutputKind.Executable || build.OutputKind == OutputKind.DynamicLibrary)
            {
                build.Metrics.StartTime("Linking");
                // We need to explicitly tell the linker to export everything public
                build.Extra["publicSymbols"] = asm.Symbols
                    .Where(sym => sym.Visibility == Visibility.Public)
                    .ToList();
                // Invoke the linker
                Linker.Link(build);
                build.Metrics.EndTime();
            }
            else if (build.OutputKind == OutputKind.StaticLibrary)
            {
                build.Metrics.StartTime("Archiving");
                // We use the archiver
                Archiver.Archive(build);
                build.Metrics.EndTime();
            }
            else
            {
                // Object files, we do nothing
            }
            build.Metrics.EndTime();
        }
    }
}
