﻿using System;
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
                foreach (var decl in program.Declarations) CheckType(decl);
                break;

            case ConstDefinition constDef:
                var exprType = CheckType(constDef.Value);
                if (constDef.Type != null)
                {
                    var type = CheckType(constDef.Type);
                    Unifier.Unify(type, exprType);
                }
                break;

            case ExpressionStatement expression:
                CheckType(expression.Expression);
                break;

            default: throw new NotImplementedException();
            }
        }

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
                foreach (var stmt in block.Statements) CheckType(stmt);
                if (block.Value == null) return Type.Unit;
                return CheckType(block.Value);
            }

            default: throw new NotImplementedException();
            }
        }
    }
}
