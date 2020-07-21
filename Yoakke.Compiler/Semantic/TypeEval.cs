using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Yoakke.Compiler.Ast;
using Yoakke.Compiler.Syntax;
using Yoakke.Compiler.Utils;

namespace Yoakke.Compiler.Semantic
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
                // TODO: Can we cache here? Isn't it like in consteval?
                // Simple cache-ing
                expression.EvaluationType = EvaluateImpl(expression).Substitution;
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
            case Expression.VarType _:
            case Expression.StructType _:
            case Expression.ProcSignature _: 
                return Type.Type_;

            case Expression.StructValue structValue:
                return ConstEval.EvaluateAsType(structValue.StructType);

            case Expression.ProcValue proc: 
                return ConstEval.Evaluate(proc).Type;

            case Expression.Ident ident:
                Assert.NonNull(ident.Symbol);
                // Depends on the symbol
                switch (ident.Symbol)
                {
                case Symbol.Const constSym: return constSym.GetValue().Type;
                case Symbol.Variable varSym: return varSym.Type;

                default: throw new NotImplementedException();
                }

            case Expression.DotPath dotPath:
            {
                var leftType = Evaluate(dotPath.Left).Substitution;
                if (leftType is Type.Struct structType)
                {
                    // Field access
                    if (!structType.Fields.TryGetValue(dotPath.Right.Value, out var fieldType))
                    {
                        // TODO
                        throw new NotImplementedException("No such field of struct!");
                    }
                    return fieldType;
                }
                else if (leftType.Equals(Type.Type_))
                {
                    // Constant access
                    var leftValue = ConstEval.EvaluateAsType(dotPath.Left);
                    if (leftValue is Type.Struct structTy)
                    {
                        // We have a chance of accessing a constant here

                        // TODO: We need to somehow restrict the scope in this case
                        // For example:
                        // const A = struct {};
                        // const B = struct {};
                        // ...
                        // B.A should NOT work, but probably does now, since the scope-tree walks up into global space.

                        var symbol = (Symbol.Const)structTy.Scope.Reference(dotPath.Right.Value);
                        return symbol.GetValue().Type;
                    }
                    else
                    {
                        // TODO
                        throw new NotImplementedException("Not a struct type on the left-hand-side of dot!");
                    }
                }
                else
                {
                    // TODO
                    throw new NotImplementedException("Not a struct type on the left-hand-side of dot!");
                }
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
                procType.UnifyWith(callSiteProcType);
                return retType;
            }

            case Expression.If iff:
            {
                // Condition must be a boolean
                var condType = Evaluate(iff.Condition);
                Type.Bool.UnifyWith(condType);
                // Then and else must yield the same type
                var thenType = Evaluate(iff.Then);
                if (iff.Else != null)
                {
                    var elseType = Evaluate(iff.Else);
                    thenType.UnifyWith(elseType);
                }
                else
                {
                    // No else, must not return anything
                    Type.Unit.UnifyWith(thenType);
                }
                return thenType;
            }

            case Expression.BinOp binOp:
                if (binOp.Operator.Type == TokenType.Assign)
                {
                    // The two types need to match to assign
                    var leftType = Evaluate(binOp.Left);
                    var rightType = Evaluate(binOp.Right);
                    leftType.UnifyWith(rightType);
                    return leftType;
                }
                else
                {
                    // It's some binary operator that's potentially overloaded

                    // TODO: For now we hard-code some of these for i32
                    var leftType = Evaluate(binOp.Left);
                    var rightType = Evaluate(binOp.Right);
                    Type.I32.UnifyWith(leftType);
                    Type.I32.UnifyWith(rightType);

                    if (binOp.Operator.Type == TokenType.Add)
                    {
                        return Type.I32;
                    }
                    else if (binOp.Operator.Type == TokenType.Less)
                    {
                        return Type.Bool;
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
