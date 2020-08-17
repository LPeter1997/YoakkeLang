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
            var triplet = new TargetTriplet(CpuFamily.X86, OperatingSystem.Windows);
            System.Console.WriteLine(triplet);
#endif
        }
    }
}
