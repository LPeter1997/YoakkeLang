using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Yoakke.Ast;
using Yoakke.Utils;

namespace Yoakke.Semantic
{
    /// <summary>
    /// Does type-evaluation at compile-time.
    /// </summary>
    static class TypeEval
    {
        /// <summary>
        /// Evaluates the <see cref="Type"/> of a given <see cref="Expression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to get the <see cref="Type"/> of.</param>
        /// <returns>The <see cref="Type"/> of the given <see cref="Expression"/>.</returns>
        public static Type Evaluate(Expression expression)
        {
            if (expression.EvaluationType == null)
            {
                // Simple cache-ing
                expression.EvaluationType = EvaluateImpl(expression);
            }
            return expression.EvaluationType;
        }

        private static Type EvaluateImpl(Expression expression)
        {
            switch (expression)
            {
            // TODO: Some generic integer type instead!
            case Expression.IntLit _: return Type.I32;
            case Expression.BoolLit _: return Type.Bool;
            case Expression.StrLit _: return Type.Str;

            // We know these are just types
            case Expression.StructType _:
            case Expression.ProcType _: 
                return Type.Type_;

            case Expression.StructValue structValue:
                return ConstEval.EvaluateAsType(structValue.StructType);

            case Expression.Proc proc: 
                return ConstEval.Evaluate(proc).Type;

            case Expression.Intrinsic intrinsic:
                Assert.NonNull(intrinsic.Symbol);
                return intrinsic.Symbol.Type;

            case Expression.Ident ident:
                Assert.NonNull(ident.Symbol);
                // Depends on the symbol
                switch (ident.Symbol)
                {
                case Symbol.Const constSym: return constSym.GetValue().Type;

                case Symbol.Intrinsic intrinsicSym: return intrinsicSym.Type;

                case Symbol.Variable varSym:
                    Assert.NonNull(varSym.Type);
                    return varSym.Type;

                default: throw new NotImplementedException();
                }

            case Expression.Block block:
                return block.Value == null ? Type.Unit : Evaluate(block.Value);

            case Expression.Call call:
            {
                // Evaluate the procedure type
                var procType = Evaluate(call.Proc);
                // Evaluate the arguments types
                var argTypes = call.Arguments.Select(x => Evaluate(x)).ToList();
                // We create a type variable for the return type
                var retType = new Type.Variable();
                // We craft the call-site procedure type
                var callSiteProcType = new Type.Proc(argTypes, retType);
                // If we unify that with the actual procedure type, our type variable will be substituted for the return type
                procType.Unify(callSiteProcType);
                return retType;
            }

            case Expression.If iff:
            {
                // Condition must be a boolean
                var condType = Evaluate(iff.Condition);
                Type.Bool.Unify(condType);
                // Then and else must yield the same type
                var thenType = Evaluate(iff.Then);
                if (iff.Else != null)
                {
                    var elseType = Evaluate(iff.Else);
                    thenType.Unify(elseType);
                }
                return thenType;
            }

            default: throw new NotImplementedException();
            }
        }
    }
}
