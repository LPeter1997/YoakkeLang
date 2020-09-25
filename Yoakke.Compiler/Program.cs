using System;
using System.Collections.Generic;
using System.Linq;
using Yoakke.DataStructures;
using Yoakke.Lir;
using Yoakke.Lir.Backend;
using Yoakke.Lir.Backend.Toolchain;
using Yoakke.Lir.Instructions;
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

            var func = b.DefineProc("factorial");
            func.Return = Type.I32;
            var p = b.DefineParameter(Type.I32);
            var lastBlock = b.CurrentBasicBlock;
            var lessThan3Block = b.DefineBasicBlock("less_than_3");
            var elseBlock = b.DefineBasicBlock("els");

            b.CurrentBasicBlock = lastBlock;
            b.JmpIf(b.Cmp(Comparison.le, p, Type.I32.NewValue(3)), lessThan3Block, elseBlock);

            b.CurrentBasicBlock = lessThan3Block;
            b.Ret(p);

            b.CurrentBasicBlock = elseBlock;
            b.Ret(b.Mul(p, b.Call(func, new List<Value> { b.Sub(p, Type.I32.NewValue(1)) })));

            b.CurrentProc = main;
            b.Ret(b.Call(func, new List<Value> { Type.I32.NewValue(5) }));

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
        }
    }
}
