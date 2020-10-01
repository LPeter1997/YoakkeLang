using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Semantic
{
    /// <summary>
    /// A semantic step to define the <see cref="Scope"/>s for each AST node that opens a new lexical scope.
    /// </summary>
    public class DefineScope : Visitor<object>
    {
        private SymbolTable symbolTable;

        /// <summary>
        /// Initializes a new <see cref="DefineScope"/>.
        /// </summary>
        /// <param name="symbolTable">The <see cref="SymbolTable"/> to use.</param>
        public DefineScope(SymbolTable symbolTable)
        {
            this.symbolTable = symbolTable;
        }

        /// <summary>
        /// Defines <see cref="Scope"/>s for the given <see cref="Statement"/> and it's children.
        /// </summary>
        /// <param name="statement">The <see cref="Statement"/> to define inside.</param>
        public void Define(Statement statement) => Visit(statement);

        /// <summary>
        /// Defines <see cref="Scope"/>s for the given <see cref="Expression"/> and it's children.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to define inside.</param>
        public void Define(Expression expression) => Visit(expression);

        // Attach current scope

        protected override object? Visit(Declaration declaration)
        {
            symbolTable.AssignCurrentScope(declaration);
            return base.Visit(declaration);
        }

        protected override object? Visit(Statement statement)
        {
            symbolTable.AssignCurrentScope(statement);
            return base.Visit(statement);
        }

        protected override object? Visit(Expression expression)
        {
            symbolTable.AssignCurrentScope(expression);
            return base.Visit(expression);
        }

        // Define scopes

        protected override object? Visit(Expression.StructType sty)
        {
            symbolTable.PushScope(ScopeKind.Struct);
            base.Visit(sty);
            symbolTable.PopScope();
            return null;
        }

        protected override object? Visit(Expression.Proc proc)
        {
            symbolTable.PushScope(ScopeKind.Proc);
            base.Visit(proc);
            symbolTable.PopScope();
            return null;
        }

        protected override object? Visit(Expression.Block block)
        {
            symbolTable.PushScope(ScopeKind.None);
            base.Visit(block);
            symbolTable.PopScope();
            return null;
        }
    }
}
