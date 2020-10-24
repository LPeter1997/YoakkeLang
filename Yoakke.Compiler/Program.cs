using System;
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
using Yoakke.Syntax;
using Yoakke.Syntax.Ast;
using Yoakke.Text;

namespace Yoakke.Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
#if true
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
            Console.WriteLine(ast.Dump());

            // Semantics
            // TODO: Maye this should also be part of the dependency system?
            // Probably yes!
            new DefineScope(symTab).Define(ast);
            new DeclareSymbol(symTab).Declare(ast);
            new ResolveSymbol(symTab).Resolve(ast);

            // Compilation
            //var system = new DependencySystem(symTab);
            
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
            build.ExternalBinaries.Add("libcmt.lib");
            build.ExternalBinaries.Add("kernel32.lib");
            toolchain.Compile(build);
            if (build.HasErrors)
            {
                Console.WriteLine("Build error!");
                foreach (var err in build.Status.Errors)
                {
                    Console.WriteLine(err.GetErrorMessage());
                }
            }
#else
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
#endif
        }
    }
}
