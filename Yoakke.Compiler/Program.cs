using System.Diagnostics;
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
#elif false
            var proc = new Proc("main");
            proc.Visibility = Visibility.Public;
            proc.Return = Type.I32;
            var asm = new Assembly();
            asm.Procedures.Add(proc);
            var someNumber = new Extern("some_number", Type.I32, "C:/TMP/globals.obj");
            asm.Externals.Add(someNumber);
            proc.BasicBlocks[0].Instructions.Add(new Instr.Ret(new Value.Extern(someNumber)));

            System.Console.WriteLine(asm);
            System.Console.WriteLine("\n\n");

            var tt = new TargetTriplet(CpuFamily.X86, OperatingSystem.Windows);
            var be = new NasmX86Backend();
            var code = be.Compile(tt, asm);

            System.Console.WriteLine(code);
            System.Console.WriteLine("\n\n");

            var vm = new VirtualMachine(asm);
            var res = vm.Execute("main");
            System.Console.WriteLine($"VM result = {res}");
#else
            var tt = new TargetTriplet(CpuFamily.X86, OperatingSystem.Windows);
            var tcLocator = new MsvcToolchainLocator();
            if (tcLocator.TryLocate(out var tc))
            {
                // TODO: Eww
                Debug.Assert(tc != null);

                tc.TargetTriplet = tt;

                /*
                // Try linker
                tc.Linker.SourceFiles.Add("C:/TMP/hello.o");
                tc.Linker.SourceFiles.Add("C:/TMP/globals.obj");
                tc.Linker.OutputKind = OutputKind.DynamicLibrary;
                var err = tc.Linker.Link("C:/TMP/reee.dll");
                System.Console.WriteLine($"Linker exit code: {err}");
                // Try the archiver
                tc.Archiver.SourceFiles.Add("C:/TMP/globals.obj");
                err = tc.Archiver.Archive("C:/TMP/testlib.lib");
                System.Console.WriteLine($"Archiver exit code: {err}");
                */
                // Try the assembler
                tc.Assembler.SourceFiles.Add("C:/TMP/hello.asm");
                var err = tc.Assembler.Assemble("C:/TMP/oof.o");
                System.Console.WriteLine($"Assembler exit code: {err}");
            }
#endif
        }
    }
}
