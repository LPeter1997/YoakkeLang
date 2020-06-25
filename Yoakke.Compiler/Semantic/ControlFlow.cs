using System;
using System.Collections.Generic;
using System.Linq;
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

        private static ReturnKind Sequence(IEnumerable<ReturnKind> kinds)
        {
            var result = ReturnKind.DoesNotReturn;
            foreach (var k in kinds) result = Sequence(result, k);
            return result;
        }

        private static ReturnKind Alternative(ReturnKind k1, ReturnKind k2)
        {
            if (k1 == ReturnKind.AlwaysReturns && k2 == ReturnKind.AlwaysReturns) return ReturnKind.AlwaysReturns;
            if (k1 == ReturnKind.DoesNotReturn && k2 == ReturnKind.DoesNotReturn) return ReturnKind.DoesNotReturn;
            return ReturnKind.MightReturn;
        }

        // TODO: Value and type level should be differentiated!
        // Now type-level returns can give us false-positives for the run-time code!

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

            case Statement.Expression_ expr: return Analyze(expr.Expression);

            case Statement.VarDef varDef:
            {
                var result = ReturnKind.DoesNotReturn;
                if (varDef.Type != null) result = Sequence(result, Analyze(varDef.Type));
                return Sequence(result, Analyze(varDef.Value));
            }
            
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
                // TODO: Here for example it matters what the type of LHS is!
                return Analyze(dotPath.Left);

            case Expression.StructType structType:
                return Sequence(structType.Fields.Select(x => Analyze(x.Type)));

            case Expression.StructValue structValue:
            {
                var r1 = Analyze(structValue.StructType);
                var r2 = Sequence(structValue.Fields.Select(x => Analyze(x.Value)));
                return Sequence(r1, r2);
            }

            case Expression.ProcType procType:
            {
                var result = Sequence(procType.ParameterTypes.Select(Analyze));
                if (procType.ReturnType != null) result = Sequence(result, Analyze(procType.ReturnType));
                return result;
            }

            case Expression.ProcValue proc:
            {
                var result = Sequence(proc.Parameters.Select(x => Analyze(x.Type)));
                if (proc.ReturnType != null) result = Sequence(result, Analyze(proc.ReturnType));
                return result;
            }

            case Expression.Block block:
                return Sequence(block.Statements.Select(Analyze));

            case Expression.Call call:
            {
                var r1 = Analyze(call.Proc);
                var r2 = Sequence(call.Arguments.Select(Analyze));
                return Sequence(r1, r2);
            }

            case Expression.If iff:
            {
                var if_then = Sequence(Analyze(iff.Condition), Analyze(iff.Then));
                var els = iff.Else == null ? ReturnKind.DoesNotReturn : Analyze(iff.Else);
                return Alternative(if_then, els);
            }

            case Expression.BinOp binOp:
                // TODO: For lazy operators this is the same as an if-else!
                return Sequence(Analyze(binOp.Left), Analyze(binOp.Right));

            default: throw new NotImplementedException();
            }
        }
    }
}
