using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Compiler.Symbols
{
    /// <summary>
    /// Interface for lexical scopes.
    /// </summary>
    public interface IScope
    {
        /// <summary>
        /// The parent scope of this one.
        /// </summary>
        public IScope? Parent { get; set; }
        /// <summary>
        /// The symbols defined inside this scope.
        /// </summary>
        public IReadOnlyDictionary<string, ISymbol> Symbols { get; }

        /// <summary>
        /// Tries to reference a symbol in this scope with a given name.
        /// </summary>
        /// <param name="name">The name of the symbol to search for.</param>
        /// <returns>The symbol, or null if it's not found.</returns>
        public ISymbol? Reference(string name);

        /// <summary>
        /// Tries to resolve a symbol from this scope using path parts.
        /// </summary>
        /// <param name="parts">The parts of the identifier chain.</param>
        /// <returns>The resolved symbol, if found.</returns>
        public ISymbol? Resolve(params string[] parts);
    }
}
