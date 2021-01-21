using System.Linq;

namespace Yoakke.Syntax.Ast
{
    /// <summary>
    /// A transtormation for AST <see cref="Node"/>s.
    /// </summary>
    public class Transformator : Visitor<Node>
    {
        /// <summary>
        /// Transforms a <see cref="Declaration"/>.
        /// </summary>
        /// <param name="decl">The <see cref="Declaration"/> to transform.</param>
        /// <returns>The transformed <see cref="Declaration"/>.</returns>
        protected Declaration Transform(Declaration decl) => (Declaration)VisitNonNull(decl);
        /// <summary>
        /// Transforms a <see cref="Statement"/>.
        /// </summary>
        /// <param name="decl">The <see cref="Statement"/> to transform.</param>
        /// <returns>The transformed <see cref="Statement"/>.</returns>
        protected Statement Transform(Statement stmt) => (Statement)VisitNonNull(stmt);
        /// <summary>
        /// Transforms an <see cref="Expression"/>.
        /// </summary>
        /// <param name="decl">The <see cref="Expression"/> to transform.</param>
        /// <returns>The transformed <see cref="Expression"/>.</returns>
        protected Expression Transform(Expression expr) => (Expression)VisitNonNull(expr);

        protected Expression? TransformNullable(Expression? expr) => expr == null ? null : Transform(expr);

        // Declarations ////////////////////////////////////////////////////////

        protected override Node? Visit(Declaration.Const cons) => 
            new Declaration.Const(
                cons.ParseTreeNode, 
                cons.Name,
                TransformNullable(cons.Type),
                Transform(cons.Value));

        protected override Node? Visit(Declaration.File file) =>
            new Declaration.File(
                file.ParseTreeNode,
                file.Statements.Select(Transform).ToArray());

        // Statements //////////////////////////////////////////////////////////

        protected override Node? Visit(Statement.Var var) =>
            new Statement.Var(
                var.ParseTreeNode,
                var.Name,
                TransformNullable(var.Type),
                TransformNullable(var.Value));

        protected override Node? Visit(Statement.Return ret) =>
            new Statement.Return(
                ret.ParseTreeNode,
                TransformNullable(ret.Value));

        protected override Node? Visit(Statement.Expression_ expr) =>
            new Statement.Expression_(
                expr.ParseTreeNode,
                Transform(expr.Expression),
                expr.HasSemicolon);

        // Expressions /////////////////////////////////////////////////////////

        protected override Node? Visit(Expression.ArrayType aty) =>
            new Expression.ArrayType(
                aty.ParseTreeNode,
                Transform(aty.Length),
                Transform(aty.ElementType));

        protected override Node? Visit(Expression.Binary bin) =>
            new Expression.Binary(
                bin.ParseTreeNode,
                Transform(bin.Left),
                bin.Operator,
                Transform(bin.Right));

        protected override Node? Visit(Expression.Block block) =>
            new Expression.Block(
                block.ParseTreeNode,
                block.Statements.Select(Transform).ToArray(),
                TransformNullable(block.Value));

        protected override Node? Visit(Expression.Call call) =>
            new Expression.Call(
                call.ParseTreeNode,
                Transform(call.Procedure),
                call.Arguments.Select(Transform).ToArray());

        protected override Node? Visit(Expression.DotPath dot) =>
            new Expression.DotPath(
                dot.ParseTreeNode,
                Transform(dot.Left),
                dot.Right);

        protected override Node? Visit(Expression.If iff) =>
            new Expression.If(
                iff.ParseTreeNode,
                Transform(iff.Condition),
                Transform(iff.Then),
                TransformNullable(iff.Else));

        protected override Node? Visit(Expression.Identifier ident) => ident;
        protected override Node? Visit(Expression.Literal lit) => lit;

        protected override Node? Visit(Expression.Proc proc) =>
            new Expression.Proc(
                proc.ParseTreeNode,
                (Expression.ProcSignature)Transform(proc.Signature),
                Transform(proc.Body));

        protected override Node? Visit(Expression.ProcSignature sign) =>
            new Expression.ProcSignature(
                sign.ParseTreeNode,
                sign.Parameters.Select(Transform).ToArray(),
                TransformNullable(sign.Return));

        protected override Node? Visit(Expression.ProcSignature.Parameter param) =>
            new Expression.ProcSignature.Parameter(
                param.ParseTreeNode,
                param.Name,
                Transform(param.Type));

        protected override Node? Visit(Expression.StructType sty) =>
            new Expression.StructType(
                sty.ParseTreeNode,
                sty.KwStruct,
                sty.Fields.Select(Transform).ToArray(),
                sty.Declarations.Select(Transform).ToArray());

        protected override Node? Visit(Expression.StructType.Field field) =>
            new Expression.StructType.Field(
                field.ParseTreeNode,
                field.Name,
                Transform(field.Type));

        protected override Node? Visit(Expression.StructValue sval) =>
            new Expression.StructValue(
                sval.ParseTreeNode,
                Transform(sval.StructType),
                sval.Fields.Select(Transform).ToArray());

        protected override Node? Visit(Expression.StructValue.Field field) =>
            new Expression.StructValue.Field(
                field.ParseTreeNode,
                field.Name,
                Transform(field.Value));

        protected override Node? Visit(Expression.Subscript sub) =>
            new Expression.Subscript(
                sub.ParseTreeNode,
                Transform(sub.Array),
                Transform(sub.Index));

        protected override Node? Visit(Expression.Unary ury) =>
            new Expression.Unary(
                ury.ParseTreeNode,
                ury.Operator,
                Transform(ury.Operand));

        protected override Node? Visit(Expression.While whil) =>
            new Expression.While(
                whil.ParseTreeNode,
                Transform(whil.Condition),
                Transform(whil.Body));

        protected override Node? Visit(Expression.Const cons) =>
            new Expression.Const(
                cons.ParseTreeNode,
                Transform(cons.Subexpression));

        protected Expression.ProcSignature.Parameter Transform(Expression.ProcSignature.Parameter param) =>
            (Expression.ProcSignature.Parameter)VisitNonNull(param);

        protected Expression.StructType.Field Transform(Expression.StructType.Field field) =>
            (Expression.StructType.Field)VisitNonNull(field);

        protected Expression.StructValue.Field Transform(Expression.StructValue.Field field) =>
            (Expression.StructValue.Field)VisitNonNull(field);
    }
}
