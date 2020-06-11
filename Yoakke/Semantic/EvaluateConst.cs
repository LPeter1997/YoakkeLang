using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Yoakke.Ast;
using Yoakke.Utils;

namespace Yoakke.Semantic
{
    /// <summary>
    /// Does constant-evaluation at compile-time.
    /// </summary>
    static class EvaluateConst
    {
        /// <summary>
        /// Evaluates the given <see cref="Expression"/> at compile-time.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to evaluate.</param>
        /// <returns>The compile-time <see cref="Value"/> of the <see cref="Expression"/>.</returns>
        public static Value Evaluate(Expression expression)
        {
            if (expression.ConstantValue == null)
            {
                // TODO: Re-consider where we can even do constant-cache -ing!
                // Is there a case where mutability can affect us and we need to invalidate the cache?
                expression.ConstantValue = EvaluateInternal(expression);
            }
            return expression.ConstantValue;
        }

        /// <summary>
        /// Evaluates the given <see cref="Expression"/> at compile-time as a <see cref="Type"/>.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to evaluate.</param>
        /// <returns>The <see cref="Type"/> the <see cref="Expression"/> describes.</returns>
        public static Type EvaluateToType(Expression expression)
        {
            /*var val = Evaluate(expression);
            Type.Unify(val.Type, Type.Type_);
            return ((Value.Type_)val).Value;*/
            throw new NotImplementedException();
        }

        private static Value EvaluateInternal(Expression expression)
        {
            switch (expression)
            {
            case Expression.IntLit intLit:
                // TODO: The exact integer type should not be determined here!
                // It should go through the inference-process, starting from a generic int value!
                return new Value.Int(Type.I32, BigInteger.Parse(intLit.Token.Value));

            case Expression.StrLit strLit:
                return new Value.Str(strLit.Escape());

            case Expression.Ident ident:
            {
                Assert.NonNull(ident.Symbol);
                // We are in constant evaluation, the symbol must be a constant
                if (ident.Symbol is Symbol.Const symbol)
                {
                    if (symbol.Value == null)
                    {
                        // If value is null, it hasn't been calculated yet
                        Assert.NonNull(symbol.Definition);
                        //symbol.Value = Evaluate(symbol.Definition.Value);
                        throw new NotImplementedException();
                    }
                    // Value is surely calculated here
                    return symbol.Value;
                }
                throw new NotImplementedException("Non-constant symbol referenced in a constant expression!");
            }

            case Expression.Intrinsic intrinsic:
                // Simply wrap the corresponding symbol
                Assert.NonNull(intrinsic.Symbol);
                //return new Value.IntrinsicProc(intrinsic.Symbol);
                throw new NotImplementedException();

            case Expression.ProcType procType:
            {
                // Evaluate types
                var paramTypes = procType.ParameterTypes.Select(EvaluateToType).ToList();
                var returnType = procType.ReturnType == null ? Type.Unit : EvaluateToType(procType.ReturnType);
                // Create a procedure type
                //var type = Type.Procedure(paramTypes, returnType);
                // Wrap it in a value
                //return new Value.Type_(type);
                throw new NotImplementedException();
            }

            case Expression.Proc proc:
            {
                // Evaluate types
                var paramTypes = proc.Parameters.Select(x => EvaluateToType(x.Type)).ToList();
                var returnType = proc.ReturnType == null ? Type.Unit : EvaluateToType(proc.ReturnType);
                // Create a procedure type
                //var procType = Type.Procedure(paramTypes, returnType);
                // Wrap the node into a value
                //return new Value.Proc(proc, procType);
                throw new NotImplementedException();
            }

            // TODO: Evaluate statements, block
            case Expression.Block block: throw new NotImplementedException();

            case Expression.Call call:
            {
                var proc = Evaluate(call.Proc);
                var args = call.Arguments.Select(Evaluate).ToList();
                if (proc is Value.IntrinsicProc intrinsic)
                {
                    // Just call it
                    var func = intrinsic.Symbol.Function;
                    return func(args);
                }
                else
                {
                    // TODO
                    throw new NotImplementedException();
                }
            }

            default: throw new NotImplementedException();
            }
        }
    }
}
