using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic;
using Yoakke.Compiler.Symbols;
using Yoakke.Dependency;

namespace Yoakke.Compiler.Services
{
    /// <summary>
    /// Service for symbol access.
    /// </summary>
    [QueryGroup]
    public partial interface ISymbolService
    {
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
    }
}
