using System;
using System.Collections.Generic;
using System.Linq;
using Yoakke.Compiler.Compile;
using Yoakke.Compiler.Error;
using Yoakke.DataStructures;
using Yoakke.Syntax;

namespace Yoakke.Compiler.Semantic
{
    /// <summary>
    /// The kind of the <see cref="Scope"/>s.
    /// </summary>
    public enum ScopeKind
    {
        /// <summary>
        /// Nothing special.
        /// </summary>
        None,
        /// <summary>
        /// A <see cref="Scope"/> for a procedure.
        /// </summary>
        Proc,
        /// <summary>
        /// A <see cref="Scope"/> for a structure.
        /// </summary>
        Struct,
    }

    /// <summary>
    /// A single lexical scope containing <see cref="Symbol"/>s.
    /// </summary>
    public class Scope
    {
        /// <summary>
        /// The <see cref="ScopeKind"/> of this <see cref="Scope"/>.
        /// </summary>
        public readonly ScopeKind Kind;
        /// <summary>
        /// The parent of this <see cref="Scope"/>. Null, if this is the root.
        /// </summary>
        public readonly Scope? Parent;
        /// <summary>
        /// The defined <see cref="Symbol"/>s in this <see cref="Scope"/>.
        /// </summary>
        public IEnumerable<Symbol> Symbols => symbols.Values;

        private Dictionary<string, Symbol> symbols = new Dictionary<string, Symbol>();

        /// <summary>
        /// Initializes a new <see cref="Scope"/>.
        /// </summary>
        /// <param name="kind">The <see cref="ScopeKind"/> for this scope.</param>
        /// <param name="parent">The parent scope of this scope.</param>
        public Scope(ScopeKind kind, Scope? parent)
        {
            Kind = kind;
            Parent = parent;
        }

        /// <summary>
        /// Defines a <see cref="Symbol"/> in this <see cref="Scope"/>.
        /// </summary>
        /// <param name="symbol">The <see cref="Symbol"/> to define.</param>
        public void Define(Symbol symbol) => symbols.Add(symbol.Name, symbol);

        /// <summary>
        /// Searches for a <see cref="Symbol"/> in this or any parent <see cref="Scope"/>.
        /// </summary>
        /// <param name="name">The symbol name reference <see cref="Token"/>.</param>
        /// <param name="system">The <see cref="IDependencySystem"/> to report errors to.</param>
        /// <returns>The referred <see cref="Symbol"/>.</returns>
        public Symbol Reference(Token name, IDependencySystem system)
        {
            return ReferenceImpl(name.Value, name, system);
        }

        /// <summary>
        /// Searches for a <see cref="Symbol"/> in this or any parent <see cref="Scope"/>.
        /// </summary>
        /// <param name="name">The symbol name.</param>
        /// <param name="system">The <see cref="IDependencySystem"/> to report errors to.</param>
        /// <returns>The referred <see cref="Symbol"/>.</returns>
        public Symbol Reference(string name, IDependencySystem system)
        {
            return ReferenceImpl(name, null, system);
        }

        /// <summary>
        /// Retrieves the innermost ancestor <see cref="Scope"/> with any of the given <see cref="ScopeKind"/>s.
        /// </summary>
        /// <param name="kinds">The <see cref="ScopeKind"/>s to look for.</param>
        /// <returns>The innermost <see cref="Scope"/> with one of the given <see cref="ScopeKind"/>s, or null if 
        /// there was none.</returns>
        public Scope? AncestorWithKind(params ScopeKind[] kinds)
        {
            if (kinds.Contains(Kind)) return this;
            if (Parent != null) return Parent.AncestorWithKind(kinds);
            return null;
        }

        private Symbol ReferenceImpl(string name, Token? reference, IDependencySystem system)
        {
            var symbol = ReferenceRecursive(name);
            if (symbol != null) return symbol;
            // No such symbol, find all similar names with an OSA-distance of less than 2
            var similarlyNamedSymbol = CollectSimilarlyNamed(name, StringMetrics.OptimalStringAlignmentDistance, 2);
            // Report
            var diag = reference == null ? new UndefinedSymbolError(name) : new UndefinedSymbolError(reference);
            diag.SimilarExistingNames = similarlyNamedSymbol.Select(sym => sym.Name);
            system.Status.Report(diag);
            // Return a dummy symbol
            return new Symbol.Var(null, name, Symbol.VarKind.Local);
        }

        private Symbol? ReferenceRecursive(string name)
        {
            if (symbols.TryGetValue(name, out var symbol)) return symbol;
            if (Parent != null) return Parent.ReferenceRecursive(name);
            return null;
        }

        private IEnumerable<Symbol> CollectSimilarlyNamed(string name, Func<string, string, int> metric, int threshold)
        {
            foreach (var sym in symbols.Values.Where(sym => metric(name, sym.Name) <= threshold))
            {
                yield return sym;
            }
            if (Parent != null)
            {
                foreach (var sym in Parent.CollectSimilarlyNamed(name, metric, threshold))
                {
                    yield return sym;
                }
            }
        }
    }
}
