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
        private int uniqueId;

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
                OutputPath = Path.Combine(IntermediatesDirectory, $"{TestContext.TestName}_{uniqueId++}.dll"),
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
            TestOnAllBackends(builder.Assembly.Check(), expected);
        }

        private Builder GetBuilder()
        {
            var asm = new UncheckedAssembly(TestContext.TestName);
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

        [TestMethod]
        public void CmpEqTrue()
        {
            var b = GetBuilder();
            b.Ret(b.CmpEq(Type.I32.NewValue(7), Type.I32.NewValue(7)));
            TestOnAllBackends(b, Type.I32.NewValue(1));
        }

        [TestMethod]
        public void CmpEqFalse()
        {
            var b = GetBuilder();
            b.Ret(b.CmpEq(Type.I32.NewValue(62), Type.I32.NewValue(25)));
            TestOnAllBackends(b, Type.I32.NewValue(0));
        }

        [TestMethod]
        public void CmpNeTrue()
        {
            var b = GetBuilder();
            b.Ret(b.CmpNe(Type.I32.NewValue(15), Type.I32.NewValue(27)));
            TestOnAllBackends(b, Type.I32.NewValue(1));
        }

        [TestMethod]
        public void CmpNeFalse()
        {
            var b = GetBuilder();
            b.Ret(b.CmpNe(Type.I32.NewValue(16), Type.I32.NewValue(16)));
            TestOnAllBackends(b, Type.I32.NewValue(0));
        }

        [TestMethod]
        public void CmpGrTrue()
        {
            var b = GetBuilder();
            b.Ret(b.CmpGr(Type.I32.NewValue(273), Type.I32.NewValue(238)));
            TestOnAllBackends(b, Type.I32.NewValue(1));
        }

        [TestMethod]
        public void CmpGrFalse()
        {
            var b = GetBuilder();
            b.Ret(b.CmpGr(Type.I32.NewValue(27), Type.I32.NewValue(42)));
            TestOnAllBackends(b, Type.I32.NewValue(0));

            b = GetBuilder();
            b.Ret(b.CmpGr(Type.I32.NewValue(27), Type.I32.NewValue(27)));
            TestOnAllBackends(b, Type.I32.NewValue(0));
        }

        [TestMethod]
        public void CmpLeTrue()
        {
            var b = GetBuilder();
            b.Ret(b.CmpLe(Type.I32.NewValue(238), Type.I32.NewValue(273)));
            TestOnAllBackends(b, Type.I32.NewValue(1));
        }

        [TestMethod]
        public void CmpLeFalse()
        {
            var b = GetBuilder();
            b.Ret(b.CmpLe(Type.I32.NewValue(42), Type.I32.NewValue(27)));
            TestOnAllBackends(b, Type.I32.NewValue(0));

            b = GetBuilder();
            b.Ret(b.CmpLe(Type.I32.NewValue(27), Type.I32.NewValue(27)));
            TestOnAllBackends(b, Type.I32.NewValue(0));
        }

        [TestMethod]
        public void CmpGrEqTrue()
        {
            var b = GetBuilder();
            b.Ret(b.CmpGrEq(Type.I32.NewValue(52), Type.I32.NewValue(38)));
            TestOnAllBackends(b, Type.I32.NewValue(1));

            b = GetBuilder();
            b.Ret(b.CmpGrEq(Type.I32.NewValue(38), Type.I32.NewValue(38)));
            TestOnAllBackends(b, Type.I32.NewValue(1));
        }

        [TestMethod]
        public void CmpGrEqFalse()
        {
            var b = GetBuilder();
            b.Ret(b.CmpGrEq(Type.I32.NewValue(15), Type.I32.NewValue(438)));
            TestOnAllBackends(b, Type.I32.NewValue(0));
        }

        [TestMethod]
        public void CmpLeEqTrue()
        {
            var b = GetBuilder();
            b.Ret(b.CmpLeEq(Type.I32.NewValue(38), Type.I32.NewValue(52)));
            TestOnAllBackends(b, Type.I32.NewValue(1));

            b = GetBuilder();
            b.Ret(b.CmpLeEq(Type.I32.NewValue(38), Type.I32.NewValue(38)));
            TestOnAllBackends(b, Type.I32.NewValue(1));
        }

        [TestMethod]
        public void CmpLeEqFalse()
        {
            var b = GetBuilder();
            b.Ret(b.CmpLeEq(Type.I32.NewValue(438), Type.I32.NewValue(16)));
            TestOnAllBackends(b, Type.I32.NewValue(0));
        }

        [TestMethod]
        public void Add()
        {
            var b = GetBuilder();
            b.Ret(b.Add(Type.I32.NewValue(25), Type.I32.NewValue(16)));
            TestOnAllBackends(b, Type.I32.NewValue(41));
        }

        [TestMethod]
        public void Sub()
        {
            var b = GetBuilder();
            b.Ret(b.Sub(Type.I32.NewValue(25), Type.I32.NewValue(16)));
            TestOnAllBackends(b, Type.I32.NewValue(9));
        }

        [TestMethod]
        public void Mul()
        {
            var b = GetBuilder();
            b.Ret(b.Mul(Type.I32.NewValue(7), Type.I32.NewValue(3)));
            TestOnAllBackends(b, Type.I32.NewValue(21));
        }

        [TestMethod]
        public void Div()
        {
            var b = GetBuilder();
            b.Ret(b.Div(Type.I32.NewValue(30), Type.I32.NewValue(6)));
            TestOnAllBackends(b, Type.I32.NewValue(5));

            b = GetBuilder();
            b.Ret(b.Div(Type.I32.NewValue(33), Type.I32.NewValue(6)));
            TestOnAllBackends(b, Type.I32.NewValue(5));
        }

        [TestMethod]
        public void Mod()
        {
            var b = GetBuilder();
            b.Ret(b.Mod(Type.I32.NewValue(26), Type.I32.NewValue(7)));
            TestOnAllBackends(b, Type.I32.NewValue(5));

            b = GetBuilder();
            b.Ret(b.Mod(Type.I32.NewValue(21), Type.I32.NewValue(7)));
            TestOnAllBackends(b, Type.I32.NewValue(0));
        }

        [TestMethod]
        public void And()
        {
            var b = GetBuilder();
            b.Ret(b.BitAnd(Type.I32.NewValue(0b11101101000), Type.I32.NewValue(0b00110101101)));
            TestOnAllBackends(b, Type.I32.NewValue(0b00100101000));
        }

        [TestMethod]
        public void Or()
        {
            var b = GetBuilder();
            b.Ret(b.BitOr(Type.I32.NewValue(0b11101101000), Type.I32.NewValue(0b00110101101)));
            TestOnAllBackends(b, Type.I32.NewValue(0b11111101101));
        }

        [TestMethod]
        public void Xor()
        {
            var b = GetBuilder();
            b.Ret(b.BitXor(Type.I32.NewValue(0b11101101000), Type.I32.NewValue(0b00110101101)));
            TestOnAllBackends(b, Type.I32.NewValue(0b11011000101));
        }
    }
}
