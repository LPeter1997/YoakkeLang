using Yoakke.Lir;
using Yoakke.Lir.Instructions;
using Yoakke.Lir.Types;

namespace Yoakke.Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
#if false
            if (Debugger.IsAttached && args.Length == 0)
            {
                // For simplicity we inject parameters so we can run from the IDE
                var cmp = new Compiler
                {
                    SourceFile = "../../../../../../samples/test.yk",
                    //DumpAst = true,
                    //DumpIr = true,
                    //DumpBackend = true,
                    ExecuteImmediately = true,
                    OptimizationLevel = 0,
                    BackendFlags = new string[] { "../../../../../../samples/ffi.c" },
                };
                cmp.OnExecute();
            }
            else
            {
                CommandLineApplication.Execute<Compiler>(args);
            }
#else
            var i32 = new Type.Int(true, 32);
            var p = new Proc("main");
            p.BasicBlocks[0].Instructions.Add(new Instr.Ret(i32.NewValue(0)));
            System.Console.WriteLine(p);
#endif
        }
    }
}
