using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

            // Semantics
            // TODO: Maye this should also be part of the dependency system?
            // Probably yes!
            var symTab = new SymbolTable();
            symTab.GlobalScope.Define(new Symbol.Const("i32", new Value.User(Lir.Types.Type.I32)));
            new DefineScope(symTab).Define(ast);
            new DeclareSymbol(symTab).Declare(ast);
            new ResolveSymbol(symTab).Resolve(ast);

            // Compilation
            //var system = new DependencySystem(symTab);
            var codegen = new Codegen();
            var buildStatus = new BuildStatus();
            var asm = codegen.Generate(ast, buildStatus);
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

            foreach (var err in build.Status.Errors)
            {
                Console.WriteLine(err.GetErrorMessage());
            }
        }
    }
}
