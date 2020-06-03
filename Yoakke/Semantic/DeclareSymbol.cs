using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.Ast;

namespace Yoakke.Semantic
{
    /// <summary>
    /// Declares every order-independent <see cref="Symbol"/>.
    /// </summary>
    static class DeclareSymbol
    {
        /// <summary>
        /// Declares every order-independent <see cref="Symbol"/> in the given <see cref="SymbolTable"/>.
        /// Also assigns the scope for every syntax tree node.
        /// </summary>
        /// <param name="symbolTable">The symbol table to use.</param>
        /// <param name="statement">The statement to start the declarations from.</param>
        public static void Declare(SymbolTable symbolTable, Statement statement)
        {
            statement.Scope = symbolTable.CurrentScope;

            switch (statement)
            {
            case ProgramDeclaration program:
                // Just loop through every declaration
                foreach (var decl in program.Declarations) Declare(symbolTable, decl);
                break;

            case ConstDefinition constDef:
            {
                // First declare everything in value
                Declare(symbolTable, constDef.Value);
                // For safety, declare in type too
                if (constDef.Type != null) Declare(symbolTable, constDef.Type);
                // Declare this symbol, store it and add to the symbol
                var symbol = new ConstSymbol(constDef);
                constDef.Symbol = symbol;
                symbolTable.CurrentScope.Define(symbol);
            }  
            break;

            case ExpressionStatement expression:
                // Declare in the expression
                Declare(symbolTable, expression.Expression);
                break;

            default: throw new NotImplementedException();
            }
        }

        private static void Declare(SymbolTable symbolTable, Expression expression)
        {
            expression.Scope = symbolTable.CurrentScope;

            switch (expression)
            {
            case IntLiteralExpression intLiteral: 
            case IdentifierExpression identifier:
                // Nothing to declare, leaf nodes
                break;

            case ProcExpression proc:
                // Processes introduce a scope for their signature
                symbolTable.PushScope();
                // Declare in parameters
                foreach (var param in proc.Parameters) Declare(symbolTable, param.Type);
                // Declare in return-type
                if (proc.ReturnType != null) Declare(symbolTable, proc.ReturnType);
                // Declare in body
                Declare(symbolTable, proc.Body);
                symbolTable.PopScope();
                break;

            case BlockExpression block:
                // Blocks introduce a scope
                symbolTable.PushScope();
                // Declare in each statement
                foreach (var stmt in block.Statements) Declare(symbolTable, stmt);
                // In return value too
                if (block.Value != null) Declare(symbolTable, block.Value);
                symbolTable.PopScope();
                break;

            default: throw new NotImplementedException();
            }
        }
    }
}
