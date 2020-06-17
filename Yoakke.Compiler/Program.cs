using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Yoakke.Ast;
using Yoakke.Backend;
using Yoakke.Compiler.IR;
using Yoakke.Compiler.IR.Passes;
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

                //Console.WriteLine(ast.DumpTree());

                Checks.CheckAll(ast);

                var asm = IR.Compiler.Compile(ast);

                var passes = new List<IPass>
                {
                    new RemoveVoid(),
                    new JumpThreading(),
                    new DeadCodeElimination(),
                    new ConstantFolding(),
                };
                Optimizer.Optimize(asm, passes);

                var namingCtx = new NamingContext(asm);

                Console.WriteLine("IR code:\n");
                Console.WriteLine(IrDump.Dump(namingCtx));

                Console.WriteLine("\n\nC code:\n");
                var cBackend = new CCodegen();
                Console.WriteLine(cBackend.Compile(namingCtx));
            }
            catch (CompileError error)
            {
                error.Show();
            }
        }
    }
}
