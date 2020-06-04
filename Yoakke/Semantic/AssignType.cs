using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.Ast;
using Yoakke.Utils;

namespace Yoakke.Semantic
{
    /// <summary>
    /// A pass that makes sure that every <see cref="Expression"/> is correctly <see cref="Type"/>d.
    /// </summary>
    static class AssignType
    {
        /// <summary>
        /// Assigns <see cref="Type"/>s to <see cref="Expression"/>s inside the given <see cref="Statement"/>s.
        /// </summary>
        /// <param name="statement">The <see cref="Statement"/> to assign <see cref="Type"/>s in.</param>
        public static void Assign(Statement statement)
        {
            switch (statement)
            {
            case ProgramDeclaration program:
                foreach (var decl in program.Declarations) Assign(decl);
                break;

            case ConstDefinition constDef:
                Assign(constDef.Value);
                break;

            case ExpressionStatement expression:
                Assign(expression.Expression);
                break;

            default: throw new NotImplementedException();
            }
        }

        private static void Assign(Expression expression)
        {
            if (expression.EvaluationType != null) return;
            expression.EvaluationType = AssignInternal(expression);
        }

        private static Type AssignInternal(Expression expression)
        {
            switch (expression)
            {
            case IntLiteralExpression intLit:
                // TODO: This should have an abstract integral type instead!
                return EvaluateConst.Evaluate(intLit).Type;

            case IdentifierExpression ident:
                Assert.NonNull(ident.Symbol);
                if (ident.Symbol is ConstSymbol constSym)
                {
                    Assert.NonNull(constSym.Value);
                    return constSym.Value.Type;
                }
                else
                {
                    throw new NotImplementedException();
                }

            case ProcExpression proc:
                foreach (var param in proc.Parameters) Assign(param.Type);
                if (proc.ReturnType != null) Assign(proc.ReturnType);
                Assign(proc.Body);
                return EvaluateConst.Evaluate(proc).Type;

            case BlockExpression block:
                foreach (var stmt in block.Statements) Assign(stmt);
                if (block.Value != null)
                {
                    Assign(block.Value);
                    Assert.NonNull(block.Value.EvaluationType);
                    return block.Value.EvaluationType;
                }
                else
                {
                    return Type.Unit;
                }

            default: throw new NotImplementedException();
            }
        }
    }
}
