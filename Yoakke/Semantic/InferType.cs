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
            case ProgramDeclaration program:
                foreach (var decl in program.Declarations) Infer(decl);
                break;

            case ConstDefinition constDef:
                Infer(constDef.Value);
                if (constDef.Type != null) Infer(constDef.Type);
                break;

            case ExpressionStatement expression:
                Infer(expression.Expression);
                break;

            default: throw new NotImplementedException();
            }
        }

        private static void Infer(Expression expression)
        {
            switch (expression)
            {
            case IntLiteralExpression intLiteral:
            case IdentifierExpression identifier:
                break;

            case ProcExpression proc:
                foreach (var param in proc.Parameters) Infer(param.Type);
                if (proc.ReturnType != null) Infer(proc.ReturnType);
                Infer(proc.Body);
                var returnType = proc.ReturnType == null 
                                ? Type.Unit
                                : EvaluateConst.EvaluateToType(proc.ReturnType);
                Assert.NonNull(proc.Body.EvaluationType);
                Unifier.Unify(proc.Body.EvaluationType, returnType);
                break;

            case BlockExpression block:
                foreach (var stmt in block.Statements) Infer(stmt);
                if (block.Value != null) Infer(block.Value);
                break;

            default: throw new NotImplementedException();
            }
        }
    }
}
