using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Xml.Schema;
using Yoakke.Ast;
using Yoakke.Utils;

namespace Yoakke.Semantic
{
    /// <summary>
    /// Does constant-evaluation at compile-time.
    /// </summary>
    static class ConstEval
    {
        /// <summary>
        /// Evaluates the given <see cref="Statement"/> at compile-time.
        /// </summary>
        /// <param name="statement">The <see cref="Statement"/> to evaluate.</param>
        public static void Evaluate(Statement statement)
        {
            var callStk = new Stack<StackFrame>();
            callStk.Push(new StackFrame());
            Evaluate(callStk, statement);
        }

        /// <summary>
        /// Evaluates the given <see cref="Expression"/> at compile-time.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to evaluate.</param>
        /// <returns>The compile-time <see cref="Value"/> of the <see cref="Expression"/>.</returns>
        public static Value Evaluate(Expression expression)
        {
            var callStk = new Stack<StackFrame>();
            callStk.Push(new StackFrame());
            return Evaluate(callStk, expression);
        }

        /// <summary>
        /// Evaluates the given <see cref="Expression"/> at compile-time as a <see cref="Type"/>.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to evaluate.</param>
        /// <returns>The <see cref="Type"/> the <see cref="Expression"/> describes.</returns>
        public static Type EvaluateAsType(Expression expression)
        {
            var callStk = new Stack<StackFrame>();
            callStk.Push(new StackFrame());
            return EvaluateAsType(callStk, expression);
        }

        private class StackFrame
        {
            public Dictionary<Symbol.Variable, Value> Variables = new Dictionary<Symbol.Variable, Value>();
        }

        private static Value Evaluate(Stack<StackFrame> callStack, Expression expression)
        {
            // Some simple cache-ing mechanism
            if (expression.ConstantValue == null)
            {
                expression.ConstantValue = EvaluateImpl(callStack, expression);
            }
            return expression.ConstantValue;
        }

        private static Type EvaluateAsType(Stack<StackFrame> callStack, Expression expression)
        {
            var value = Evaluate(callStack, expression);
            value.Type.Unify(Type.Type_);
            return (Type)value;
        }

        private static void Evaluate(Stack<StackFrame> callStack, Statement statement)
        {
            switch (statement)
            {
            case Declaration.Program program:
                foreach (var decl in program.Declarations) Evaluate(callStack, decl);
                break;

            case Declaration.ConstDef constDef:
                Assert.NonNull(constDef.Symbol);
                // Evaluate the type, if there's any
                if (constDef.Type != null) EvaluateAsType(callStack, constDef.Type);
                // Evaluate the value
                if (constDef.Symbol.Value == null)
                {
                    // This is still unevaluated, evaluate and assign to symbol
                    constDef.Symbol.Value = Evaluate(callStack, constDef.Value);
                }
                // If has a type, unify with value
                if (constDef.Type != null)
                {
                    Assert.NonNull(constDef.Type.ConstantValue);
                    Assert.NonNull(constDef.Value.ConstantValue);

                    var type = (Type)constDef.Type.ConstantValue;
                    type.Unify(constDef.Value.ConstantValue.Type);
                }

                // TODO: This is just for debugging
                if (constDef.Type == null)
                {
                    Console.WriteLine($"const {constDef.Name.Value} = {constDef.Value.ConstantValue}");
                    Assert.NonNull(constDef.Value.ConstantValue);
                    Console.WriteLine($"  deduced type: {constDef.Value.ConstantValue.Type}");
                }
                else
                {
                    Console.WriteLine($"const {constDef.Name.Value}: {constDef.Type.ConstantValue} = {constDef.Value.ConstantValue}");
                    Assert.NonNull(constDef.Value.ConstantValue);
                    Console.WriteLine($"  deduced type: {constDef.Value.ConstantValue.Type}");
                }
                break;

            case Statement.Expression_ expression:
            {
                var value = Evaluate(callStack, expression.Expression);
                value.Type.Unify(Type.Unit);
            }
            break;

            default: throw new NotImplementedException();
            }
        }

