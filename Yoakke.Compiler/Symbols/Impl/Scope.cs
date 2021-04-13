using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Compiler.Symbols.Impl
{
    internal abstract partial class Scope : IScope
    {
        public IScope? Parent { get; }
        public IReadOnlyDictionary<string, ISymbol> Symbols => symbols;

        private Dictionary<string, ISymbol> symbols = new Dictionary<string, ISymbol>();

        public Scope(IScope? parent)
        {
            Parent = parent;
        }

        public bool TryDefine(ISymbol symbol)
        {
            if (symbol.ContainingScope != this)
            {
                throw new ArgumentException("Scope of symbol does not match this scope!", nameof(symbol));
            }
            return symbols.TryAdd(symbol.Name, symbol);
        }

        public ISymbol? Reference(string name)
        {
            if (Symbols.TryGetValue(name, out var symbol)) return symbol;
            if (Parent == null) return null;
            return Parent.Reference(name);
        }

        public IEnumerable<ISymbol> Reference(string name, IScope.EditDistanceDelegate editDistance, int threshold)
        {
            foreach (var symbol in Symbols.Values.Where(sym => editDistance(name, sym.Name) <= threshold))
            {
                yield return symbol;
            }
            if (Parent != null)
            {
                foreach (var symbol in Parent.Reference(name, editDistance, threshold))
                {
                    yield return symbol;
                }
            }
        }

        public ISymbol? Resolve(params string[] parts)
        {
            Debug.Assert(parts.Length > 0);
            if (!Symbols.TryGetValue(parts[0], out var symbol)) return null;
            if (parts.Length == 1) return symbol;
            // TODO: If the symbol contains a type as a value that acts as a scope, it should be accessible!
            throw new NotImplementedException();
            //if (symbol.DefinedScope == null) return null;
            //return symbol.DefinedScope.Resolve(parts.Skip(1).ToArray());
        }
    }
}
