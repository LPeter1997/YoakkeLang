using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Yoakke.Lir.Backend;
using Yoakke.Lir.Backend.Toolchain;
using Yoakke.Lir.Runtime;
using Yoakke.Lir.Types;
using Yoakke.Lir.Utils;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Tests
{
    // TODO: Note below
    // NOTE: For now we assume int32 everywhere
    // It's fine for initial tests
    // For later we'll need utilities to convert between native and non-native types and values.
    [TestClass]
    public class BasicTests
    {
        // Configuration and utilities

        private static readonly string IntermediatesDirectory = "binaries";
        private static IToolchain NativeToolchain { get; set; }
        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void Init(TestContext testContext)
        {
            NativeToolchain = Toolchains.All().First();
            Directory.CreateDirectory(IntermediatesDirectory);
        }

        private void TestOnToolchain(IToolchain toolchain, Assembly assembly, Value.Int expected)
        {
            // Compile
            var build = new Build
            {
                IntermediatesDirectory = IntermediatesDirectory,
                OutputKind = OutputKind.DynamicLibrary,
                OutputPath = Path.Combine(IntermediatesDirectory, $"{TestContext.TestName}.dll"),
            };
            build.Assemblies.Add(assembly);
            var exitCode = toolchain.Compile(build);
            Assert.AreEqual(0, exitCode);
            // Load function
            var proc = NativeUtils.LoadNativeProcedure<Func<Int32>>(build.OutputPath, "entry", CallConv.Cdecl);
            // Test
            var result = proc();
            Assert.AreEqual(expected, Type.I32.NewValue(result));
        }

        private void TestOnVirtualMachine(Assembly assembly, Value.Int expected)
        {
            var vm = new VirtualMachine(assembly);
            var result = vm.Execute("entry", new Value[] { });
            Assert.AreEqual(expected, result);
        }

        private void TestOnAllBackends(Assembly assembly, Value.Int expected)
        {
            TestOnToolchain(NativeToolchain, assembly, expected);
            TestOnVirtualMachine(assembly, expected);
        }

        private void TestOnAllBackends(Builder builder, Value.Int expected)
        {
            TestOnAllBackends(builder.Assembly, expected);
        }

        private Builder GetBuilder()
        {
            var asm = new Assembly(TestContext.TestName);
            var builder = new Builder(asm);
            var entry = builder.DefineProc("entry");
            entry.Visibility = Visibility.Public;
            entry.Return = Type.I32;
            return builder;
        }

        // Actual tests

        [TestMethod]
        public void ReturnConstant()
        {
            var b = GetBuilder();
            b.Ret(Type.I32.NewValue(263));
            TestOnAllBackends(b, Type.I32.NewValue(263));
        }

        [TestMethod]
        public void ReturnParameter()
        {
            var b = GetBuilder();
            var entry = b.CurrentProc;

            var identity = b.DefineProc("identity");
            identity.Return = Type.I32;
            var p = b.DefineParameter(Type.I32);
            b.Ret(p);

            b.CurrentProc = entry;
            b.Ret(b.Call(identity, new List<Value> { Type.I32.NewValue(524) }));
            TestOnAllBackends(b, Type.I32.NewValue(524));
        }

        [TestMethod]
        public void ModifyParameter()
        {
            var intPtr = new Type.Ptr(Type.I32);
            var b = GetBuilder();
            var entry = b.CurrentProc;

            var modify = b.DefineProc("modify");
            var p = b.DefineParameter(intPtr);
            b.Store(p, Type.I32.NewValue(73));
            b.Ret();

            b.CurrentProc = entry;
            var storage = b.Alloc(Type.I32);
            b.Store(storage, Type.I32.NewValue(62));
            b.Call(modify, new List<Value> { storage });
            b.Ret(b.Load(storage));
            TestOnAllBackends(b, Type.I32.NewValue(73));
        }

        [TestMethod]
        public void IfElseThenPart()
        {
            var b = GetBuilder();
            var entry = b.CurrentProc;

            var lastBlock = b.CurrentBasicBlock;
            var thenBlock = b.DefineBasicBlock("then");
            b.Ret(Type.I32.NewValue(36));

            var elsBlock = b.DefineBasicBlock("els");
            b.Ret(Type.I32.NewValue(383));

            b.CurrentBasicBlock = lastBlock;
            b.JmpIf(Type.I32.NewValue(1), thenBlock, elsBlock);

            TestOnAllBackends(b, Type.I32.NewValue(36));
        }

        [TestMethod]
        public void IfElseElsePart()
        {
            var b = GetBuilder();

            var lastBlock = b.CurrentBasicBlock;
            var thenBlock = b.DefineBasicBlock("then");
            b.Ret(Type.I32.NewValue(36));

            var elsBlock = b.DefineBasicBlock("els");
            b.Ret(Type.I32.NewValue(383));

            b.CurrentBasicBlock = lastBlock;
            b.JmpIf(Type.I32.NewValue(0), thenBlock, elsBlock);

            TestOnAllBackends(b, Type.I32.NewValue(383));
        }
    }
}
