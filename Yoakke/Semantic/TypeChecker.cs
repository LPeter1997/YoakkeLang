using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yoakke.Ast;

namespace Yoakke.Semantic
{
    static class TypeChecker
    {
        public static Type CheckType(Expression expression)
        {
            if (expression.Type == null)
            {
                expression.Type = CheckTypeInternal(expression);
            }
            return expression.Type;
        }

        private static Type CheckTypeInternal(Expression expression)
        {
            switch (expression)
            {
            // TODO: Some generic integral type?
            case IntLiteralExpression intLiteral: return Type.I32;

            // TODO
            case IdentifierExpression identifier: throw new NotImplementedException();

            case ProcExpression proc:
            {
                var parameterTypes = proc.Parameters.Select(x => CheckType(x.Type)).ToList();
                var returnType = proc.ReturnType == null ? Type.Unit : CheckType(proc.ReturnType);
                var bodyType = CheckType(proc.Body);
                // TODO: Make sure return type is a concrete type, so signatures are always fix?
                // Same with params?
                Unifier.Unify(bodyType, returnType);
                return returnType;
            }

            case BlockExpression block:
            {
                if (block.Value == null) return Type.Unit;
                return CheckType(block.Value);
            }

            default: throw new NotImplementedException();
            }
        }
    }
}
