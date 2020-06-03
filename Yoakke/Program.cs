using System;
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
            /*try
            {
                var src = new Source("some.yk", @$"
    const main = proc(x: i32) -> i32 {{
        x
    }}
");
                var tokens = Lexer.Lex(src);
                var ast = Parser.ParseProgram(tokens);

                var symbolTable = new SymbolTable();
                // Construct the i32 type
                {
                    var i32_sym = new ConstSymbol("i32", new TypeValue(Type.I32));
                    symbolTable.GlobalScope.Define(i32_sym);
                }

                DeclareSymbol.Declare(symbolTable, ast);
                DefineSymbol.Define(ast);
                AssignConstantSymbol.Assign(ast);

                var entry = symbolTable.GlobalScope.Reference("main");
            }
            catch (CompileError error)
            {
                error.Show();
            }*/

            var asm = new Assembly();
            var proc = new Proc("main", IR.Type.I32);
            var bb = new BasicBlock("start");
            proc.BasicBlocks.Add(bb);
            asm.Procedures.Add(proc);

            bb.Instructions.Add(new AllocInstruction(0, IR.Type.I32));
            bb.Instructions.Add(new AllocInstruction(1, IR.Type.I32));

            Console.WriteLine(IrDump.Dump(asm));
        }
    }
}
