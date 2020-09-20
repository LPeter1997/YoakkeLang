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
            var builder = new Builder(uncheckedAsm);

            var intPair = builder.DefineStruct(new Type[] { Type.I32, Type.I32 });

            var main = builder.DefineProc("main");
            main.Return = Type.I32;

#if false
            var sPtr = builder.Alloc(Type.I32);
            builder.Store(sPtr, Type.I32.NewValue(13));
            builder.Ret(builder.Load(sPtr));
#else
            var s = builder.DefineStruct(new Type[] { Type.I32, Type.I32, Type.I32 });
            var sPtr = builder.Alloc(s);
            builder.Store(builder.ElementPtr(sPtr, 0), Type.I32.NewValue(13));
            builder.Store(builder.ElementPtr(sPtr, 1), Type.I32.NewValue(29));
            builder.Store(builder.ElementPtr(sPtr, 2), Type.I32.NewValue(41));
            builder.Ret(builder.Load(builder.ElementPtr(sPtr, 0)));
#endif

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

            //var vm = new VirtualMachine(asm);
            //var res = vm.Execute("main", new List<Value> { });
            //Console.WriteLine($"VM result = {res}");

            var err = toolchain.Compile(build);
            Console.WriteLine($"Toolchain exit code: {err}");
            Console.WriteLine();
            foreach (var (name, timeSpan) in build.Metrics.TimeMetrics)
            {
                Console.WriteLine($"{name} took: {(int)timeSpan.TotalMilliseconds} ms");
            }

#if false
            var intPtr = new Type.Ptr(Type.I32);

            var modify = builder.DefineProc("modify");
            var param = builder.DefineParameter(intPtr);
            builder.Store(param, Type.I32.NewValue(123));
            builder.Ret();

            var main = builder.DefineProc("main");
            main.CallConv = CallConv.Cdecl;
            main.Return = Type.I32;
            main.Visibility = Visibility.Public;
            var intPlace = builder.Alloc(Type.I32);
            builder.Store(intPlace, Type.I32.NewValue(556));
            builder.Call(modify, new List<Value> { intPlace });
            var retValue = builder.Load(intPlace);
            builder.Ret(retValue);

            // Dump IR code
            Console.WriteLine(asm);
            Console.WriteLine("\n\n");

            var targetTriplet = new TargetTriplet(CpuFamily.X86, OperatingSystem.Windows);
            var toolchain = Toolchains.Supporting(targetTriplet).First();

            // Compile to ASM
            var code = toolchain.Backend.Compile(asm);
            Console.WriteLine(code);

            var vm = new VirtualMachine(asm);
            var res = vm.Execute("main", new List<Value> { });
            Console.WriteLine($"VM result = {res}");

            // Compile it to backend
            toolchain.Assemblies.Add(asm);
            toolchain.BuildDirectory = "C:/TMP/test_app_build";

            var err = toolchain.Compile("C:/TMP/globals.exe");
            Console.WriteLine($"Toolchain exit code: {err}");
#endif
#endif
        }
    }
}
