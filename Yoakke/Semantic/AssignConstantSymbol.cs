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
            case ProgramDeclaration program:
                foreach (var decl in program.Declarations) Assign(decl);
                break;

            case ConstDefinition constDef:
                Assert.NonNull(constDef.Symbol);
                if (constDef.Symbol.Value == null)
                {
                    constDef.Symbol.Value = EvaluateConst.Evaluate(constDef.Value);
                }
                break;

            case ExpressionStatement expression:
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
            case IntLiteralExpression intLit:
            case IdentifierExpression ident:
                break;

            case ProcExpression proc:
                foreach (var param in proc.Parameters) Assign(param.Type);
                if (proc.ReturnType != null) Assign(proc.ReturnType);
                Assign(proc.Body);
                break;

            case BlockExpression block:
                foreach (var stmt in block.Statements) Assign(stmt);
                if (block.Value != null) Assign(block.Value);
                break;

            default: throw new NotImplementedException();
            }
        }
    }
}
