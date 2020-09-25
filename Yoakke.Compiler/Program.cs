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

            var factorial = b.DefineProc("factorial");
            factorial.Return = Type.I32;

            var p = b.DefineParameter(Type.I32);

            var begin = b.CurrentBasicBlock;
            var i = b.Alloc(Type.I32);
            var ret = b.Alloc(Type.I32);
            b.Store(i, Type.I32.NewValue(1));
            b.Store(ret, Type.I32.NewValue(1));

            var loopConditionBlock = b.DefineBasicBlock("loop_condition");
            var loopBlock = b.DefineBasicBlock("loop");
            var endLoopBlock = b.DefineBasicBlock("end_loop");
            
            b.CurrentBasicBlock = begin;
            b.Jmp(loopConditionBlock);

            b.CurrentBasicBlock = loopConditionBlock;
            b.JmpIf(b.CmpLeEq(b.Load(i), p), loopBlock, endLoopBlock);

            b.CurrentBasicBlock = loopBlock;
            b.Store(ret, b.Mul(b.Load(ret), b.Load(i)));
            b.Store(i, b.Add(b.Load(i), Type.I32.NewValue(1)));
            b.Jmp(loopConditionBlock);

            b.CurrentBasicBlock = endLoopBlock;
            b.Ret(b.Load(ret));

            b.CurrentProc = main;
            b.Ret(b.Call(factorial, new List<Value> { Type.I32.NewValue(5) }));

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
