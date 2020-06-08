using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.Ast;
using Yoakke.Utils;

namespace Yoakke.Semantic
{
    /// <summary>
    /// Does a constant evaluation and assingment on the AST, so every constant's <see cref="Symbol"/> 
    /// is assigned a compile-time <see cref="Value"/>.
    /// </summary>
    static class AssignConstantSymbol
    {
        /// <summary>
        /// Assigns constants inside a given <see cref="Statement"/>.
        /// </summary>
        /// <param name="statement">The <see cref="Statement"/> to assign the constants in.</param>
        public static void Assign(Statement statement)
        {
            switch (statement)
            {
            case Declaration.Program program:
                foreach (var decl in program.Declarations) Assign(decl);
                break;

            case Declaration.ConstDef constDef:
                if (constDef.Type != null) Assign(constDef.Type);
                Assign(constDef.Value);
                // We assign the compile-time value to the symbol
                Assert.NonNull(constDef.Symbol);
                if (constDef.Symbol.Value == null)
                {
                    constDef.Symbol.Value = EvaluateConst.Evaluate(constDef.Value);
                }
                break;

            case Statement.Expression_ expression:
                Assign(expression.Expression);
                break;

            default: throw new NotImplementedException();
            }
        }

        private static void Assign(Expression expression)
        {
            switch (expression)
            {
            // Nothing to do, leaf nodes
            case Expression.IntLit _:
            case Expression.StrLit _:
            case Expression.Ident _:
            case Expression.Intrinsic _:
                break;

            case Expression.ProcType procType:
                foreach (var param in procType.ParameterTypes) Assign(param);
                if (procType.ReturnType != null) Assign(procType.ReturnType);
                break;

            case Expression.Proc proc:
                foreach (var param in proc.Parameters) Assign(param.Type);
                if (proc.ReturnType != null) Assign(proc.ReturnType);
                Assign(proc.Body);
                break;

            case Expression.Block block:
                foreach (var stmt in block.Statements) Assign(stmt);
                if (block.Value != null) Assign(block.Value);
                break;

            case Expression.Call call:
                Assign(call.Proc);
                foreach (var arg in call.Arguments) Assign(arg);
                break;

            default: throw new NotImplementedException();
            }
        }
    }
}
