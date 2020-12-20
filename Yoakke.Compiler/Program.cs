using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using Yoakke.C.Syntax.Cpp;
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
using Yoakke.Syntax.Ast;
using Yoakke.Text;

namespace Yoakke.Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
#if false
            var system = new DependencySystem("../../../../../stdlib");
            var symTab = system.SymbolTable;

            // TODO: Temporary
            /*symTab.DefineBuiltin(
                "puts",
                new Semantic.Type.Proc(new ValueList<Semantic.Type> { new Semantic.Type.Ptr(Semantic.Type.U8) }, Semantic.Type.I32),
                system.Builder.DefineExtern(
                    "puts",
                    new Lir.Types.Type.Proc(Lir.CallConv.Cdecl, Lir.Types.Type.I32, new ValueList<Lir.Types.Type> { new Lir.Types.Type.Ptr(Lir.Types.Type.U8) }),
                    "libucrt.lib"));
            symTab.DefineBuiltin(
                "abs",
                new Semantic.Type.Proc(new ValueList<Semantic.Type> { Semantic.Type.I32 }, Semantic.Type.I32),
                system.Builder.DefineExtern(
                    "abs",
                    new Lir.Types.Type.Proc(Lir.CallConv.Cdecl, Lir.Types.Type.I32, new ValueList<Lir.Types.Type> { Lir.Types.Type.I32 }),
                    "libucrt.lib"));*/

            var ast = system.LoadAst(@"../../../../../samples/test.yk");

            //Console.WriteLine(prg.Dump());
            //Console.WriteLine(ast.Dump());

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
            var buildStatus = new BuildStatus();
            var asm = system.Compile(ast, buildStatus);
            foreach (var err in buildStatus.Errors)
            {
                Console.WriteLine(err.GetErrorMessage());
            }
            if (asm == null) return;

            new CodePassSet().Pass(asm);
            Console.WriteLine(asm);
            Console.WriteLine("\n");

            // Run in the VM
#if true
            var vm = new VirtualMachine(asm);
            var result = vm.Execute("main", new Value[] { });
            Console.WriteLine($"Result: {result}");
#endif

            // Build an exe
            var toolchain = Toolchains.All().First();
            var build = new Build
            {
                CheckedAssembly = asm,
                IntermediatesDirectory = "C:/TMP/program_build",
                OutputPath = "C:/TMP/program.exe",
            };
            build.ExternalBinaries.Add("libvcruntime.lib");
            build.ExternalBinaries.Add("msvcrt.lib");
            build.ExternalBinaries.Add("legacy_stdio_definitions.lib");
            //build.ExternalBinaries.Add("libcmt.lib");
            //build.ExternalBinaries.Add("kernel32.lib");
            toolchain.Compile(build);
            if (build.HasErrors)
            {
                Console.WriteLine("Build error!");
                foreach (var err in build.Status.Errors)
                {
                    Console.WriteLine(err.GetErrorMessage());
                }
            }
#endif
#elif false
            // TODO: We need to expand arguments
            // We need to refactor out expansion (and parsing) mechanism to work everywhere, not just in the main parser module
            var src = File.ReadAllText("C:/TMP/SDL2/include/SDL.h");
            var ppTokens = C.Syntax.Lexer.Lex(CppTextReader.Process(src));
            var pp = new PreProcessor("C:/TMP/SDL2/include/SDL.h");
            pp.AddIncludePath("C:/TMP/SDL2/include");
            pp.AddIncludePath(@"c:\Program Files (x86)\Microsoft Visual Studio\2019\Community\SDK\ScopeCppSDK\vc15\SDK\include\ucrt");
            pp.AddIncludePath(@"c:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Tools\MSVC\14.27.29110\include");
            pp.Define("WIN32", new UserMacro(false, false, new string[] { }, new C.Syntax.Token[] { }));
            var output = new StreamWriter(new FileStream("out.txt", FileMode.OpenOrCreate));
            foreach (var t in pp.Process(ppTokens))
            {
                output.WriteLine(t.Value);
            }
            output.Close();
#else
            var src = new SourceFile("foo.yk",
@"
const Frac = struct {
    nom: i32;
    den: i32;

	const new = proc(n: i32, d: i32) -> Frac {
        Frac{ nom = n; den = d; }
    };
};

const Half = Frac.new(1, 2);

const main = proc() -> i32 {
    Half.den
};
");

            //for (int i = 0; i < src.LineCount; ++i)
            //{
            //    Console.WriteLine($"line {i}: {src.Line(i).ToString()}");
            //}

            var diag = new Diagnostic
            {
                Severity = Severity.Error,
                Code = "E0523",
                Message = "You oofed this one",
                Information = 
                { 
                    new PrimaryDiagnosticInfo
                    {
                        Span = new Span(src, new Position(6, 8), 4),
                        Message = "u tried to make it here",
                    },
                    new SpannedDiagnosticInfo
                    {
                        Span = new Span(src, new Position(9, 6), 4),
                        Message = "and using it here",
                    },
                    new HintDiagnosticInfo
                    {
                        Message = "Did you mean to aaa?",
                    },
                },
            };
            new ConsoleDiagnosticRenderer().Render(diag);
#endif
        }
    }
}
