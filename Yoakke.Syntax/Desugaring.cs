using System;
using System.Linq;
using System.Transactions;
using Yoakke.Syntax.Ast;

namespace Yoakke.Syntax
{
    /// <summary>
    /// Desugaring the AST.
    /// </summary>
    public class Desugaring : Transformator
    {
        /// <summary>
        /// Desugars a <see cref="Declaration.File"/>.
        /// </summary>
        /// <param name="file">The <see cref="Declaration.File"/> to desugar.</param>
        /// <returns>The desugared <see cref="Declaration.File"/>.</returns>
        public Declaration.File Desugar(Declaration.File file) =>
            (Declaration.File)Desugar((Declaration)file);

        /// <summary>
        /// Desugars a <see cref="Declaration"/>.
        /// </summary>
        /// <param name="declaration">The <see cref="Declaration"/> to desugar.</param>
        /// <returns>The desugared <see cref="Declaration"/>.</returns>
        public Declaration Desugar(Declaration declaration) => Transform(declaration);

        /// <summary>
        /// Desugars a <see cref="Statement"/>.
        /// </summary>
        /// <param name="statement">The <see cref="Statement"/> to desugar.</param>
        /// <returns>The desugared <see cref="Statement"/>.</returns>
        public Statement Desugar(Statement statement) => Transform(statement);

        /// <summary>
        /// Desugars an <see cref="Expression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to desugar.</param>
        /// <returns>The desugared <see cref="Expression"/>.</returns>
        public Expression Desugar(Expression expression) => Transform(expression);

        protected override Node? Visit(Expression.Proc proc) =>
            new Expression.Proc(
                proc.ParseTreeNode,
                (Expression.ProcSignature)Transform(proc.Signature),
                DesugarProcBody(proc.Body));

        protected override Expression Visit(Expression.Block block)
        {
            var statements = block.Statements.Select(Desugar).ToArray();
            var value = TransformNullable(block.Value);
            if (   value == null 
                && statements.Length > 0
                && statements.Last() is Statement.Expression_ expr
                && !expr.HasSemicolon
                && HasExplicitValue(expr.Expression))
            {
                // There was no return value, but the last statement was an expression without a semicolon
                // Promote it to value
                value = expr.Expression;
                statements = statements.SkipLast(1).ToArray();
            }
            return new Expression.Block(block.ParseTreeNode, statements, value);
        }

        private Expression DesugarProcBody(Expression body)
        {
            body = Transform(body);
            if (HasExplicitValue(body))
            {
                // Wrap in a return statement
                return new Expression.Block(
                    body.ParseTreeNode,
                    new Statement[] { new Statement.Return(body.ParseTreeNode, body) },
                    null);
            }
            return body;
        }

        private static bool HasExplicitValue(Expression expression) => expression switch
        {
            Expression.Block block => block.Value != null,

            // TODO: Don't we always want else to exist?
            Expression.If iff => HasExplicitValue(iff.Then) || (iff.Else != null && HasExplicitValue(iff.Else)),

            _ => true,
        };
    }
}
