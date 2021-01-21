using System;
using System.Diagnostics;

namespace Yoakke.Syntax.Ast
{
    /// <summary>
    /// A visitor for AST nodes.
    /// </summary>
    /// <typeparam name="T">The result type of the visitation.</typeparam>
    public abstract class Visitor<T>
    {
        [DebuggerStepThrough]
        protected T VisitNonNull(Declaration declaration) => AssertNonNull(Visit(declaration));
        [DebuggerStepThrough]
        protected T VisitNonNull(Statement statement) => AssertNonNull(Visit(statement));
        [DebuggerStepThrough]
        protected T VisitNonNull(Expression expression) => AssertNonNull(Visit(expression));

        [DebuggerStepThrough]
        private static T AssertNonNull(T? value)
        {
            Debug.Assert(value != null);
            return value;
        }

        [DebuggerStepThrough]
        protected virtual T? Visit(Node node) => node switch
        {
            Declaration d => Visit(d),
            Statement   s => Visit(s),
            Expression  e => Visit(e),

            _ => throw new NotImplementedException(),
        };

        [DebuggerStepThrough]
        protected virtual T? Visit(Declaration declaration) => declaration switch
        {
            Declaration.File  f => Visit(f),
            Declaration.Const c => Visit(c),

            Expression.StructType.Field f => Visit(f),

            Expression.ProcSignature.Parameter p => Visit(p),

            _ => throw new NotImplementedException(),
        };

        [DebuggerStepThrough]
        protected virtual T? Visit(Statement statement) => statement switch
        {
            Declaration           d => Visit(d),
            Statement.Var         v => Visit(v),
            Statement.Return      r => Visit(r),
            Statement.Expression_ e => Visit(e),

            Expression.StructValue.Field f => Visit(f),

            _ => throw new NotImplementedException(),
        };

        [DebuggerStepThrough]
        protected virtual T? Visit(Expression expression) => expression switch
        {
            Expression.Literal       l => Visit(l),
            Expression.Identifier    i => Visit(i),
            Expression.ArrayType     a => Visit(a),
            Expression.StructType    s => Visit(s),
            Expression.StructValue   s => Visit(s),
            Expression.ProcSignature s => Visit(s),
            Expression.Proc          p => Visit(p),
            Expression.Block         b => Visit(b),
            Expression.Call          c => Visit(c),
            Expression.Subscript     s => Visit(s),
            Expression.If            i => Visit(i),
            Expression.While         w => Visit(w),
            Expression.Binary        b => Visit(b),
            Expression.Unary         u => Visit(u),
            Expression.DotPath       d => Visit(d),
            Expression.Const         c => Visit(c),

            _ => throw new NotImplementedException(),
        };

        [DebuggerStepThrough]
        private T? VisitNullable(Expression? expression)
        {
            if (expression != null) return Visit(expression);
            return default;
        }

        protected virtual T? Visit(Declaration.File file)
        {
            foreach (var stmt in file.Statements) Visit(stmt);
            return default;
        }

        protected virtual T? Visit(Declaration.Const cons)
        {
            VisitNullable(cons.Type);
            Visit(cons.Value);
            return default;
        }

        protected virtual T? Visit(Statement.Var var)
        {
            VisitNullable(var.Type);
            VisitNullable(var.Value);
            return default;
        }

        protected virtual T? Visit(Statement.Return ret)
        {
            VisitNullable(ret.Value);
            return default;
        }

        protected virtual T? Visit(Statement.Expression_ expr)
        {
            Visit(expr.Expression);
            return default;
        }

        protected virtual T? Visit(Expression.Literal lit) => default;
        protected virtual T? Visit(Expression.Identifier ident) => default;

        protected virtual T? Visit(Expression.ArrayType aty)
        {
            Visit(aty.Length);
            Visit(aty.ElementType);
            return default;
        }

        protected virtual T? Visit(Expression.StructType sty)
        {
            foreach (var field in sty.Fields) Visit(field);
            foreach (var decl in sty.Declarations) Visit(decl);
            return default;
        }

        protected virtual T? Visit(Expression.StructValue sval)
        {
            Visit(sval.StructType);
            foreach (var field in sval.Fields) Visit(field);
            return default;
        }

        protected virtual T? Visit(Expression.ProcSignature sign)
        {
            foreach (var param in sign.Parameters) Visit(param);
            VisitNullable(sign.Return);
            return default;
        }

        protected virtual T? Visit(Expression.Proc proc)
        {
            Visit(proc.Signature);
            Visit(proc.Body);
            return default;
        }

        protected virtual T? Visit(Expression.Block block)
        {
            foreach (var stmt in block.Statements) Visit(stmt);
            VisitNullable(block.Value);
            return default;
        }

        protected virtual T? Visit(Expression.Call call)
        {
            Visit(call.Procedure);
            foreach (var arg in call.Arguments) Visit(arg);
            return default;
        }

        protected virtual T? Visit(Expression.Subscript sub)
        {
            Visit(sub.Array);
            Visit(sub.Index);
            return default;
        }

        protected virtual T? Visit(Expression.If iff)
        {
            Visit(iff.Condition);
            Visit(iff.Then);
            VisitNullable(iff.Else);
            return default;
        }

        protected virtual T? Visit(Expression.While whil)
        {
            Visit(whil.Condition);
            Visit(whil.Body);
            return default;
        }

        protected virtual T? Visit(Expression.Binary bin)
        {
            Visit(bin.Left);
            Visit(bin.Right);
            return default;
        }

        protected virtual T? Visit(Expression.Unary ury)
        {
            Visit(ury.Operand);
            return default;
        }

        protected virtual T? Visit(Expression.DotPath dot)
        {
            Visit(dot.Left);
            return default;
        }

        protected virtual T? Visit(Expression.Const cons)
        {
            Visit(cons.Subexpression);
            return default;
        }

        protected virtual T? Visit(Expression.StructType.Field field)
        {
            Visit(field.Type);
            return default;
        }

        protected virtual T? Visit(Expression.StructValue.Field field)
        {
            Visit(field.Value);
            return default;
        }

        protected virtual T? Visit(Expression.ProcSignature.Parameter param)
        {
            Visit(param.Type);
            return default;
        }
    }
}
