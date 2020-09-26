using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Yoakke.DataStructures;
using Yoakke.Lir;
using Yoakke.Lir.Backend;
using Yoakke.Lir.Backend.Toolchain;
using Yoakke.Lir.Instructions;
using Yoakke.Lir.Passes;
using Yoakke.Lir.Runtime;
using Yoakke.Lir.Values;
using OperatingSystem = Yoakke.Lir.Backend.OperatingSystem;
using Type = Yoakke.Lir.Types.Type;

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
#elif true

            var uncheckedAsm = new UncheckedAssembly("test_app");
            var b = new Builder(uncheckedAsm);

            var main = b.DefineProc("main");
            main.Return = Type.I32;
            main.Visibility = Visibility.Public;

            var c = b.DefineConst("hello", Type.I32.NewValue(62));
            b.Ret(b.Load(c));

            var targetTriplet = new TargetTriplet(CpuFamily.X86, OperatingSystem.Windows);
            var toolchain = Toolchains.Supporting(targetTriplet).First();

            var build = new Build
            {
                OutputKind = OutputKind.Executable,
                OutputPath = "C:/TMP/globals.exe",
                IntermediatesDirectory = "C:/TMP/test_app_build",
                Assembly = b.Assembly,
                CodePass = new CodePassSet(),
            };

            toolchain.Compile(build);
            
            Console.WriteLine($"Errors: {build.HasErrors}");
            foreach (var err in build.Status.Errors)
            {
                Console.WriteLine(err.GetErrorMessage());
            }
            Console.WriteLine("\n");
            Console.WriteLine(build.GetIrCode());
            Console.WriteLine("\n");
            Console.WriteLine(build.GetAssemblyCode());
            Console.WriteLine("\n");

            Debug.Assert(build.CheckedAssembly != null);
            //var vm = new VirtualMachine(build.CheckedAssembly);
            //var res = vm.Execute("main", new List<Value> { });
            //Console.WriteLine($"VM result = {res}");

            Console.WriteLine();
            foreach (var (name, timeSpan) in build.Metrics.TimeMetrics)
            {
                Console.WriteLine($"{name} took: {(int)timeSpan.TotalMilliseconds} ms");
            }
#endif
        }
    }
}
