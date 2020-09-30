using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Syntax;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Semantic
{
    /// <summary>
    /// A context object to hold data for semantic analysis.
    /// </summary>
    public class SymbolTable
    {
        /// <summary>
        /// The global <see cref="Scope"/>.
        /// </summary>
        public readonly Scope GlobalScope = new Scope(ScopeTag.None, null);

        internal readonly IDictionary<Node, Scope> ContainingScope = new Dictionary<Node, Scope>();
        internal readonly IDictionary<Node, Symbol> DefinedSymbol = new Dictionary<Node, Symbol>();
        internal readonly IDictionary<Node, Symbol> ReferredSymbol = new Dictionary<Node, Symbol>();
    }
}
