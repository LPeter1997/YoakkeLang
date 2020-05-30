using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yoakke.Ast;

namespace Yoakke.Semantic.Steps
{
    static class TypeChecker
    {
        public static void CheckType(Statement statement)
        {
            switch (statement)
            {
            case ProgramDeclaration program:
                // Type-check each declaration
                foreach (var decl in program.Declarations) CheckType(decl);
                break;

            case ConstDefinition constDef:
                // Get type of constant value
                var exprType = CheckType(constDef.Value);
                if (constDef.Type != null)
                {
                    // TODO: Type is probably "type (ValueType)"
                    // while value is "ValueType", we'd want to check if type is a type-type,
                    // and unify with subtype!
                    throw new NotImplementedException();
                    // If has a type, get the type of it
                    //var type = CheckType(constDef.Type);
                    // Unify with value
                    //Unifier.Unify(type, exprType);
                }
                var symbol = constDef.Symbol as ConstSymbol;
                if (symbol == null) throw new NotImplementedException();
                symbol.Type = exprType;
                break;

            case ExpressionStatement expression:
                CheckType(expression.Expression);
                break;

            default: throw new NotImplementedException();
            }
        }

        private static Type CheckType(Expression expression)
        {
            // Some cacheing
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

            case IdentifierExpression identifier:
            {
                if (identifier.Scope == null) throw new NotImplementedException();
                var symbol = identifier.Scope.Reference(identifier.Token.Value);
                identifier.Symbol = symbol;
                if (symbol.Type == null) throw new NotImplementedException();
                return symbol.Type;
            }    

            case ProcExpression proc:
            {
                var parameterTypes = new List<Type>();
                foreach (var param in proc.Parameters)
                {
                    var paramType = CheckType(param.Type);
                    parameterTypes.Add(paramType);
                    var symbol = param.Symbol as VariableSymbol;
                    if (symbol == null) throw new NotImplementedException();
                    symbol.Type = paramType;
                }
                var returnType = proc.ReturnType == null ? Type.Unit : CheckType(proc.ReturnType);
                var bodyType = CheckType(proc.Body);
                // TODO: Make sure return type is a concrete type, so signatures are always fix?
                // Same with params?
                Unifier.Unify(bodyType, returnType);
                return returnType;
            }

            case BlockExpression block:
                foreach (var stmt in block.Statements) CheckType(stmt);
                if (block.Value == null) return Type.Unit;
                return CheckType(block.Value);

            default: throw new NotImplementedException();
            }
        }
    }
}
