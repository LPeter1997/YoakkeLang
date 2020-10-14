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
#if false
            var srcPath = @"../../../../../samples/test.yk";
            // Syntax
            var src = File.ReadAllText(srcPath);
            var srcFile = new SourceFile(srcPath, src);
            var syntaxStatus = new SyntaxStatus();
            var tokens = Lexer.Lex(srcFile, syntaxStatus);
            var parser = new Parser(tokens, syntaxStatus);
            var prg = parser.ParseFile();
            var ast = ParseTreeToAst.Convert(prg);
            ast = Desugaring.Desugar(ast);

            //Console.WriteLine(prg.Dump());
            Console.WriteLine(ast.Dump());

#if true
            // Semantics
            // TODO: Maye this should also be part of the dependency system?
            // Probably yes!
            var symTab = new SymbolTable();
            symTab.GlobalScope.Define(new Symbol.Const("i32", Semantic.Type.Type_, new Value.User(Semantic.Type.I32)));
            symTab.GlobalScope.Define(new Symbol.Const("i64", Semantic.Type.Type_, new Value.User(Semantic.Type.I64)));
            symTab.GlobalScope.Define(new Symbol.Const("bool", Semantic.Type.Type_, new Value.User(Semantic.Type.Bool)));
            symTab.GlobalScope.Define(new Symbol.Const("type", Semantic.Type.Type_, new Value.User(Semantic.Type.Type_)));
            new DefineScope(symTab).Define(ast);
            new DeclareSymbol(symTab).Declare(ast);
            new ResolveSymbol(symTab).Resolve(ast);

            // Compilation
            //var system = new DependencySystem(symTab);
            var system = new DependencySystem(symTab);
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
            var vm = new VirtualMachine(asm);
            var result = vm.Execute("main", new Value[] { });
            Console.WriteLine($"Result: {result}");

            // Build an exe
            var toolchain = Toolchains.All().First();
            var build = new Build
            {
                CheckedAssembly = asm,
                IntermediatesDirectory = "C:/TMP/program_build",
                OutputPath = "C:/TMP/program.exe",
            };
            toolchain.Compile(build);
#endif

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
