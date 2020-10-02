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
    class TestDependencySystem : IDependencySystem
    {
        public SymbolTable SymbolTable { get; }

        public TestDependencySystem(SymbolTable symbolTable)
        {
            SymbolTable = symbolTable;
        }

        public Lir.Types.Type TranslateToLirType(Semantic.Type type) => type switch
        {
            Semantic.Type.Prim prim => prim.Type,

            _ => throw new NotImplementedException(),
        };

        public void TypeCheck(Statement statement)
        {
            // TODO: Assume correct
        }

        public Semantic.Type TypeOf(Expression expression)
        {
            // TODO
            if (expression is Expression.Proc)
            {
                return new Semantic.Type.Proc(new ValueList<Semantic.Type> { }, Semantic.Type.I32);
            }
            if (expression is Expression.If || expression is Expression.Call)
            {
                return Semantic.Type.I32;
            }
            if (expression is Expression.Literal lit)
            {
                if (lit.Type == TokenType.IntLiteral) return Semantic.Type.I32;
                if (lit.Type == TokenType.KwTrue) return Semantic.Type.Bool;
                if (lit.Type == TokenType.KwFalse) return Semantic.Type.Bool;
            }
            throw new NotImplementedException();
        }

        public Value Evaluate(Expression expression)
        {
            throw new NotImplementedException();
        }

        public Semantic.Type EvaluateToType(Expression expression)
        {
            if (expression is Expression.Identifier ident)
            {
                if (ident.Name == "i32") return Semantic.Type.I32;
            }
            throw new NotImplementedException();
        }
    }

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
            var codegen = new Codegen(new TestDependencySystem(symTab));
            var buildStatus = new BuildStatus();
            var asm = codegen.Generate(ast, buildStatus);
            new CodePassSet().Pass(asm);
            Console.WriteLine(asm);
            Console.WriteLine("\n");
            foreach (var err in buildStatus.Errors)
            {
                Console.WriteLine(err.GetErrorMessage());
            }

            if (buildStatus.Errors.Count == 0)
            {
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
}
