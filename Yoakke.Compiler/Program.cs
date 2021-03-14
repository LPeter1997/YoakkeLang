using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
//using Yoakke.Compiler.Compile;
using Yoakke.Compiler.Error;
using Yoakke.Compiler.Semantic;
using Yoakke.DataStructures;
using Yoakke.Debugging;
using Yoakke.Dependency;
using Yoakke.Lir.Backend;
using Yoakke.Lir.Backend.Toolchain;
using Yoakke.Lir.Passes;
using Yoakke.Lir.Runtime;
using Yoakke.Lir.Status;
using Yoakke.Lir.Values;
using Yoakke.Reporting;
using Yoakke.Reporting.Info;
using Yoakke.Reporting.Render;
using Yoakke.Syntax;
using Yoakke.Syntax.Error;

namespace Yoakke.Compiler
{
    class Program
    {
        private static Dictionary<IProcess, int> processIds = new Dictionary<IProcess, int>();
        private static Dictionary<IThread, int> threadIds = new Dictionary<IThread, int>();

        private static int GetProcessId(IProcess p)
        {
            if (processIds.TryGetValue(p, out var id)) return id;
            id = processIds.Count;
            processIds[p] = id;
            return id;
        }

        private static int GetThreadId(IThread p)
        {
            if (threadIds.TryGetValue(p, out var id)) return id;
            id = threadIds.Count;
            threadIds[p] = id;
            return id;
        }

        static void Main(string[] args)
        {
            using (var debugger = IDebugger.Create())
            {
                var proc = debugger.StartProcess(@"c:\Users\Péter Lenkefi\source\repos\DebuggerTest\DebuggerTest\foo.exe", null);

                Thread.Sleep(5000);
                debugger.ContinueProcess(proc);

                while (true) { }
            }
        }
    }

#if false
    class Program
    {
        private static List<ICompileError> errors = new List<ICompileError>();

        static void Main(string[] args)
        {
            var stopwatch = Stopwatch.StartNew();

            var system = new DependencySystem("../../../../../stdlib");
            system.CompileError += (s, err) => errors.Add(err);
            system.PhaseComplete += OnPhaseComplete;
            var symTab = system.SymbolTable;

            var ast = system.LoadAst(@"../../../../../samples/test.yk");

            // Semantics
            // TODO: Maye this should also be part of the dependency system?
            // Probably yes!
            SymbolResolution.Resolve(symTab, ast);

#if false
            system.TypeCheck(ast);
            // Type-query loop
            while (true)
            {
                var input = Console.ReadLine()?.Trim().Split('.').Select(x => x.Trim()).ToArray();
                Debug.Assert(input != null);
                var sym = system.ReferToConstTypeOf(input);
                Console.WriteLine(sym);
            }
#else
            var asm = system.Compile(ast);
            var compilationTime = stopwatch.ElapsedMilliseconds;

            Console.WriteLine(asm);
            Console.WriteLine("\n");

            // Run in the VM
#if false
            asm.ValidationError += OnICE;
            var checkedAsm = asm.Check();
            var vm = new VirtualMachine(checkedAsm);
            var result = vm.Execute("main", new Value[] { });
            Console.WriteLine($"Result: {result}");
#endif

            // Build an exe
            var toolchain = Toolchains.All().First();
            var build = new Build
            {
                CodePass = CodePassSet.BasicPass,
                Assembly = asm,
                IntermediatesDirectory = "C:/TMP/program_build",
                OutputPath = "C:/TMP/program.exe",
            };
            build.BuildWarning += (s, warn) =>
            {
                // TODO
            };
            build.BuildError += OnICE;
            build.ExternalBinaries.Add("libvcruntime.lib");
            build.ExternalBinaries.Add("msvcrt.lib");
            build.ExternalBinaries.Add("legacy_stdio_definitions.lib");
            //build.ExternalBinaries.Add("libcmt.lib");
            //build.ExternalBinaries.Add("kernel32.lib");
            toolchain.Compile(build);

            Console.WriteLine($"\nCompilation took {compilationTime} ms");
#endif
        }

        private static void OnICE(object sender, IBuildError error)
        {
            var diag = error.GetDiagnostic();
            diag.Severity = Severity.InternalError;
            diag.Message = $"Internal compiler error (ICE)\ninfo: {diag.Message}";
            diag.Information.Add(new FootnoteDiagnosticInfo
            {
                Message = "Please report this error at the official Yoakke repository.",
            });
            new TextDiagnosticRenderer().Render(diag);
            Environment.Exit(1);
        }

        private static void OnPhaseComplete(IDependencySystem system)
        {
            if (errors.Count == 0) return;

            var diagRenderer = new TextDiagnosticRenderer 
            { 
                SyntaxHighlighter = new YoakkeReportingSyntaxHighlighter() 
            };
            foreach (var err in errors) diagRenderer.Render(err.GetDiagnostic());
            Environment.Exit(1);
        }
    }
#endif
}
