using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Syntax.Ast;

namespace Yoakke.Syntax
{
    /// <summary>
    /// Desugaring the AST.
    /// </summary>
    public static class Desugaring
    {
        /// <summary>
        /// Desugars a <see cref="Declaration.File"/>.
        /// </summary>
        /// <param name="file">The <see cref="Declaration.File"/> to desugar.</param>
        /// <returns>The desugared <see cref="Declaration.File"/>.</returns>
        public static Declaration.File Desugar(Declaration.File file) =>
            (Declaration.File)Desugar((Declaration)file);

        /// <summary>
        /// Desugars a <see cref="Declaration"/>.
        /// </summary>
        /// <param name="declaration">The <see cref="Declaration"/> to desugar.</param>
        /// <returns>The desugared <see cref="Declaration"/>.</returns>
        public static Declaration Desugar(Declaration declaration) => declaration switch
        {
            Declaration.File file =>
                new Declaration.File(file.ParseTreeNode, file.Statements.Select(Desugar).ToArray()),

            Declaration.Const cons => 
                new Declaration.Const(cons.ParseTreeNode, cons.Name, DesugarNullable(cons.Type), Desugar(cons.Value)),

            _ => throw new NotImplementedException(),
        };

        /// <summary>
        /// Desugars a <see cref="Statement"/>.
        /// </summary>
        /// <param name="statement">The <see cref="Statement"/> to desugar.</param>
        /// <returns>The desugared <see cref="Statement"/>.</returns>
        public static Statement Desugar(Statement statement) => statement switch
        {
            Statement.Var var =>
                new Statement.Var(var.ParseTreeNode, var.Name, DesugarNullable(var.Type), DesugarNullable(var.Value)),

            Statement.Return ret =>
                new Statement.Return(ret.ParseTreeNode, DesugarNullable(ret.Value)),

            Statement.Expression_ expr =>
                new Statement.Expression_(expr.ParseTreeNode, Desugar(expr.Expression)),

            _ => throw new NotImplementedException(),
        };

        /// <summary>
        /// Desugars an <see cref="Expression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to desugar.</param>
        /// <returns>The desugared <see cref="Expression"/>.</returns>
        public static Expression Desugar(Expression expression) => expression switch
        {
            Expression.Literal lit => lit,
            Expression.Identifier ident => ident,

            Expression.StructType sty =>
                new Expression.StructType(sty.ParseTreeNode, sty.Fields.Select(Desugar).ToArray()),

            Expression.StructValue sval =>
                new Expression.StructValue(
                    sval.ParseTreeNode, 
                    Desugar(sval.StructType), 
                    sval.Fields.Select(Desugar).ToArray()),

            Expression.ProcSignature sign =>
                new Expression.ProcSignature(
                    sign.ParseTreeNode, 
                    sign.Parameters.Select(Desugar).ToArray(), 
                    DesugarNullable(sign.Return)),

            Expression.Proc proc =>
                new Expression.Proc(
                    proc.ParseTreeNode, 
                    (Expression.ProcSignature)Desugar(proc.Signature), 
                    Desugar(proc.Body)),

            Expression.Block block =>
                new Expression.Block(
                    block.ParseTreeNode, 
                    block.Statements.Select(Desugar).ToArray(), 
                    DesugarNullable(block.Value)),

            Expression.Call call =>
                new Expression.Call(
                    call.ParseTreeNode, 
                    Desugar(call.Procedure), 
                    call.Arguments.Select(Desugar).ToArray()),

            Expression.If iff =>
                new Expression.If(
                    iff.ParseTreeNode, 
                    Desugar(iff.Condition), 
                    Desugar(iff.Then), 
                    DesugarNullable(iff.Else)),

            Expression.While whil =>
                new Expression.While(whil.ParseTreeNode, Desugar(whil.Condition), Desugar(whil.Condition)),

            Expression.Binary bin =>
                new Expression.Binary(bin.ParseTreeNode, Desugar(bin.Left), bin.Operator, Desugar(bin.Right)),

            Expression.DotPath dot =>
                new Expression.DotPath(dot.ParseTreeNode, Desugar(dot.Left), dot.Right),

            _ => throw new NotImplementedException(),
        };

        private static Expression.StructType.Field Desugar(Expression.StructType.Field field) =>
            new Expression.StructType.Field(field.ParseTreeNode, field.Name, Desugar(field.Type));

        private static Expression.StructValue.Field Desugar(Expression.StructValue.Field field) =>
            new Expression.StructValue.Field(field.ParseTreeNode, field.Name, Desugar(field.Value));

        private static Expression.ProcSignature.Parameter Desugar(Expression.ProcSignature.Parameter p) =>
            new Expression.ProcSignature.Parameter(p.ParseTreeNode, p.Name, Desugar(p.Type));

        private static Expression? DesugarNullable(Expression? expression) =>
            expression == null ? null : Desugar(expression);
    }
}
