using System.Collections.Generic;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Semantic
{
    /// <summary>
    /// Utility to collect local variable references.
    /// </summary>
    public class CollectLocalRefs : Visitor<object>
    {
        private SymbolTable symbolTable;

        private Scope borderScope;
        private HashSet<Symbol.Var> symbols = new HashSet<Symbol.Var>();

        private CollectLocalRefs(SymbolTable symbolTable, Scope borderScope)
        {
            this.symbolTable = symbolTable;
            this.borderScope = borderScope;
        }

        /// <summary>
        /// Collects all local variable references inside an <see cref="Expression"/>.
        /// </summary>
        /// <param name="symbolTable">The <see cref="SymbolTable"/> that contains symbol information.</param>
        /// <param name="expr">The <see cref="Expression"/> to collect the local references in.</param>
        /// <returns>The set of <see cref="Symbol.Var"/>s referenced locally to the expression.</returns>
        public static ISet<Symbol.Var> Collect(SymbolTable symbolTable, Expression expr)
        {
            var borderScope = symbolTable.ContainingScope(expr);
            var collector = new CollectLocalRefs(symbolTable, borderScope);
            collector.Visit(expr);
            return collector.symbols;
        }

        protected override object? Visit(Expression.Identifier ident)
        {
            var sym = symbolTable.ReferredSymbol(ident);
            if (sym is Symbol.Var varSym && IsCrossReference(varSym)) symbols.Add(varSym);
            return null;
        }

        private bool IsCrossReference(Symbol.Var symbol)
        {
            if (symbol.Definition == null) return false;
            var defScope = symbolTable.ContainingScope(symbol.Definition);
            if (IsSubScopeOfBorder(defScope)) return false;
            return true;
        }

        private bool IsSubScopeOfBorder(Scope scope)
        {
            while (true)
            {
                if (scope == borderScope) return true;
                if (scope.Parent == null) return false;
                scope = scope.Parent;
            }
        }
    }
}
