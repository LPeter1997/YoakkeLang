﻿using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            if (build.Assembly == null && build.CheckedAssembly == null)
            {
                build.Report(new EmptyBuild());
                return;
            }

            build.Metrics.StartTime("Overall");

            // Do the code pass first
            if (build.CodePass != null && build.Assembly != null)
            {
                build.Metrics.StartTime("Code passes");
                build.CodePass.Pass(build.Assembly, out var _);
                build.Metrics.EndTime();
            }

            // Then we validate
            if (build.Assembly != null && build.CheckedAssembly == null)
            {
                build.Metrics.StartTime("Validation");
                build.CheckedAssembly = build.Assembly.Check();
                build.Metrics.EndTime();
            }

            if (build.HasErrors)
            {
                build.Metrics.EndTime();
                return;
            }

            Debug.Assert(build.CheckedAssembly != null);

            if (build.OutputKind == OutputKind.Ir)
            {
                // We exit here, as the IR is in it's final form
                build.Metrics.EndTime();
                return;
            }

            // Make sure our intermediates directory exists
            Directory.CreateDirectory(build.IntermediatesDirectory);

            // We translate the IR assemblies to the given backend
            build.Metrics.StartTime("Translation to backend code");
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

            if (build.OutputKind == OutputKind.Assembly || build.HasErrors)
            {
                build.Metrics.EndTime();
                return;
            }

            // Invoke the linker or the archiver
            if (build.OutputKind == OutputKind.Executable || build.OutputKind == OutputKind.DynamicLibrary)
            {
                build.Metrics.StartTime("Linking");
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
