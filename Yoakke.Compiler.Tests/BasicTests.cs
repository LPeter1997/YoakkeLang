﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Yoakke.Compiler.Semantic;
using Type = Yoakke.Compiler.Semantic.Type;

namespace Yoakke.Compiler.Tests
{
    [TestClass]
    public class ImplicitReturnTests
    {
        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void CreateBinariesFolder(TestContext testContext)
        {
            Directory.CreateDirectory("binaries");
        }

        private T CompileAndLoadFunc<T>(string source, string fname = "foo") where T : Delegate
        {
            var output = Path.GetFullPath($"binaries/{TestContext.TestName}.dll");
            var compiler = new Compiler
            {
                Source = new Syntax.Source("test.yk", source),
                OutputType = OutputType.Shared,
                OutputPath = output,
            };
            var exitCode = compiler.Execute();
            Assert.AreEqual(exitCode, 0);
            return NativeUtils.LoadNativeMethod<T>(output, fname);
        }

        [TestMethod]
        public void ReturnNumberLiteral()
        {
            string source = @"const foo = proc() -> i32 { 7 };";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 7);
        }

        [TestMethod]
        public void ReturnDefinedConstant()
        {
            string source = @"
            const Value = 15;    
            const foo = proc() -> i32 { Value };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 15);
        }

        [TestMethod]
        public void ReturnDefinedConstantAfterFunction()
        {
            string source = @"
            const foo = proc() -> i32 { Value };
            const Value = 15;
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 15);
        }

        [TestMethod]
        public void ReturnValueReturnedByFunction()
        {
            string source = @"
            const bar = proc() -> i32 { 3 };
            const foo = proc() -> i32 { bar() };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 3);
        }

        [TestMethod]
        public void ReturnValueReturnedByFunctionAfterFunction()
        {
            string source = @"
            const foo = proc() -> i32 { bar() };
            const bar = proc() -> i32 { 123 };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 123);
        }

        [TestMethod]
        public void ReturnValueReturnedByFunctionCompileTime()
        {
            string source = @"
            const bar = proc() -> i32 { 9 };
            const Value = bar();
            const foo = proc() -> i32 { Value };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 9);
        }

        [TestMethod]
        public void ReturnValueReturnedByFunctionCompileTimeReverseOrder()
        {
            string source = @"
            const Value = bar();
            const foo = proc() -> i32 { Value };
            const bar = proc() -> i32 { 9 };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 9);
        }

        [TestMethod]
        public void ReturnValueIsParameter()
        {
            string source = @"
            const foo = proc(x: i32) -> i32 { x };
";
            var f = CompileAndLoadFunc<Func<Int32, Int32>>(source);
            Assert.AreEqual(f(2), 2);
            Assert.AreEqual(f(5), 5);
            Assert.AreEqual(f(123), 123);
        }

        [TestMethod]
        public void ReturnValueIsParameterReassigned()
        {
            string source = @"
            const foo = proc(x: i32) -> i32 { 
                x = 8;
                x 
            };
";
            var f = CompileAndLoadFunc<Func<Int32, Int32>>(source);
            Assert.AreEqual(f(2), 8);
            Assert.AreEqual(f(5), 8);
            Assert.AreEqual(f(123), 8);
        }

        [TestMethod]
        public void SimpleTernaryImplementation()
        {
            string source = @"
            const foo = proc(condition: bool, first: i32, second: i32) -> i32 { 
                if condition { first } else { second }
            };
";
            var f = CompileAndLoadFunc<Func<bool, Int32, Int32, Int32>>(source);
            Assert.AreEqual(f(true, 2, 9), 2);
            Assert.AreEqual(f(false, 2, 9), 9);
            Assert.AreEqual(f(false, 123, 657), 657);
            Assert.AreEqual(f(true, 123, 657), 123);
        }

        [TestMethod]
        public void SimpleTernaryImplementationCompileTime()
        {
            string source = @"
            const ternary = proc(condition: bool, first: i32, second: i32) -> i32 { 
                if condition { first } else { second }
            };
            const foo = proc() -> i32 {
                const Value = ternary(true, 3, 8);
                Value
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 3);
        }

        [TestMethod]
        public void SimpleTernaryImplementationCompileTime2()
        {
            string source = @"
            const ternary = proc(condition: bool, first: i32, second: i32) -> i32 { 
                if condition { first } else { second }
            };
            const foo = proc() -> i32 {
                const Value = ternary(false, 3, 8);
                Value
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 8);
        }

        [TestMethod]
        public void SimpleTernaryImplementationCompileTime3()
        {
            string source = @"
            const ternary = proc(condition: bool, first: i32, second: i32) -> i32 { 
                if condition { first } else { second }
            };
            const foo = proc() -> i32 {
                const Value1 = ternary(true, 3, 8);
                const Value2 = ternary(true, 9, 17);
                const Value3 = ternary(false, 45, 84);
                Value2
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 9);
        }
    }

    [TestClass]
    public class TypeErrorTests
    {
        private void Compile(string source)
        {
            var output = Path.GetFullPath($"binaries/should_not_exist.dll");
            var compiler = new Compiler
            {
                Source = new Syntax.Source("test.yk", source),
                DumpIr = true, // So we don't compile
            };
            compiler.Execute();
        }

        private bool PairwiseEquals((Type, Type) set1, (Type, Type) set2) =>
               (set1.Item1.EqualsNonNull(set2.Item1) && set1.Item2.EqualsNonNull(set2.Item2))
            || (set1.Item1.EqualsNonNull(set2.Item2) && set1.Item2.EqualsNonNull(set2.Item1));

        [TestMethod]
        public void ConstantTypeMismatch()
        {
            string source = @"const Foo: i32 = true;";
            var err = Assert.ThrowsException<TypeError>(() => Compile(source));
            Assert.IsTrue(PairwiseEquals((err.First, err.Second), (Type.I32, Type.Bool)));
        }

        [TestMethod]
        public void ConstantTypeMatch()
        {
            string source = @"const Foo: bool = true;";
            Compile(source);
        }

        [TestMethod]
        public void ParameterTypeMismatch()
        {
            string source = @"
            const Value = foo(true);
            const foo = proc(x: i32) {
            };
";
            var err = Assert.ThrowsException<TypeError>(() => Compile(source));
            Assert.IsTrue(PairwiseEquals((err.First, err.Second), (Type.I32, Type.Bool)));
        }

        [TestMethod]
        public void ParameterCountMismatch()
        {
            string source = @"
            const Value = foo(1);
            const foo = proc(x: i32, y: bool) {
            };
";
            var err = Assert.ThrowsException<TypeError>(() => Compile(source));
            Assert.IsTrue(err.First is Type.Proc);
            Assert.IsTrue(err.Second is Type.Proc);
            var p1 = (Type.Proc)err.First;
            var p2 = (Type.Proc)err.Second;
            Assert.IsTrue(p1.Parameters.Count == 1 || p2.Parameters.Count == 1);
            Assert.IsTrue(p1.Parameters.Count == 2 || p2.Parameters.Count == 2);
        }

        [TestMethod]
        public void ImplicitReturnTypeMismatch()
        {
            string source = @"
            const foo = proc() {
                0
            };
";
            var err = Assert.ThrowsException<TypeError>(() => Compile(source));
            Assert.IsTrue(PairwiseEquals((err.First, err.Second), (Type.I32, Type.Unit)));
        }

        [TestMethod]
        public void ReturnTypeMismatch()
        {
            string source = @"
            const foo = proc() -> bool {
                0
            };
";
            var err = Assert.ThrowsException<TypeError>(() => Compile(source));
            Assert.IsTrue(PairwiseEquals((err.First, err.Second), (Type.I32, Type.Bool)));
        }

        [TestMethod]
        public void EarlyImplicitReturnError()
        {
            string source = @"
            const foo = proc() -> i32 {
                {
                    123
                }
                0
            };
";
            var err = Assert.ThrowsException<TypeError>(() => Compile(source));
            Assert.IsTrue(PairwiseEquals((err.First, err.Second), (Type.I32, Type.Unit)));
        }

        [TestMethod]
        public void EarlyImplicitReturnErrorComptime()
        {
            string source = @"
            const A = {
                {
                    123
                }
                0
            };
";
            var err = Assert.ThrowsException<TypeError>(() => Compile(source));
            Assert.IsTrue(PairwiseEquals((err.First, err.Second), (Type.I32, Type.Unit)));
        }

        [TestMethod]
        public void IfElseMismatch()
        {
            string source = @"
            const foo = proc() {
                if true { 0 } else { false }
            };
";
            var err = Assert.ThrowsException<TypeError>(() => Compile(source));
            Assert.IsTrue(PairwiseEquals((err.First, err.Second), (Type.I32, Type.Bool)));
        }

        [TestMethod]
        public void IfElseMismatchComptime()
        {
            string source = @"
            const A = if true { 0 } else { false };
";
            var err = Assert.ThrowsException<TypeError>(() => Compile(source));
            Assert.IsTrue(PairwiseEquals((err.First, err.Second), (Type.I32, Type.Bool)));
        }

        [TestMethod]
        public void IfElseConditionMismatch()
        {
            string source = @"
            const foo = proc() {
                if 4 { 0 } else { 1 }
            };
";
            var err = Assert.ThrowsException<TypeError>(() => Compile(source));
            Assert.IsTrue(PairwiseEquals((err.First, err.Second), (Type.I32, Type.Bool)));
        }

        [TestMethod]
        public void IfElseConditionMismatchComptime()
        {
            string source = @"
            const A = if 4 { 0 } else { 1 };
";
            var err = Assert.ThrowsException<TypeError>(() => Compile(source));
            Assert.IsTrue(PairwiseEquals((err.First, err.Second), (Type.I32, Type.Bool)));
        }

        [TestMethod]
        public void VariableTypeMismatch()
        {
            string source = @"
            const foo = proc() {
                var x: i32 = true;
            };
";
            var err = Assert.ThrowsException<TypeError>(() => Compile(source));
            Assert.IsTrue(PairwiseEquals((err.First, err.Second), (Type.I32, Type.Bool)));
        }

        [TestMethod]
        public void VariableTypeMismatchComptime()
        {
            string source = @"
            const A = {
                var x: i32 = true;
                x
            };
";
            var err = Assert.ThrowsException<TypeError>(() => Compile(source));
            Assert.IsTrue(PairwiseEquals((err.First, err.Second), (Type.I32, Type.Bool)));
        }
    }

    [TestClass]
    public class VariableTests
    {
        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void CreateBinariesFolder(TestContext testContext)
        {
            Directory.CreateDirectory("binaries");
        }

        private T CompileAndLoadFunc<T>(string source, string fname = "foo") where T : Delegate
        {
            var output = Path.GetFullPath($"binaries/{TestContext.TestName}.dll");
            var compiler = new Compiler
            {
                Source = new Syntax.Source("test.yk", source),
                OutputType = OutputType.Shared,
                OutputPath = output,
            };
            var exitCode = compiler.Execute();
            Assert.AreEqual(exitCode, 0);
            return NativeUtils.LoadNativeMethod<T>(output, fname);
        }

        [TestMethod]
        public void InitializedValue()
        {
            string source = @"
            const foo = proc() -> i32 { 
                var x = 234;
                x
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 234);
        }

        [TestMethod]
        public void InitializedValueExecuteComptime()
        {
            string source = @"
            const bar = proc() -> i32 { 
                var x = 66;
                x
            };
            const Value = bar();
            const foo = proc() -> i32 { Value };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 66);
        }

        [TestMethod]
        public void InitializedValueComptime()
        {
            string source = @"
            const V = { 
                var x = 99;
                x
            };
            const foo = proc() -> i32 { V };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 99);
        }

        [TestMethod]
        public void ReassignValue()
        {
            string source = @"
            const foo = proc() -> i32 { 
                var x = 234;
                x = 56;
                x
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 56);
        }

        [TestMethod]
        public void ReassignValueExecuteComptime()
        {
            string source = @"
            const bar = proc() -> i32 { 
                var x = 66;
                x = 53;
                x
            };
            const Value = bar();
            const foo = proc() -> i32 { Value };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 53);
        }

        [TestMethod]
        public void ReassignValueComptime()
        {
            string source = @"
            const V = { 
                var x = 99;
                x = 23;
                x
            };
            const foo = proc() -> i32 { V };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 23);
        }

        [TestMethod]
        public void ReassignParameter()
        {
            string source = @"
            const foo = proc(x: i32) -> i32 { 
                x = 576;
                x
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 576);
        }

        [TestMethod]
        public void ReassignParameterComptime()
        {
            string source = @"
            const bar = proc(x: i32) -> i32 { 
                x = 22;
                x
            };
            const Value = bar(2);
            const foo = proc() -> i32 { Value };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 22);
        }
    }

    // TODO: Test if-else
    // TODO: Test structs
    // TODO: Test generics
}
