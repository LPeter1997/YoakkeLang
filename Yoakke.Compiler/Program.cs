using System.Diagnostics;
using System.Linq;
using Yoakke.Lir;
using Yoakke.Lir.Backend;
using Yoakke.Lir.Backend.Backends;
using Yoakke.Lir.Backend.Toolchain.Msvc;
using Yoakke.Lir.Instructions;
using Yoakke.Lir.Runtime;
using Yoakke.Lir.Types;
using Yoakke.Lir.Values;

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
            // First we build the IR code
            var proc = new Proc("main");
            proc.Visibility = Visibility.Public;
            proc.Return = Type.I32;
            var asm = new Assembly("test_app");
            asm.Procedures.Add(proc);
            var someNumber = new Extern("some_number", Type.I32, "C:/TMP/globals.obj");
            asm.Externals.Add(someNumber);
            proc.BasicBlocks[0].Instructions.Add(new Instr.Ret(new Value.Extern(someNumber)));

            // Dump IR code
            System.Console.WriteLine(asm);
            System.Console.WriteLine("\n\n");

            // TODO: Execution
            // var vm = new VirtualMachine(asm);
            // var res = vm.Execute("main");
            // System.Console.WriteLine($"VM result = {res}");

            // Compile it to backend
            var tt = new TargetTriplet(CpuFamily.X86, OperatingSystem.Windows);
            var tcLocator = new MsvcToolchainLocator();
            var tc = tcLocator.Locate().First();

            tc.Assemblies.Add(asm);
            tc.BuildDirectory = "C:/TMP/test_app_build";
            tc.TargetTriplet = tt;

            tc.AddObjectFile("C:/TMP/globals.obj");
            var err = tc.Compile("C:/TMP/globals.exe");
            System.Console.WriteLine($"Toolchain exit code: {err}");
#endif
        }
    }
}
