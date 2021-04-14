using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic.Types;
using Yoakke.Lir.Values;
using Yoakke.Syntax.Ast;
using Type = Yoakke.Compiler.Semantic.Types.Type;

namespace Yoakke.Compiler.Symbols.Impl
{
    internal class SymbolTable : ISymbolTable
    {
        public IScope GlobalScope { get; }
        public IScope CurrentScope { get; private set; }

        private Dictionary<Node, ISymbol> associatedSymbols = new Dictionary<Node, ISymbol>();
        private Dictionary<Node, IScope> containingScopes = new Dictionary<Node, IScope>();

        public SymbolTable()
        {
            GlobalScope = new Scope.Global();
            CurrentScope = GlobalScope;
            DefineBuiltins();
        }

        public void AssociateScope(Node node) => containingScopes.Add(node, CurrentScope);
        public void AssociateSymbol(Node node, ISymbol symbol) => associatedSymbols.Add(node, symbol);

        public void PushScope(Func<IScope, IScope> newScope) => CurrentScope = newScope(CurrentScope);
        public void PopScope()
        {
            Debug.Assert(CurrentScope.Parent != null);
            CurrentScope = CurrentScope.Parent;
        }

        public ISymbol AssociatedSymbol(Node node) => associatedSymbols[node];
        public IScope ContainingScope(Node node) => containingScopes[node];
        public ISymbol? Resolve(params string[] parts) => GlobalScope.Resolve(parts);

        private void DefineBuiltins()
        {
            DefineBuiltinType("i32", Type.I32);
        }

        private void DefineBuiltinType(string name, Type type) =>
            ((Scope)GlobalScope).TryDefine(new Symbol.Const(GlobalScope, name, Type.Type_, new Value.User(type)));
    }
}
