using System;
using System.Runtime.InteropServices;
using Yoakke.Compiler.Compile;
using Yoakke.Compiler.Semantic;
using Yoakke.DataStructures;
using Yoakke.Lir.Status;
using Yoakke.Lir.Values;
using Yoakke.Syntax;
using Yoakke.Syntax.Ast;
using Yoakke.Text;

namespace Yoakke.Compiler
{
    class TestDependencySystem : IDependencySystem
    {
        private SymbolTable symbolTable;

        public TestDependencySystem(SymbolTable symbolTable)
        {
            this.symbolTable = symbolTable;
        }

        public Symbol DefinedSymbolFor(Node node) => symbolTable.DefinedSymbol[node];
        public Symbol ReferredSymbolFor(Node node) => symbolTable.ReferredSymbol[node];

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
            if (expression is Expression.If)
            {
                return Semantic.Type.I32;
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
            var src = @"
            const foo = proc() -> i32 { 
                if true { 1 } else { 0 }
            };
";
            // Syntax
            var srcFile = new SourceFile("foo.yk", src);
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
            Console.WriteLine(asm);
            //var dependencySystem = new DependencySystem();
            //var assembly = dependencySystem.Compile(ast);
        }
    }
}
