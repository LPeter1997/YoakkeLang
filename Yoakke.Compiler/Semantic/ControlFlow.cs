using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.Compiler.Ast;

namespace Yoakke.Compiler.Semantic
{
    /// <summary>
    /// Does control flow analysis on an AST <see cref="Node"/>.
    /// </summary>
    static class ControlFlow
    {
        /// <summary>
        /// The possibilities of how the piece of code returns.
        /// </summary>
        public enum ReturnKind
        {
            /// <summary>
            /// Never returns.
            /// </summary>
            DoesNotReturn,
            /// <summary>
            /// Could return in some cases.
            /// </summary>
            MightReturn,
            /// <summary>
            /// Returns no matter what.
            /// </summary>
            AlwaysReturns,
        }

        private static ReturnKind Sequence(ReturnKind k1, ReturnKind k2)
        {
            if (k1 == ReturnKind.AlwaysReturns || k2 == ReturnKind.AlwaysReturns) return ReturnKind.AlwaysReturns;
            if (k1 == ReturnKind.DoesNotReturn && k2 == ReturnKind.DoesNotReturn) return ReturnKind.DoesNotReturn;
            return ReturnKind.MightReturn;
        }

        private static ReturnKind Alternative(ReturnKind k1, ReturnKind k2)
        {
            if (k1 == ReturnKind.AlwaysReturns && k2 == ReturnKind.AlwaysReturns) return ReturnKind.AlwaysReturns;
            if (k1 == ReturnKind.DoesNotReturn && k2 == ReturnKind.DoesNotReturn) return ReturnKind.DoesNotReturn;
            return ReturnKind.MightReturn;
        }

        /// <summary>
        /// Checks the <see cref="ReturnKind"/> of a given <see cref="Statement"/>.
        /// </summary>
        /// <param name="statement">The <see cref="Statement"/> to check.</param>
        /// <returns>The <see cref="ReturnKind"/> of the <see cref="Statement"/>.</returns>
        public static ReturnKind Analyze(Statement statement)
        {
            switch (statement)
            {
            case Declaration.ConstDef _: return ReturnKind.DoesNotReturn;

            case Statement.Return _: return ReturnKind.AlwaysReturns;

            case Statement.VarDef varDef: return Analyze(varDef.Value);

            case Statement.Expression_ expr: return Analyze(expr.Expression);

            default: throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Checks the <see cref="ReturnKind"/> of a given <see cref="Expression"/>.
        /// </summary>
        /// <param name="statement">The <see cref="Expression"/> to check.</param>
        /// <returns>The <see cref="ReturnKind"/> of the <see cref="Expression"/>.</returns>
        public static ReturnKind Analyze(Expression expression)
        {
            switch (expression)
            {
            case Expression.IntLit _:
            case Expression.BoolLit _:
            case Expression.StrLit _:
            case Expression.Ident _:
            case Expression.Intrinsic _:
                return ReturnKind.DoesNotReturn;

            case Expression.DotPath dotPath:
                return Analyze(dotPath.Left);

            case Expression.StructType structType:
            {
                var result = ReturnKind.DoesNotReturn;
                foreach (var field in structType.Fields) result = Sequence(result, Analyze(field.Type));
                return result;
            }

            case Expression.StructValue structValue:
            {
                var result = ReturnKind.DoesNotReturn;
                result = Sequence(result, Analyze(structValue.StructType));
                foreach (var field in structValue.Fields) result = Sequence(result, Analyze(field.Value));
                return result;
            }

            case Expression.ProcType procType:
            {
                var result = ReturnKind.DoesNotReturn;
                foreach (var param in procType.ParameterTypes) result = Sequence(result, Analyze(param));
                if (procType.ReturnType != null) result = Sequence(result, Analyze(procType.ReturnType));
                return result;
            }

            case Expression.ProcValue proc:
            {
                var result = ReturnKind.DoesNotReturn;
                foreach (var param in proc.Parameters) result = Sequence(result, Analyze(param.Type));
                if (proc.ReturnType != null) result = Sequence(result, Analyze(proc.ReturnType));
                return result;
            }

            case Expression.Block block:
            {
                var result = ReturnKind.DoesNotReturn;
                foreach (var stmt in block.Statements) result = Sequence(result, Analyze(stmt));
                return result;
            }

            case Expression.Call call:
            {
                var result = ReturnKind.DoesNotReturn;
                result = Sequence(result, Analyze(call.Proc));
                foreach (var arg in call.Arguments) result = Sequence(result, Analyze(arg));
                return result;
            }

            case Expression.If iff:
            {
                var then = Analyze(iff.Then);
                var els = iff.Else == null ? ReturnKind.DoesNotReturn : Analyze(iff.Else);
                return Alternative(then, els);
            }

            case Expression.BinOp binOp:
                // TODO: For lazy operators this is the same as an if-else!
                return Sequence(Analyze(binOp.Left), Analyze(binOp.Right));

            default: throw new NotImplementedException();
            }
        }
    }
}
