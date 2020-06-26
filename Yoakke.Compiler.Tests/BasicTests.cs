using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    public class ExplicitReturnTests
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
        public void ExplicitReturnNumberLiteral()
        {
            string source = @"const foo = proc() -> i32 { return 7; };";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 7);
        }

        [TestMethod]
        public void ExplicitReturnNumberLiteralInBlock()
        {
            string source = @"const foo = proc() -> i32 { return { 84 }; };";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 84);
        }

        [TestMethod]
        public void NestedExplicitReturnNumberLiteralInBlock()
        {
            string source = @"const foo = proc() -> i32 { { return { 62 }; } };";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 62);
        }

        [TestMethod]
        public void ExplicitReturnDefinedConstant()
        {
            string source = @"
            const Value = 15;    
            const foo = proc() -> i32 { return Value; };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 15);
        }

        [TestMethod]
        public void ExplicitReturnDefinedConstantAfterFunction()
        {
            string source = @"
            const foo = proc() -> i32 { return Value; };
            const Value = 15;
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 15);
        }

        [TestMethod]
        public void ExplicitReturnValueReturnedByFunction()
        {
            string source = @"
            const bar = proc() -> i32 { return 3; };
            const foo = proc() -> i32 { return bar(); };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 3);
        }

        [TestMethod]
        public void ExplicitReturnValueReturnedByFunctionAfterFunction()
        {
            string source = @"
            const foo = proc() -> i32 { return bar(); };
            const bar = proc() -> i32 { return 123; };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 123);
        }

        [TestMethod]
        public void ExplicitReturnValueReturnedByFunctionCompileTime()
        {
            string source = @"
            const bar = proc() -> i32 { return 9; };
            const Value = bar();
            const foo = proc() -> i32 { return Value; };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 9);
        }

        [TestMethod]
        public void ExplicitReturnValueReturnedByFunctionCompileTimeReverseOrder()
        {
            string source = @"
            const Value = bar();
            const foo = proc() -> i32 { return Value; };
            const bar = proc() -> i32 { return 9; };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 9);
        }

        [TestMethod]
        public void ExplicitReturnValueIsParameter()
        {
            string source = @"
            const foo = proc(x: i32) -> i32 { return x; };
";
            var f = CompileAndLoadFunc<Func<Int32, Int32>>(source);
            Assert.AreEqual(f(2), 2);
            Assert.AreEqual(f(5), 5);
            Assert.AreEqual(f(123), 123);
        }

        [TestMethod]
        public void ExplicitReturnValueIsParameterReassigned()
        {
            string source = @"
            const foo = proc(x: i32) -> i32 { 
                x = 8;
                return x;
            };
";
            var f = CompileAndLoadFunc<Func<Int32, Int32>>(source);
            Assert.AreEqual(f(2), 8);
            Assert.AreEqual(f(5), 8);
            Assert.AreEqual(f(123), 8);
        }

        [TestMethod]
        public void ExplicitSimpleTernaryImplementation()
        {
            string source = @"
            const foo = proc(condition: bool, first: i32, second: i32) -> i32 { 
                return if condition { first } else { second };
            };
";
            var f = CompileAndLoadFunc<Func<bool, Int32, Int32, Int32>>(source);
            Assert.AreEqual(f(true, 2, 9), 2);
            Assert.AreEqual(f(false, 2, 9), 9);
            Assert.AreEqual(f(false, 123, 657), 657);
            Assert.AreEqual(f(true, 123, 657), 123);
        }

        [TestMethod]
        public void ExplicitEarlyReturn()
        {
            string source = @"
            const foo = proc() -> i32 {
                var x = 12;
                if true { return x; }
                x = 34;
                return x;
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 12);
        }

        [TestMethod]
        public void ExplicitEarlyReturnNotRunning()
        {
            string source = @"
            const foo = proc() -> i32 {
                var x = 12;
                if false { return x; }
                x = 34;
                return x;
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 34);
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
               (set1.Item1.Equals(set2.Item1) && set1.Item2.Equals(set2.Item2))
            || (set1.Item1.Equals(set2.Item2) && set1.Item2.Equals(set2.Item1));

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
        public void ReturnTypeNoReturnMismatch()
        {
            string source = @"
            const foo = proc() -> bool {
            };
";
            Assert.ThrowsException<NotAllPathsReturnError>(() => Compile(source));
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
        public void IfElseWithElseIfsMismatch()
        {
            string source = @"
            const foo = proc() {
                if true { 0 } else if false { false } else { 0 }
            };
";
            var err = Assert.ThrowsException<TypeError>(() => Compile(source));
            Assert.IsTrue(PairwiseEquals((err.First, err.Second), (Type.I32, Type.Bool)));
        }

        [TestMethod]
        public void IfElseWithElseIfsMismatchElseUnit()
        {
            string source = @"
            const foo = proc() {
                if true { 0 } else if false { 1 } else { }
            };
";
            var err = Assert.ThrowsException<TypeError>(() => Compile(source));
            Assert.IsTrue(PairwiseEquals((err.First, err.Second), (Type.I32, Type.Unit)));
        }

        [TestMethod]
        public void IfElseWithElseIfsMismatchElseIfUnit()
        {
            string source = @"
            const foo = proc() {
                if true { 0 } else if false { } else { 3 }
            };
";
            var err = Assert.ThrowsException<TypeError>(() => Compile(source));
            Assert.IsTrue(PairwiseEquals((err.First, err.Second), (Type.I32, Type.Unit)));
        }

        [TestMethod]
        public void IfWithoutElseInExpression()
        {
            string source = @"
            const foo = proc() {
                var x = if true { 0 };
            };
";
            var err = Assert.ThrowsException<TypeError>(() => Compile(source));
            Assert.IsTrue(PairwiseEquals((err.First, err.Second), (Type.I32, Type.Unit)));
        }

        [TestMethod]
        public void IfWithElseIfWithoutElseInExpression()
        {
            string source = @"
            const foo = proc() {
                var x = if true { 0 } else if false { 1 };
            };
";
            var err = Assert.ThrowsException<TypeError>(() => Compile(source));
            Assert.IsTrue(PairwiseEquals((err.First, err.Second), (Type.I32, Type.Unit)));
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

        [TestMethod]
        public void StructFieldTypeMismatch()
        {
            string source = @"
            const Vector2 = struct {
                x: i32;
                y: i32;
            };
            const foo = proc() -> Vector2 {
                Vector2 {
                    x = 0;
                    y = true;
                }
            };
";
            var err = Assert.ThrowsException<TypeError>(() => Compile(source));
            Assert.IsTrue(PairwiseEquals((err.First, err.Second), (Type.I32, Type.Bool)));
        }

        [TestMethod]
        public void StructFieldTypeMismatchComptime()
        {
            string source = @"
            const Vector2 = struct {
                x: i32;
                y: i32;
            };
            const A = Vector2 {
                x = 0;
                y = true;
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

    [TestClass]
    public class IfElseTests
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
        public void IfWithoutElseRuns()
        {
            string source = @"
            const foo = proc() -> i32 { 
                var x = 3;
                if true {
                    x = 7;
                }
                x
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 7);
        }

        [TestMethod]
        public void IfWithoutElseDoesNotRun()
        {
            string source = @"
            const foo = proc() -> i32 { 
                var x = 34;
                if false {
                    x = 74;
                }
                x
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 34);
        }

        [TestMethod]
        public void IfWithElseRunsThen()
        {
            string source = @"
            const foo = proc() -> i32 { 
                var x = 8;
                if true {
                    x = 3;
                }
                else {
                    x = 2;
                }
                x
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 3);
        }

        [TestMethod]
        public void IfWithElseRunsElse()
        {
            string source = @"
            const foo = proc() -> i32 { 
                var x = 23;
                if false {
                    x = 37;
                }
                else {
                    x = 93;
                }
                x
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 93);
        }

        [TestMethod]
        public void IfWithElseIfsRunsFirstElseIf()
        {
            string source = @"
            const foo = proc() -> i32 { 
                var x = 23;
                if false {
                    x = 12;
                }
                else if true {
                    x = 44;
                }
                else if true {
                    x = 35;
                }
                else {
                    x = 62;
                }
                x
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 44);
        }

        [TestMethod]
        public void IfWithElseIfsRunsSecondElseIf()
        {
            string source = @"
            const foo = proc() -> i32 { 
                var x = 23;
                if false {
                    x = 12;
                }
                else if false {
                    x = 44;
                }
                else if true {
                    x = 35;
                }
                else {
                    x = 62;
                }
                x
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 35);
        }
    }

    [TestClass]
    public class StructTests
    {
        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void CreateBinariesFolder(TestContext testContext)
        {
            Directory.CreateDirectory("binaries");
        }

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
        public void NotAllMembersAreInitialized()
        {
            string source = @"
            const Vector2 = struct {
                x: i32;
                y: i32;
            };

            const foo = proc() -> i32 { 
                var v = Vector2 {
                    x = 12;
                };
            };
";
            var err = Assert.ThrowsException<InitializationError>(() => Compile(source));
            Assert.IsNotNull(err.UninitializedFields);
            Assert.AreEqual(err.UninitializedFields.Count, 1);
            Assert.AreEqual(err.UninitializedFields[0], "y");
        }

        [TestMethod]
        public void UnknownMemberInitialized()
        {
            string source = @"
            const Vector2 = struct {
                x: i32;
                y: i32;
            };

            const foo = proc() -> i32 { 
                var v = Vector2 {
                    x = 12;
                    z = 34;
                };
            };
";
            var err = Assert.ThrowsException<InitializationError>(() => Compile(source));
            Assert.IsNotNull(err.UnknownField);
            Assert.AreEqual(err.UnknownField.Value.Value, "z");
        }

        [TestMethod]
        public void InitializedInOrder()
        {
            string source = @"
            const Vector2 = struct {
                x: i32;
                y: i32;
            };

            const foo = proc() -> i32 { 
                var v = Vector2 {
                    x = 12;
                    y = 34;
                };
                v.y
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 34);
        }

        [TestMethod]
        public void InitializedOutOfOrder()
        {
            string source = @"
            const Vector2 = struct {
                x: i32;
                y: i32;
            };

            const foo = proc() -> i32 { 
                var v = Vector2 {
                    y = 53;
                    x = 72;
                };
                v.x
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 72);
        }

        [TestMethod]
        public void ReassignMembers()
        {
            string source = @"
            const Vector2 = struct {
                x: i32;
                y: i32;
            };

            const foo = proc() -> i32 { 
                var v = Vector2 {
                    y = 65;
                    x = 16;
                };
                v.x = 87;
                v.y = 13;
                v.x
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 87);
        }

        [TestMethod]
        public void InitializedInOrderComptime()
        {
            string source = @"
            const Vector2 = struct {
                x: i32;
                y: i32;
            };
            const Value = {
                var v = Vector2 {
                    x = 12;
                    y = 34;
                };
                v.y
            };
            const foo = proc() -> i32 { 
                Value
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 34);
        }

        [TestMethod]
        public void InitializedOutOfOrderComptime()
        {
            string source = @"
            const Vector2 = struct {
                x: i32;
                y: i32;
            };
            const Value = {
                var v = Vector2 {
                    y = 53;
                    x = 72;
                };
                v.x
            };
            const foo = proc() -> i32 { 
                Value
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 72);
        }

        [TestMethod]
        public void ReassignMembersComptime()
        {
            string source = @"
            const Vector2 = struct {
                x: i32;
                y: i32;
            };
            const Value = {
                var v = Vector2 {
                    y = 65;
                    x = 16;
                };
                v.x = 87;
                v.y = 13;
                v.x
            };
            const foo = proc() -> i32 { 
                Value
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 87);
        }

        [TestMethod]
        public void MaterializeConstantAtRuntime()
        {
            string source = @"
            const Vector2 = struct {
                x: i32;
                y: i32;
            };
            const Value = Vector2 { x = 45; y = 71; };
            const foo = proc() -> i32 {
                var x = Value;
                x.x
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 45);
        }

        [TestMethod]
        public void MaterializeConstantAtRuntimeInline()
        {
            string source = @"
            const Vector2 = struct {
                x: i32;
                y: i32;
            };
            const Value = Vector2 { x = 45; y = 71; };
            const foo = proc() -> i32 {
                Value.x
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 45);
        }

        [TestMethod]
        public void ReassignCopyDoesNotChangeOriginal()
        {
            string source = @"
            const Vector2 = struct {
                x: i32;
                y: i32;
            };
            const foo = proc() -> i32 {
                var v = Vector2{ x = 35; y = 16; };
                var v2 = v;
                v2.x = 75;
                v2.y = 47;
                v.x
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 35);
        }

        [TestMethod]
        public void ReassignCopyDoesNotChangeOriginalComptime()
        {
            string source = @"
            const Vector2 = struct {
                x: i32;
                y: i32;
            };
            const Value = {
                var v = Vector2{ x = 35; y = 16; };
                var v2 = v;
                v2.x = 75;
                v2.y = 47;
                v.x
            };
            const foo = proc() -> i32 {
                Value
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 35);
        }

        [TestMethod]
        public void NestedStruct()
        {
            string source = @"
            const Outer = struct {
                m: Inner;
            };
            const Inner = struct {
                n: i32;
            };
            const foo = proc() -> i32 { 
                var v = Outer {
                    m = Inner {
                        n = 10;
                    };
                };
                v.m.n
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 10);
        }

        [TestMethod]
        public void NestedStructReassignInner()
        {
            string source = @"
            const Outer = struct {
                m: Inner;
            };
            const Inner = struct {
                n: i32;
            };
            const foo = proc() -> i32 { 
                var v = Outer {
                    m = Inner {
                        n = 10;
                    };
                };
                v.m.n = 15;
                v.m.n
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 15);
        }

        [TestMethod]
        public void NestedStructReassignInnerComptime()
        {
            string source = @"
            const Outer = struct {
                m: Inner;
            };
            const Inner = struct {
                n: i32;
            };
            const Value = {
                var v = Outer {
                    m = Inner {
                        n = 10;
                    };
                };
                v.m.n = 15;
                v.m.n
            };
            const foo = proc() -> i32 { 
                Value
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 15);
        }

        [TestMethod]
        public void NestedStructCopyInnerDoesNotAffectOriginal()
        {
            string source = @"
            const Outer = struct {
                m: Inner;
            };
            const Inner = struct {
                n: i32;
            };
            const foo = proc() -> i32 { 
                var v = Outer {
                    m = Inner {
                        n = 42;
                    };
                };
                var inner = v.m;
                inner.n = 63;
                v.m.n
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 42);
        }

        [TestMethod]
        public void NestedStructCopyInnerDoesNotAffectOriginalComptime()
        {
            string source = @"
            const Outer = struct {
                m: Inner;
            };
            const Inner = struct {
                n: i32;
            };
            const Value = {
                var v = Outer {
                    m = Inner {
                        n = 42;
                    };
                };
                var inner = v.m;
                inner.n = 63;
                v.m.n
            };
            const foo = proc() -> i32 { 
                Value
            };
";
            var f = CompileAndLoadFunc<Func<Int32>>(source);
            Assert.AreEqual(f(), 42);
        }
    }

    // TODO: Test generics
}
