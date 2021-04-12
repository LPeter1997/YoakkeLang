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
        /// Calculates the edit distance between two strings.
        /// </summary>
        /// <param name="s1">The first string to compare.</param>
        /// <param name="s2">The second string to compare.</param>
        /// <returns>An integer denoting the edit distance. A smaller number indicates that the strings are closer. 
        /// 0 means the strings are identical.</returns>
        public delegate int EditDistanceDelegate(string s1, string s2);

        /// <summary>
        /// The parent scope of this one.
        /// </summary>
        public IScope? Parent { get; set; }
        /// <summary>
        /// The symbols defined inside this scope.
        /// </summary>
        public IReadOnlyDictionary<string, ISymbol> Symbols { get; }

        /// <summary>
        /// Tries to reference a symbol accessible in this scope with a given name.
        /// </summary>
        /// <param name="name">The name of the symbol to search for.</param>
        /// <returns>The symbol, or null if it's not found.</returns>
        public ISymbol? Reference(string name);

        /// <summary>
        /// References symbols accessible in this scope within a given edit distance.
        /// </summary>
        /// <param name="name">The name of the symbol to search for.</param>
        /// <param name="editDistance">The edit distance function to use.</param>
        /// <param name="threshold">The maximum threshold to allow.</param>
        /// <returns>All symbols accessible in this scope within a given edit distance.</returns>
        public IEnumerable<ISymbol> Reference(string name, EditDistanceDelegate editDistance, int threshold);

        /// <summary>
        /// Tries to resolve a symbol from this scope using path parts.
        /// </summary>
        /// <param name="parts">The parts of the identifier chain.</param>
        /// <returns>The resolved symbol, if found.</returns>
        public ISymbol? Resolve(params string[] parts);
    }
}
