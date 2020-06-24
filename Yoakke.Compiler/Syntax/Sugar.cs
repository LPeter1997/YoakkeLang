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
    static class Sugar
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
                return new Declaration.Program(program.Declarations.Select(x => (Declaration)Desugar(x)).ToList());

            case Declaration.ConstDef constDef:
                return new Declaration.ConstDef(
                    constDef.Name,
                    constDef.Type == null ? null : Desugar(constDef.Type),
                    Desugar(constDef.Value));

            case Statement.Return ret:
                return new Statement.Return(ret.Value == null ? null : Desugar(ret.Value));

            case Statement.VarDef varDef:
                return new Statement.VarDef(
                    varDef.Name,
                    varDef.Type == null ? null : Desugar(varDef.Type),
                    Desugar(varDef.Value));

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
                    structType.Declarations.Select(x => (Declaration)Desugar(x)).ToList());

            case Expression.StructValue structValue:
                return new Expression.StructValue(
                    Desugar(structValue.StructType),
                    structValue.Fields.Select(x => new Expression.StructValue.Field(x.Name, Desugar(x.Value))).ToList());

            case Expression.ProcType procType:
                return new Expression.ProcType(
                    procType.ParameterTypes.Select(x => Desugar(x)).ToList(),
                    procType.ReturnType == null ? null : Desugar(procType.ReturnType));

            case Expression.ProcValue proc:
                return new Expression.ProcValue(
                    proc.Parameters.Select(x => new Expression.ProcValue.Parameter(x.Name, Desugar(x.Type))).ToList(),
                    proc.ReturnType == null ? null : Desugar(proc.ReturnType),
                    Desugar(proc.Body));

            case Expression.Block block:
                return new Expression.Block(
                    block.Statements.Select(x => Desugar(x)).ToList(),
                    block.Value == null ? null : Desugar(block.Value));

            case Expression.Call call:
                return new Expression.Call(
                    Desugar(call.Proc),
                    call.Arguments.Select(x => Desugar(x)).ToList());

            case Expression.If iff:
                return new Expression.If(
                    Desugar(iff.Condition),
                    Desugar(iff.Then),
                    iff.Else == null ? null : Desugar(iff.Else));

            case Expression.BinOp binOp:
                return new Expression.BinOp(Desugar(binOp.Left), binOp.Operator, Desugar(binOp.Right));

            default: throw new NotImplementedException();
            }
        }
    }
}
