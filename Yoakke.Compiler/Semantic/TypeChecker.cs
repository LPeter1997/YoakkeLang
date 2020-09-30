using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Compile;
using Yoakke.Syntax;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Semantic
{
    /// <summary>
    /// A semantic pass to check static typing rules.
    /// </summary>
    public class TypeChecker : Visitor<object>
    {
        private IDependencySystem dependencySystem;

        // TODO: Doc
        public TypeChecker(IDependencySystem dependencySystem)
        {
            this.dependencySystem = dependencySystem;
        }

        // TODO: Doc
        public void Check(Statement statement) => Visit(statement);

        // TODO: Doc
        public void Check(Expression expression) => Visit(expression);

        protected override object? Visit(Declaration.Const cons)
        {
            base.Visit(cons);
            if (cons.Type != null)
            {
                // There is an explicit type declaration, has to match the type of the value
                var explicitType = dependencySystem.EvaluateToType(cons.Type);
                var valueType = dependencySystem.TypeOf(cons.Value);
                if (!explicitType.Equals(valueType))
                {
                    // TODO
                    throw new NotImplementedException();
                }
            }
            return null;
        }

        protected override object? Visit(Statement.Var var)
        {
            base.Visit(var);
            if (var.Type == null && var.Value == null)
            {
                // TODO
                throw new NotImplementedException();
            }
            if (var.Type != null && var.Value != null)
            {
                // Both a type and a value 
                var explicitType = dependencySystem.EvaluateToType(var.Type);
                var valueType = dependencySystem.TypeOf(var.Value);
                if (!explicitType.Equals(valueType))
                {
                    // TODO
                    throw new NotImplementedException();
                }
            }
            return null;
        }

        protected override object? Visit(Statement.Return ret)
        {
            base.Visit(ret);
            // TODO: Match current procedure's return type with the return value
            return null;
        }

        protected override object? Visit(Statement.Expression_ expr)
        {
            base.Visit(expr);
            var type = dependencySystem.TypeOf(expr.Expression);
            if (!expr.HasSemicolon)
            {
                // TODO: Unify with unit type
            }
            return null;
        }

        protected override object? Visit(Expression.Call call)
        {
            base.Visit(call);
            // We let the type-evaluation handle this one
            dependencySystem.TypeOf(call);
            return null;
        }

        protected override object? Visit(Expression.If iff)
        {
            base.Visit(iff);
            var conditionType = dependencySystem.TypeOf(iff.Condition);
            // TODO: Check if condition is bool
            var thenType = dependencySystem.TypeOf(iff.Then);
            if (iff.Else != null)
            {
                var elseType = dependencySystem.TypeOf(iff.Else);
                if (!thenType.Equals(elseType))
                {
                    // TODO
                    throw new NotImplementedException();
                }
            }
            else
            {
                // TODO: Unify then type with unit
            }
            return null;
        }

        protected override object? Visit(Expression.While whil)
        {
            base.Visit(whil);
            var conditionType = dependencySystem.TypeOf(whil.Condition);
            // TODO: Check if condition is bool
            var bodyType = dependencySystem.TypeOf(whil.Body);
            // TODO: Unify with unit type
            return null;
        }

        protected override object? Visit(Expression.Binary bin)
        {
            base.Visit(bin);
            var leftType = dependencySystem.TypeOf(bin.Left);
            var rightType = dependencySystem.TypeOf(bin.Right);

            if (bin.Operator == TokenType.Assign)
            {
                // The lhs and rhs gotta match
                if (!leftType.Equals(rightType))
                {
                    // TODO
                    throw new NotImplementedException();
                }
            }
            else
            {
                // TODO
                throw new NotImplementedException();
            }
            return null;
        }

        protected override object? Visit(Expression.DotPath dot)
        {
            base.Visit(dot);
            var leftType = dependencySystem.TypeOf(dot.Left);
            if (!(leftType is Type.Struct structType))
            {
                // TODO
                throw new NotImplementedException();
            }
            if (!structType.Fields.TryGetValue(dot.Right, out var fieldType))
            {
                // TODO
                throw new NotImplementedException();
            }
            return null;
        }
    }
}
