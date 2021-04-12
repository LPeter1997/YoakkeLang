using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Compiler.Symbols
{
    /// <summary>
    /// Interface for symbol tables.
    /// </summary>
    public interface ISymbolTable
    {
        /// <summary>
        /// The global scope.
        /// </summary>
        public IScope GlobalScope { get; }

        /// <summary>
        /// Retrieves the scope associated with the AST node.
        /// </summary>
        /// <param name="node">The node to get the scope associated with.</param>
        /// <returns>The associated scope.</returns>
        public IScope ContainingScope(Syntax.Ast.Node node);

        /// <summary>
        /// Retrieves the symbol associated with the AST node.
        /// </summary>
        /// <param name="node">The node to get the symbol associated with.</param>
        /// <returns>The associated symbol with the node.</returns>
        public ISymbol AssociatedSymbol(Syntax.Ast.Node node);

        /// <summary>
        /// Tries to resolve a symbol from global scope using path parts.
        /// </summary>
        /// <param name="parts">The parts of the identifier chain.</param>
        /// <returns>The resolved symbol, if found.</returns>
        public ISymbol? Resolve(params string[] parts);
    }
}
