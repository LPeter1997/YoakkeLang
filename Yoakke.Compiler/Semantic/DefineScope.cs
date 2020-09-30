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
        private Scope currentScope;

        /// <summary>
        /// Initializes a new <see cref="DefineScope"/>.
        /// </summary>
        /// <param name="symbolTable">The <see cref="SymbolTable"/> to use.</param>
        public DefineScope(SymbolTable symbolTable)
        {
            this.symbolTable = symbolTable;
            currentScope = symbolTable.GlobalScope;
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

        protected override object? Visit(Expression.StructType sty)
        {
            symbolTable.DefinedScope[sty] = PushScope(ScopeTag.None);
            base.Visit(sty);
            PopScope();
            return null;
        }

        protected override object? Visit(Expression.Proc proc)
        {
            symbolTable.DefinedScope[proc] = PushScope(ScopeTag.Proc);
            base.Visit(proc);
            PopScope();
            return null;
        }

        protected override object? Visit(Expression.Block block)
        {
            symbolTable.DefinedScope[block] = PushScope(ScopeTag.None);
            base.Visit(block);
            PopScope();
            return null;
        }

        private Scope PushScope(ScopeTag scopeTag)
        {
            currentScope = new Scope(scopeTag, currentScope);
            return currentScope;
        }

        private void PopScope()
        {
            Debug.Assert(currentScope.Parent != null);
            currentScope = currentScope.Parent;
        }
    }
}
