using System;
using System.Linq;
using Yoakke.Compiler.Compile;
using Yoakke.Compiler.Semantic;
using Yoakke.DataStructures;
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
        static void Main(string[] args)
        {
            var system = new DependencySystem("../../../../../stdlib");
            var symTab = system.SymbolTable;

            var diagRenderer = new TextDiagnosticRenderer { SyntaxHighlighter = new YoakkeReportingSyntaxHighlighter() };

            var ast = system.LoadAst(@"../../../../../samples/test.yk");

            // Semantics
            // TODO: Maye this should also be part of the dependency system?
            // Probably yes!
            SymbolResolution.Resolve(symTab, ast);

            // Compilation
            //var system = new DependencySystem(symTab);

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
            Console.WriteLine(asm);
            Console.WriteLine("\n");

            // Run in the VM
#if true
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
    }
}
