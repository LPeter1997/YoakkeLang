using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Yoakke.Ast;
using Yoakke.IR;
using Yoakke.Utils;

namespace Yoakke.Semantic
{
    /// <summary>
    /// Enforces type-checking rules in the program.
    /// </summary>
    static class TypeCheck
    {
        /// <summary>
        /// Type-checks the given <see cref="Statement"/>.
        /// </summary>
        /// <param name="statement">The <see cref="Statement"/> to type-check.</param>
        public static void Check(Statement statement)
        {
            switch (statement)
            {
            case Declaration.Program program:
                foreach (var decl in program.Declarations) Check(decl);
                break;

            case Declaration.ConstDef constDef:
                // Check subelements
                if (constDef.Type != null) Check(constDef.Type);
                Check(constDef.Value);
                // Let constant evaluation do the work here
                ConstEval.Evaluate(constDef);
                break;

            case Statement.Expression_ expression:
            {
                // Check subelement
                Check(expression.Expression);
                // We force evaluation here to ensure checks
                var ty = TypeEval.Evaluate(expression.Expression);
                // An expression in a statement's position must produce a unit type, if it's not terminated by a semicolon
                if (!expression.HasSemicolon) Type.Unit.Unify(ty);
            }
            break;

            default: throw new NotImplementedException();
            }
        }

        private static void Check(Expression expression)
        {
            switch (expression)
            {
            // Nothing to do, leaf elements
            case Expression.IntLit _:
            case Expression.StrLit _:
            case Expression.Ident _:
            case Expression.Intrinsic _:
                break;

            case Expression.ProcType procType:
                // Just check subelements
                foreach (var param in procType.ParameterTypes) Check(param);
                if (procType.ReturnType != null) Check(procType.ReturnType);
                break;

            case Expression.Proc proc:
            {
                // Check argument and return type
                foreach (var param in proc.Parameters) Check(param.Type);
                if (proc.ReturnType != null) Check(proc.ReturnType);
                // Before checking the body, we assign each parameter symbol it's proper type
                foreach (var param in proc.Parameters)
                {
                    Assert.NonNull(param.Symbol);
                    Debug.Assert(param.Symbol.Type == null);
                    param.Symbol.Type = ConstEval.EvaluateAsType(param.Type);
                }
                // Now we can check the body
                Check(proc.Body);
                // Unify the return type with the body's return type
                var procRetTy = proc.ReturnType == null ? Type.Unit : ConstEval.EvaluateAsType(proc.ReturnType);
                var bodyRetTy = TypeEval.Evaluate(proc.Body);
                procRetTy.Unify(bodyRetTy);
            }
            break;

            case Expression.Block block:
                // Just check subelements
                foreach (var stmt in block.Statements) Check(stmt);
                if (block.Value != null) Check(block.Value);
                break;

            case Expression.Call call:
                // Just check subelements
                Check(call.Proc);
                foreach (var arg in call.Arguments) Check(arg);
                break;

            default: throw new NotImplementedException();
            }
        }
    }
}
