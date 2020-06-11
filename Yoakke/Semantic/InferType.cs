using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.Ast;
using Yoakke.Utils;

namespace Yoakke.Semantic
{
    /// <summary>
    /// Infers all the <see cref="Type"/>s to their proper form, while enforcing static rules,
    /// like matching return values and return types.
    /// </summary>
    static class InferType
    {
        /// <summary>
        /// Infers <see cref="Type"/>s inside the given <see cref="Statement"/>.
        /// </summary>
        /// <param name="statement">The <see cref="Statement"/> to infer <see cref="Type"/>s in.</param>
        public static void Infer(Statement statement)
        {
            switch (statement)
            {
            case Declaration.Program program:
                foreach (var decl in program.Declarations) Infer(decl);
                break;

            case Declaration.ConstDef constDef:
                if (constDef.Type != null) Infer(constDef.Type);
                Infer(constDef.Value);
                if (constDef.Type != null)
                {
                    // Unify the defined type with the value type
                    Assert.NonNull(constDef.Value.EvaluationType);
                    var ty = EvaluateConst.EvaluateToType(constDef.Type);
                    //Type.Unify(constDef.Value.EvaluationType, ty);
                }
                break;

            case Statement.Expression_ expression:
                Infer(expression.Expression);
                break;

            default: throw new NotImplementedException();
            }
        }

        private static void Infer(Expression expression)
        {
            switch (expression)
            {
            case Expression.IntLit _:
            case Expression.StrLit _:
            case Expression.Ident _:
            case Expression.Intrinsic _:
                break;

            case Expression.ProcType procType:
                foreach (var param in procType.ParameterTypes) Infer(param);
                if (procType.ReturnType != null) Infer(procType.ReturnType);
                break;

            case Expression.Proc proc:
                foreach (var param in proc.Parameters) Infer(param.Type);
                if (proc.ReturnType != null) Infer(proc.ReturnType);
                Infer(proc.Body);
                var returnType = proc.ReturnType == null 
                                ? Type.Unit
                                : EvaluateConst.EvaluateToType(proc.ReturnType);
                Assert.NonNull(proc.Body.EvaluationType);
                //Type.Unify(proc.Body.EvaluationType, returnType);
                break;

            case Expression.Block block:
                foreach (var stmt in block.Statements) Infer(stmt);
                if (block.Value != null) Infer(block.Value);
                break;

            case Expression.Call call:
                // TODO: Make sure it's a procedure?
                Infer(call.Proc);
                foreach (var arg in call.Arguments) Infer(arg);
                break;

            default: throw new NotImplementedException();
            }
        }
    }
}
