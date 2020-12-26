using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using Yoakke.Compiler.Compile;
using Yoakke.Compiler.Semantic;
using Yoakke.Lir.Backend;
using Yoakke.Lir.Backend.Toolchain;
using Yoakke.Lir.Passes;
using Yoakke.Lir.Runtime;
using Yoakke.Lir.Status;
using Yoakke.Lir.Utils;
using Yoakke.Lir.Values;
using Yoakke.Syntax;
using Yoakke.Syntax.Error;
using Yoakke.Text;
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
                IntermediatesDirectory = IntermediatesDirectory,
                OutputKind = OutputKind.DynamicLibrary,
                OutputPath = Path.Combine(IntermediatesDirectory, $"{TestContext.TestName}_{uniqueId++}.dll"),
                CheckedAssembly = assembly,
            };
            toolchain.Compile(build);
            Assert.IsFalse(build.HasErrors);
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
            var status = new BuildStatus();
            var asm = assembly.Check(status);
            Assert.AreEqual(0, status.Errors.Count);
            return asm;
        }

        protected void TestOnAllBackends<TFunc>(string source, Value expected, params Value[] args)
            where TFunc : Delegate
        {
            var srcFile = new SourceFile($"{TestContext.TestName}.yk", source);
            var syntaxStatus = new SyntaxStatus();
            var tokens = Lexer.Lex(srcFile, syntaxStatus);
            var parser = new Parser(tokens, syntaxStatus);
            var prg = parser.ParseFile();
            Assert.AreEqual(0, syntaxStatus.Errors.Count);
            var ast = ParseTreeToAst.Convert(prg);
            ast = new Desugaring().Desugar(ast);

            var system = new DependencySystem("../../../../../stdlib");
            var symTab = system.SymbolTable;
            SymbolResolution.Resolve(symTab, ast);

            // Compilation
            var buildStatus = new BuildStatus();
            var asm = system.Compile(ast, buildStatus);
            Assert.AreEqual(0, buildStatus.Errors.Count);
            Assert.IsNotNull(asm);
            new CodePassSet().Pass(asm);

            TestOnAllBackends<TFunc>(asm, expected, args);
        }
    }
}
