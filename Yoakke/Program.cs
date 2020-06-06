﻿using System;
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
                var src = new Source("some.yk", @$"
    const identity = proc(x: i32) -> i32 {{
        x
    }}
");
                var tokens = Lexer.Lex(src);
                var ast = Parser.ParseProgram(tokens);

                var symbolTable = new SymbolTable();
                // Construct the i32 type
                {
                    var i32_sym = new Symbol.Const("i32", new Semantic.Value.Type_(Type.I32));
                    symbolTable.GlobalScope.Define(i32_sym);
                }

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
