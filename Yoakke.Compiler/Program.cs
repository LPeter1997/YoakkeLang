using Yoakke.Lir;
using Yoakke.Lir.Backend;
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
            var proc = new Proc("main");
            proc.Visibility = Visibility.Public;
            proc.Return = i32;
            var asm = new Assembly();
            asm.Procedures.Add(proc);
            proc.BasicBlocks[0].Instructions.Add(new Instr.Ret(i32.NewValue(12)));

            System.Console.WriteLine(asm);
            System.Console.WriteLine("\n\n");

            var tt = new TargetTriplet(CpuFamily.X86, OperatingSystem.Windows);
            var tc = new Toolchain
            {
            };
            var be = new Lir.Backend.Backends.NasmX86Backend(tc);
            var code = be.Compile(tt, asm);

            System.Console.WriteLine(code);
#endif
        }
    }
}
