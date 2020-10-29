using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Compile
{
    // TODO: Doc whole thing
    public class CollectLocalRefs : Visitor<object>
    {
        public IDependencySystem System { get; }

        private Scope? borderScope;
        private HashSet<Symbol.Var> symbols = new HashSet<Symbol.Var>();

        public CollectLocalRefs(IDependencySystem system)
        {
            System = system;
        }

        public ISet<Symbol.Var> Collect(Expression expr)
        {
            borderScope = System.SymbolTable.ContainingScope(expr);
            symbols.Clear();
            Visit(expr);
            return symbols;
        }

        protected override object? Visit(Expression.Identifier ident)
        {
            var sym = System.SymbolTable.ReferredSymbol(ident);
            if (sym is Symbol.Var varSym && IsCrossReference(varSym)) symbols.Add(varSym);
            return null;
        }

        private bool IsCrossReference(Symbol.Var symbol)
        {
            if (symbol.Definition == null) return false;
            var defScope = System.SymbolTable.ContainingScope(symbol.Definition);
            if (IsSubScopeOfBorder(defScope)) return false;
            return true;
        }

        private bool IsSubScopeOfBorder(Scope scope)
        {
            Debug.Assert(borderScope != null);
            while (true)
            {
                if (scope == borderScope) return true;
                if (scope.Parent == null) return false;
                scope = scope.Parent;
            }
        }
    }
}
