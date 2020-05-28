using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.Ast;

namespace Yoakke.Semantic.Steps
{
    class DeclareSymbols
    {
        public SymbolTable SymbolTable { get; }

        public DeclareSymbols(SymbolTable symbolTable)
        {
            SymbolTable = symbolTable;
        }

        public void Declare(Statement statement)
        {
            statement.Scope = SymbolTable.CurrentScope;

            switch (statement)
            {
            case ProgramDeclaration program:
                foreach (var decl in program.Declarations) Declare(decl);
                break;

            case ConstDefinition constDef:
            {
                Declare(constDef.Value);
                if (constDef.Type != null) Declare(constDef.Type);
                var symbol = new ConstSymbol(constDef.Name);
                SymbolTable.CurrentScope.Define(symbol);
            }  
            break;

            case ExpressionStatement expression:
                Declare(expression.Expression);
                break;

            default: throw new NotImplementedException();
            }
        }

        private void Declare(Expression expression)
        {
            expression.Scope = SymbolTable.CurrentScope;

            switch (expression)
            {
            case IntLiteralExpression intLiteral: 
            case IdentifierExpression identifier:
                break;

            case ProcExpression proc:
                foreach (var param in proc.Parameters) Declare(param.Type);
                if (proc.ReturnType != null) Declare(proc.ReturnType);
                SymbolTable.PushScope();
                Declare(proc.Body);
                SymbolTable.PopScope();
                break;

            case BlockExpression block:
                SymbolTable.PushScope();
                foreach (var stmt in block.Statements) Declare(stmt);
                if (block.Value != null) Declare(block.Value);
                SymbolTable.PopScope();
                break;

            default: throw new NotImplementedException();
            }
        }
    }
}
