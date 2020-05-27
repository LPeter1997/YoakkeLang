using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.Semantic
{
    using Type = Yoakke.Type.Type;

    abstract class Symbol
    {
        public string Name { get; }
        public Type? Type { get; set; }

        public Symbol(string name)
        {
            Name = name;
        }
    }

    class ConstSymbol : Symbol
    {
        public ConstSymbol(string name) 
            : base(name)
        {
        }
    }

    class Scope
    {
        public Scope? Parent { get; }

        private Dictionary<string, Symbol> symbols = new Dictionary<string, Symbol>();

        public Scope(Scope? parent)
        {
            Parent = parent;
        }

        public void Define(Symbol symbol)
        {
            symbols.Add(symbol.Name, symbol);
        }

        public Symbol Reference(string name)
        {
            if (symbols.TryGetValue(name, out var symbol))
            {
                return symbol;
            }
            if (Parent != null)
            {
                return Parent.Reference(name);
            }
            throw new NotImplementedException($"Undefined symbol {name}!");
        }
    }

    class SymbolTable
    {
        public Scope GlobalScope { get; }
        public Scope CurrentScope { get; private set; }

        public SymbolTable()
        {
            GlobalScope = new Scope(null);
            CurrentScope = GlobalScope;
        }

        public void PushScope()
        {
            CurrentScope = new Scope(CurrentScope);
        }

        public void PopScope()
        {
            if (CurrentScope.Parent == null)
            {
                throw new NotImplementedException();
            }
            CurrentScope = CurrentScope.Parent;
        }
    }
}
