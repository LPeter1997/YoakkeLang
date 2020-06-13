using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Yoakke.Ast;
using Yoakke.IR;
using Yoakke.Utils;

namespace Yoakke.Semantic
{
    // TODO: Doc
    static class TypeCheck
    {
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
                // TODO: Except when we explicitly put a semicolon!
                // An expression in a statement's position must produce a unit type
                Type.Unit.Unify(ty);
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
