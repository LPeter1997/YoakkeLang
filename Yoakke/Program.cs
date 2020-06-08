using System;
using System.Diagnostics;
using System.IO;
using Yoakke.Ast;
using Yoakke.Backend;
using Yoakke.IR;
using Yoakke.Semantic;
using Yoakke.Syntax;
using Type = Yoakke.Semantic.Type;

namespace Yoakke
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var src = new Source("some.yk", File.ReadAllText("../../../../samples/test.yk"));
                var tokens = Lexer.Lex(src);
                var ast = Parser.ParseProgram(tokens);

                var symbolTable = new SymbolTable();
                symbolTable.DefineBuiltinType("i32", Type.I32);

                symbolTable.DefineIntrinsicFunction("@extern",
                    args =>
                    {
                        // TODO: Help type assertions
                        Debug.Assert(args.Count == 2);
                        var symbolName = (Semantic.Value.Str)args[0];
                        var symbolType = (Semantic.Value.Type_)args[1];
                        return new Semantic.Value.ExternSymbol(symbolName.Value, symbolType.Value);
                    });

                DeclareSymbol.Declare(symbolTable, ast);
                DefineSymbol.Define(ast);
                AssignConstantSymbol.Assign(ast);
                AssignType.Assign(ast);
                InferType.Infer(ast);

                //var entry = symbolTable.GlobalScope.Reference("main");

                var asm = Compiler.Compile(ast);

                Console.WriteLine("IR code:\n");
                Console.WriteLine(IrDump.Dump(asm));
                Console.WriteLine("\n\nC code:\n");
                var cBackend = new CCodegen();
                Console.WriteLine(cBackend.Compile(asm));
            }
            catch (CompileError error)
            {
                error.Show();
            }
        }
    }
}
