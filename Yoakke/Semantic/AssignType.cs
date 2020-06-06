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
            case Declaration.Program program:
                foreach (var decl in program.Declarations) Assign(decl);
                break;

            case Declaration.ConstDef constDef:
                if (constDef.Type != null) Assign(constDef.Type);
                Assign(constDef.Value);
                break;

            case Statement.Expression_ expression:
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
            case Expression.IntLit intLit:
                // TODO: This should have an abstract integral type instead!
                return EvaluateConst.Evaluate(intLit).Type;

            case Expression.Ident ident:
                Assert.NonNull(ident.Symbol);
                return ident.Symbol.AssumeHasType();

            case Expression.Proc proc:
                foreach (var param in proc.Parameters)
                {
                    Assign(param.Type);
                    var type = EvaluateConst.EvaluateToType(param.Type);
                    Assert.NonNull(param.Symbol);
                    param.Symbol.Type = type;
                }
                if (proc.ReturnType != null) Assign(proc.ReturnType);
                Assign(proc.Body);
                return EvaluateConst.Evaluate(proc).Type;

            case Expression.Block block:
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
