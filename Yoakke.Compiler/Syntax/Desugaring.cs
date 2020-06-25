using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yoakke.Compiler.Ast;

namespace Yoakke.Compiler.Syntax
{
    /// <summary>
    /// Syntax desugaring.
    /// Removes syntax only useful for the user's convenience.
    /// </summary>
    static class Desugaring
    {
        /// <summary>
        /// Desugars the given <see cref="Declaration.Program"/>.
        /// </summary>
        /// <param name="program">The <see cref="Declaration.Program"/> to desugar.</param>
        /// <returns>The desugared syntax <see cref="Node"/>.</returns>
        public static Declaration.Program Desugar(Declaration.Program program)
        {
            return (Declaration.Program)Desugar((Statement)program);
        }

        /// <summary>
        /// Desugars the given <see cref="Statement"/>.
        /// </summary>
        /// <param name="program">The <see cref="Statement"/> to desugar.</param>
        /// <returns>The desugared syntax <see cref="Node"/>.</returns>
        public static Statement Desugar(Statement statement)
        {
            switch (statement)
            {
            case Declaration.Program program:
                return new Declaration.Program(DesugarList(program.Declarations));

            case Declaration.ConstDef constDef:
                return new Declaration.ConstDef(
                    constDef.Name,
                    DesugarNullable(constDef.Type),
                    Desugar(constDef.Value));

            case Statement.Return ret:
                return new Statement.Return(DesugarNullable(ret.Value));

            case Statement.VarDef varDef:
                return new Statement.VarDef(varDef.Name, DesugarNullable(varDef.Type), Desugar(varDef.Value));

            case Statement.Expression_ expr:
                return new Statement.Expression_(Desugar(expr.Expression), expr.HasSemicolon);

            default: throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Desugars the given <see cref="Expression"/>.
        /// </summary>
        /// <param name="program">The <see cref="Expression"/> to desugar.</param>
        /// <returns>The desugared syntax <see cref="Node"/>.</returns>
        public static Expression Desugar(Expression expression)
        {
            switch (expression)
            {
            case Expression.IntLit _:
            case Expression.BoolLit _:
            case Expression.StrLit _:
            case Expression.Ident _:
            case Expression.Intrinsic _:
                // Nothing to do, leaf nodes
                return expression;

            case Expression.DotPath dotPath:
                return new Expression.DotPath(Desugar(dotPath.Left), dotPath.Right);

            case Expression.StructType structType:
                return new Expression.StructType(
                    structType.Token,
                    structType.Fields.Select(x => new Expression.StructType.Field(x.Name, Desugar(x.Type))).ToList(),
                    DesugarList(structType.Declarations));

            case Expression.StructValue structValue:
                return new Expression.StructValue(
                    Desugar(structValue.StructType),
                    structValue.Fields.Select(x => new Expression.StructValue.Field(x.Name, Desugar(x.Value))).ToList());

            case Expression.ProcType procType:
                return new Expression.ProcType(
                    DesugarList(procType.ParameterTypes),
                    DesugarNullable(procType.ReturnType));

            case Expression.ProcValue proc:
                return new Expression.ProcValue(
                    proc.Parameters.Select(x => new Expression.ProcValue.Parameter(x.Name, Desugar(x.Type))).ToList(),
                    DesugarNullable(proc.ReturnType),
                    Desugar(proc.Body));

            case Expression.Block block:
            {
                var result = new Expression.Block(DesugarList(block.Statements), DesugarNullable(block.Value));
                if (   result.Value == null 
                    && result.Statements.Count > 0 
                    && result.Statements.Last() is Statement.Expression_ expr
                    && !expr.HasSemicolon
                    && HasExplicitValue(expr.Expression))
                {
                    // No return value, last statement is an expression without semicolon, has explicit return value
                    // We move that expression to the value
                    result.Value = expr.Expression;
                    result.Statements.RemoveAt(result.Statements.Count - 1);
                }
                return result;
            }

            case Expression.Call call:
                return new Expression.Call(Desugar(call.Proc), DesugarList(call.Arguments));

            case Expression.If iff:
                return new Expression.If(Desugar(iff.Condition), Desugar(iff.Then), DesugarNullable(iff.Else));

            case Expression.BinOp binOp:
                return new Expression.BinOp(Desugar(binOp.Left), binOp.Operator, Desugar(binOp.Right));

            default: throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Checks if an <see cref="Expression"/> explicitly specifies a return value.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to check.</param>
        /// <returns>True, if it explicitly returns a value.</returns>
        private static bool HasExplicitValue(Expression expression)
        {
            switch (expression)
            {
            case Expression.Block block:
                // NOTE: Do we need to check the value here? Probably not because it was desugared?
                return block.Value != null;

            case Expression.If iff:
                return HasExplicitValue(iff.Then) || (iff.Else != null && HasExplicitValue(iff.Else));

            default: return true;
            }
        }

        // Helper dispatching

        private static Expression? DesugarNullable(Expression? expression) =>
            expression == null ? null : Desugar(expression);

        private static List<Declaration> DesugarList(List<Declaration> declarations) =>
            declarations.Select(x => (Declaration)Desugar(x)).ToList();

        private static List<Statement> DesugarList(List<Statement> statements) =>
            statements.Select(Desugar).ToList();

        private static List<Expression> DesugarList(List<Expression> expressions) =>
            expressions.Select(Desugar).ToList();
    }
}
