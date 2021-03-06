﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using Yoakke.Lir.Backend;
using Yoakke.Lir.Backend.Toolchain;
using Yoakke.Lir.Passes;
using Yoakke.Lir.Runtime;
using Yoakke.Lir.Status;
using Yoakke.Lir.Utils;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Tests
{
    [TestClass]
    public abstract class TestBase
    {
        private static readonly string IntermediatesDirectory = "binaries";
        private static IToolchain NativeToolchain { get; set; }
        
        public TestContext TestContext { get; set; }
        private int uniqueId;

        static TestBase()
        {
            // We do the common initialization here

            // Create the binaries directory, where the binaries will go when compiled
            Directory.CreateDirectory(IntermediatesDirectory);
            NativeToolchain = Toolchains.All().First();
        }

        protected Value ToValue(object obj) => obj switch
        {
            Value v => v,

            UInt32 u32 => Type.U32.NewValue(u32),
            UInt64 u64 => Type.U64.NewValue(u64),

            Int32 i32 => Type.I32.NewValue(i32),
            Int64 i64 => Type.I64.NewValue(i64),

            _ => throw new NotImplementedException(),
        };

        protected object FromValue(Value v)
        {
            switch (v)
            {
            case Value.Int i:
            {
                var ty = (Type.Int)i.Type;
                return ty.Bits switch
                {
                    32 => ty.Signed ? (Int32)i.Value : (UInt32)i.Value,
                    64 => ty.Signed ? (Int64)i.Value : (UInt64)i.Value,
                    _ => throw new NotImplementedException(),
                };
            }

            default: throw new NotImplementedException();
            }
        }

        private void TestOnToolchain<TFunc>(IToolchain toolchain, Assembly assembly, Value expected, params Value[] args)
            where TFunc : Delegate
        {
            // Compile
            var build = new Build
            {
                CodePass = CodePassSet.BasicPass,
                IntermediatesDirectory = IntermediatesDirectory,
                OutputKind = OutputKind.DynamicLibrary,
                OutputPath = Path.Combine(IntermediatesDirectory, $"{TestContext.TestName}_{uniqueId++}.dll"),
                CheckedAssembly = assembly,
            };
            build.BuildError += (s, e) => Assert.Fail("Test has build error!");
            toolchain.Compile(build);
            // Load function
            var nativeArgs = args.Select(FromValue).ToArray();
            var proc = NativeUtils.LoadNativeProcedure<TFunc>(build.OutputPath, "entry", CallConv.Cdecl);
            // Test
            var result = proc.DynamicInvoke(nativeArgs);
            Assert.AreEqual(expected, ToValue(result));
        }

        private void TestOnVirtualMachine(Assembly assembly, Value expected, params Value[] args)
        {
            var vm = new VirtualMachine(assembly);
            var result = vm.Execute("entry", args);
            Assert.AreEqual(expected, result);
        }

        private void TestOnAllBackends<TFunc>(Assembly assembly, Value expected, params Value[] args) 
            where TFunc : Delegate
        {
            TestOnToolchain<TFunc>(NativeToolchain, assembly, expected, args);
            TestOnVirtualMachine(assembly, expected, args);
        }

        private Assembly Check(UncheckedAssembly assembly)
        {
            var asm = assembly.Check();
            // TODO: Errors
            return asm;
        }

        protected void TestOnVirtualMachine(Builder builder, Value expected, params Value[] args)
        {
            TestOnVirtualMachine(Check(builder.Assembly), expected, args);
        }

        protected void TestOnAllBackends<TFunc>(Builder builder, Value expected, params Value[] args)
            where TFunc : Delegate
        {
            TestOnAllBackends<TFunc>(Check(builder.Assembly), expected, args);
        }

        protected Builder GetBuilder(Type type)
        {
            var asm = new UncheckedAssembly(TestContext.TestName);
            asm.ValidationError += (s, e) => Assert.Fail("Test has validation error!");
            var builder = new Builder(asm);
            var entry = builder.DefineProc("entry");
            entry.Visibility = Visibility.Public;
            entry.Return = type;
            return builder;
        }
    }
}
