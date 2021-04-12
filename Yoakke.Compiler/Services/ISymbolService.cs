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
        /// Resolves symbols for the given AST node.
        /// </summary>
        /// <param name="node">The node to resolve symbols for.</param>
        /// <returns>The symbol table representing symbol information for the given node.</returns>
        public ISymbolTable GetSymbolTable(Syntax.Ast.Node node);
    }
}
