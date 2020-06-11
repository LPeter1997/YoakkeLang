using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.Ast;

namespace Yoakke.Semantic
{
    /// <summary>
    /// A check to assign implicit returns to their corresponding <see cref="Scope"/>s.
    /// </summary>
    static class AssignReturnTarget
    {
        /// <summary>
        /// Assigns the block return targets to their corresponding <see cref="Scope"/>s.
        /// </summary>
        /// <param name="statement">The <see cref="Statement"/> to assign in.</param>
        public static void Assign(Statement statement)
        {
            Assign(statement, null);
        }

        private static void Assign(Statement statement, Scope? currentTarget)
        {
            switch (statement)
            {
            case Declaration.Program program:
                // Just loop through every declaration
                foreach (var decl in program.Declarations) Assign(decl, currentTarget);
                break;

            case Declaration.ConstDef constDef:
                // The type is a return target too, assign the scope
                if (constDef.Type != null) Assign(constDef.Type, constDef.Type.Scope);
                // Value is a return target
                Assign(constDef.Value, constDef.Value.Scope);
                break;

            case Statement.Expression_ expression:
                // Assign in the expression
                Assign(expression.Expression, expression.Scope);
                // If the expression is a block and the return scope has been defined, it's an error
                if (   expression.Expression is Expression.Block block 
                    && block.ReturnTarget == expression.Scope)
                {
                    throw new Exception("Implicit return in the middle!");
                }
                break;

            default: throw new NotImplementedException();
            }
        }

        private static void Assign(Expression expression, Scope? currentTarget)
        {
            switch (expression)
            {
            case Expression.IntLit _:
            case Expression.StrLit _:
            case Expression.Ident _:
            case Expression.Intrinsic _:
                // Nothing to do
                break;

            case Expression.ProcType procType:
                // Assign in parameter types
                foreach (var param in procType.ParameterTypes) Assign(param, param.Scope);
                // Assign in return type, if needed
                if (procType.ReturnType != null) Assign(procType.ReturnType, procType.ReturnType.Scope);
                break;

            case Expression.Proc proc:
                // Assign in parameters
                foreach (var param in proc.Parameters) Assign(param.Type, param.Type.Scope);
                // Assign in return-type
                if (proc.ReturnType != null) Assign(proc.ReturnType, proc.ReturnType.Scope);
                // Assign in body, but body has the return target of the function's body
                Assign(proc.Body, proc.Body.Scope);
                break;

            case Expression.Block block:
                // Assign in each statement
                foreach (var stmt in block.Statements) Assign(stmt, currentTarget);
                // In return value too
                if (block.Value != null)
                {
                    Assign(block.Value, currentTarget);
                    // Also, we have a return target for the value
                    block.ReturnTarget = currentTarget;
                }
                break;

            case Expression.Call call:
                // Assign in called procedure
                Assign(call.Proc, call.Proc.Scope);
                // Assign in arguments
                foreach (var arg in call.Arguments) Assign(arg, arg.Scope);
                break;

            default: throw new NotImplementedException();
            }
        }
    }
}
