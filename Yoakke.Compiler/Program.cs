using System;
using System.Collections.Generic;
using System.Linq;
using Yoakke.Lir;
using Yoakke.Lir.Backend;
using Yoakke.Lir.Backend.Toolchain;
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

            var arrTy = new Type.Array(Type.I32, 3);
            var arrPtr = b.Alloc(arrTy);
            var intPtr = b.Cast(new Type.Ptr(Type.I32), arrPtr);
            for (int i = 0; i < 3; ++i)
            {
                b.Store(b.Add(intPtr, Type.I32.NewValue(i)), Type.I32.NewValue(2 * i + 1));
            }
            b.Ret(b.Load(b.Add(intPtr, Type.I32.NewValue(0))));

            var targetTriplet = new TargetTriplet(CpuFamily.X86, OperatingSystem.Windows);
            var toolchain = Toolchains.Supporting(targetTriplet).First();

            var build = new Build
            {
                OutputKind = OutputKind.Executable,
                OutputPath = "C:/TMP/globals.exe",
                IntermediatesDirectory = "C:/TMP/test_app_build",
            };
            var asm = uncheckedAsm.Check();
            build.Assemblies.Add(asm);
            Console.WriteLine(asm);
            Console.WriteLine();
            Console.WriteLine(toolchain.Backend.Compile(asm));
            Console.WriteLine();

#if true

            var vm = new VirtualMachine(asm);
            var res = vm.Execute("main", new List<Value> { });
            Console.WriteLine($"VM result = {res}");

            var err = toolchain.Compile(build);
            Console.WriteLine($"Toolchain exit code: {err}");
            Console.WriteLine();
            foreach (var (name, timeSpan) in build.Metrics.TimeMetrics)
            {
                Console.WriteLine($"{name} took: {(int)timeSpan.TotalMilliseconds} ms");
            }
#endif
#endif
        }
    }
}