        private static Value EvaluateImpl(Stack<StackFrame> callStack, Expression expression)
        {
            switch (expression)
            {
            case Expression.IntLit intLit: 
                // TODO: It should be a generic integer type!
                return new Value.Int(Type.I32, BigInteger.Parse(intLit.Token.Value));

            case Expression.StrLit strLit:
                return new Value.Str(strLit.Escape());

            case Expression.Intrinsic intrinsic:
                Assert.NonNull(intrinsic.Symbol);
                return new Value.IntrinsicProc(intrinsic.Symbol);

            case Expression.Ident ident:
                Assert.NonNull(ident.Symbol);
                // Depends on the symbol
                switch (ident.Symbol)
                {
                case Symbol.Const constSym:
                {
                    // Symbol already has a value, return that
                    if (constSym.Value != null) return constSym.Value;
                    // No value, must be evaluatable
                    Assert.NonNull(constSym.Definition);
                    // NOTE: We don't pass the call-stack on purpose. Constants shouldn't be related.
                    Evaluate(constSym.Definition);
                    Assert.NonNull(constSym.Value);
                    return constSym.Value;
                }

                // Simply wrap
                case Symbol.Intrinsic intrinsicSym: return new Value.IntrinsicProc(intrinsicSym);

                // Simply look up the local
                case Symbol.Variable varSym: return callStack.Peek().Variables[varSym];

                default: throw new NotImplementedException();
                }

            case Expression.ProcType procType:
            {
                // Evaluate parameters
                var parameters = procType.ParameterTypes.Select(x => EvaluateAsType(callStack, x)).ToList();
                // Evaluate return type, if any
                var ret = procType.ReturnType == null ? Type.Unit : EvaluateAsType(callStack, procType.ReturnType);
                // Create the procedure type
                return new Type.Proc(parameters, ret);
            }

            case Expression.Proc proc:
            {
                // Evaluate parameters
                var parameters = proc.Parameters.Select(x => EvaluateAsType(callStack, x.Type)).ToList();
                // Evaluate return type, if any
                var ret = proc.ReturnType == null ? Type.Unit : EvaluateAsType(callStack, proc.ReturnType);
                // TODO: Type-check body, unify with return-type
                // Create the procedure type
                var procType = new Type.Proc(parameters, ret);
                // Wrap it up in a value
                return new Value.Proc(proc, procType);
            }

            case Expression.Block block:
            {
                foreach (var stmt in block.Statements) Evaluate(callStack, stmt);
                return block.Value == null ? Value.Unit : Evaluate(callStack, block.Value);
            }

            case Expression.Call call:
            {
                // Evaluate the procedure and the arguments
                var proc = Evaluate(callStack, call.Proc);
                var args = call.Arguments.Select(x => Evaluate(callStack, x)).ToList();

                if (proc is Value.Proc procValue)
                {
                    // Create a type from the invocation
                    var argTypes = args.Select(x => x.Type).ToList();
                    // The return type is deduced with a type valiable
                    var retType = new Type.Variable();
                    // The call-site type
                    var callSiteProcType = new Type.Proc(argTypes, retType);
                    // Unify with the procedure type itself
                    procValue.Type.Unify(callSiteProcType);
                    // Now actually call the procedure
                    callStack.Push(new StackFrame());
                    // Define arguments
                    foreach (var arg in args.Zip(procValue.Node.Parameters.Select(x => x.Symbol)))
                    {
                        Assert.NonNull(arg.Second);
                        callStack.Peek().Variables.Add(arg.Second, arg.First);
                    }
                    // Evaluate the body
                    var value = Evaluate(callStack, procValue.Node.Body);
                    callStack.Pop();
                    return value;
                }
                else if (proc is Value.IntrinsicProc intrinsic)
                {
                    var argTypes = args.Select(x => x.Type).ToList();
                    // For now we ignore the return type on purpose
                    var retType = new Type.Variable();
                    // The call-site type
                    var callSiteProcType = new Type.Proc(argTypes, retType);
                    // Unify with the procedure type itself
                    intrinsic.Type.Unify(callSiteProcType);
                    // Call it
                    return intrinsic.Symbol.Function(args.ToList());
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            default: throw new NotImplementedException();
            }
        }
    }
}
