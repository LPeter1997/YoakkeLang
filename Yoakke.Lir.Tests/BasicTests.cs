using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Yoakke.Lir.Backend;
using Yoakke.Lir.Backend.Toolchain;
using Yoakke.Lir.Runtime;
using Yoakke.Lir.Utils;
using Yoakke.Lir.Values;

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
            Assert.AreEqual(expected, Types.Type.I32.NewValue(result));
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
            entry.Return = Types.Type.I32;
            return builder;
        }

        // Actual tests

        [TestMethod]
        public void ReturnConstant()
        {
            var b = GetBuilder();
            b.Ret(Types.Type.I32.NewValue(263));
            TestOnAllBackends(b, Types.Type.I32.NewValue(263));
        }

        [TestMethod]
        public void ReturnParameter()
        {
            var b = GetBuilder();
            var entry = b.CurrentProc;

            var identity = b.DefineProc("identity");
            identity.Return = Types.Type.I32;
            var p = b.DefineParameter(Types.Type.I32);
            b.Ret(p);

            b.CurrentProc = entry;
            b.Ret(b.Call(identity, new List<Value> { Types.Type.I32.NewValue(524) }));
            TestOnAllBackends(b, Types.Type.I32.NewValue(524));
        }
    }
}
