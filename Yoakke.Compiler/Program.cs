using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using Yoakke.Compiler.Syntax;
using Yoakke.DataStructures;
using Yoakke.Lir;
using Yoakke.Lir.Backend;
using Yoakke.Lir.Backend.Backends;
using Yoakke.Lir.Backend.Toolchain;
using Yoakke.Lir.Backend.Toolchain.Msvc;
using Yoakke.Lir.Instructions;
using Yoakke.Lir.Runtime;
using Yoakke.Lir.Types;
using Yoakke.Lir.Values;
using Yoakke.Text;
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

            var asm = new Assembly("test_app");
            var builder = new Builder(asm);

            /*var times = builder.DefineExtern(
                "times",
                new Type.Proc(CallConv.Cdecl, Type.I32, new ValueList<Type> { Type.I32, Type.I32 }), 
                "C:/TMP/globals.obj");*/

            var main = builder.DefineProc("main");
            main.CallConv = CallConv.Cdecl;
            main.Return = Type.I32;
            main.Visibility = Visibility.Public;

            var currentBB = builder.CurrentBasicBlock;
            var thenBB = builder.DefineBasicBlock("then");
            var elseBB = builder.DefineBasicBlock("els");

            builder.CurrentBasicBlock = currentBB;
            builder.JmpIf(Type.I32.NewValue(34), thenBB, elseBB);

            builder.CurrentBasicBlock = thenBB;
            builder.Ret(Type.I32.NewValue(23));

            builder.CurrentBasicBlock = elseBB;
            builder.Ret(Type.I32.NewValue(97));

            // Dump IR code
            Console.WriteLine(asm);
            Console.WriteLine("\n\n");

            var tt = new TargetTriplet(CpuFamily.X86, OperatingSystem.Windows);
            var tc = Toolchains.Supporting(tt).First();

            // Compile to ASM
            var code = tc.Backend.Compile(asm);
            Console.WriteLine(code);

            var vm = new VirtualMachine(asm);
            var res = vm.Execute("main", new List<Value> { });
            Console.WriteLine($"VM result = {res}");

            // Compile it to backend
            tc.Assemblies.Add(asm);
            tc.BuildDirectory = "C:/TMP/test_app_build";

            var err = tc.Compile("C:/TMP/globals.exe");
            Console.WriteLine($"Toolchain exit code: {err}");
#endif
        }
    }
}
