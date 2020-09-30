using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Compiler.Semantic
{
    /// <summary>
    /// Extra information about <see cref="Scope"/>s.
    /// </summary>
    [Flags]
    public enum ScopeTag
    {
        /// <summary>
        /// No extra information.
        /// </summary>
        None = 0,
        /// <summary>
        /// A <see cref="Scope"/> for a procedure.
        /// </summary>
        Proc = 1,
    }

    /// <summary>
    /// A single lexical scope containing <see cref="Symbol"/>s.
    /// </summary>
    public class Scope
    {
        /// <summary>
        /// The <see cref="ScopeTag"/> of this <see cref="Scope"/>.
        /// </summary>
        public readonly ScopeTag Tag;
        /// <summary>
        /// The parent of this <see cref="Scope"/>. Null, if this is the root.
        /// </summary>
        public readonly Scope? Parent;

        private Dictionary<string, Symbol> symbols = new Dictionary<string, Symbol>();

        /// <summary>
        /// Initializes a new <see cref="Scope"/>.
        /// </summary>
        /// <param name="tag">The <see cref="ScopeTag"/> for this scope.</param>
        /// <param name="parent">The parent scope of this scope.</param>
        public Scope(ScopeTag tag, Scope? parent)
        {
            Tag = tag;
            Parent = parent;
        }

        /// <summary>
        /// Searches for a <see cref="Symbol"/> in this or any parent <see cref="Scope"/>.
        /// </summary>
        /// <param name="name">The symbol name.</param>
        /// <returns>The referred <see cref="Symbol"/>.</returns>
        public Symbol Reference(string name)
        {
            if (symbols.TryGetValue(name, out var symbol)) return symbol;
            if (Parent != null) return Parent.Reference(name);
            // TODO: Error
            throw new NotImplementedException();
        }
    }
}
