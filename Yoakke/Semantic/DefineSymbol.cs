using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.Ast;
using Yoakke.Utils;

namespace Yoakke.Semantic
{
    /// <summary>
    /// Defines every symbol, also resolves each symbol reference.
    /// </summary>
    static class DefineSymbol
    {
        /// <summary>
        /// Defines the order-dependent symbols, resolves symbol references.
        /// </summary>
        /// <param name="statement">The statement to start the definitions from.</param>
        public static void Define(Statement statement)
        {
            switch (statement)
            {
            case ProgramDeclaration program:
                // Just loop through every declaration
                foreach (var decl in program.Declarations) Define(decl);
                break;

            case ConstDefinition constDef:
                // First define everything in value
                Define(constDef.Value);
                // For safety, define in type too
                if (constDef.Type != null) Define(constDef.Type);
                break;

            case ExpressionStatement expression:
                // Define in the expression
                Define(expression.Expression);
                break;

            default: throw new NotImplementedException();
            }
        }

        private static void Define(Expression expression)
        {
            switch (expression)
            {
            case IntLiteralExpression intLiteral:
                // Nothing to define, primitive literal
                break;

            case IdentifierExpression identifier:
                // We want to resolve the referred symbol
                Assert.NonNull(identifier.Scope);
                identifier.Symbol = identifier.Scope.Reference(identifier.Token);
                break;

            case ProcExpression proc:
                // We define each parameter
                foreach (var param in proc.Parameters)
                {
                    // Also define inside each parameter type
                    Define(param.Type);
                    var symbol = new VariableSymbol(param.Name);
                    param.Symbol = symbol;
                    Assert.NonNull(param.Type.Scope);
                    param.Type.Scope.Define(symbol);
                }
                // Define in return-type
                if (proc.ReturnType != null) Define(proc.ReturnType);
                // Define in body
                Define(proc.Body);
                break;

            case BlockExpression block:
                // Define in each statement
                foreach (var stmt in block.Statements) Define(stmt);
                // In return value too
                if (block.Value != null) Define(block.Value);
                break;

            default: throw new NotImplementedException();
            }
        }
    }
}
