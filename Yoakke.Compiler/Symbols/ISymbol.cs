using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Compiler.Symbols
{
    /// <summary>
    /// Interface for any symbol in the language.
    /// </summary>
    public interface ISymbol
    {
        /// <summary>
        /// The containing scope of the symbol.
        /// </summary>
        public IScope ContainingScope { get; }
        /// <summary>
        /// The name of the symbol.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The definition AST node of the symbol, if any.
        /// </summary>
        public Syntax.Ast.Node? Definition { get; }
    }
}
