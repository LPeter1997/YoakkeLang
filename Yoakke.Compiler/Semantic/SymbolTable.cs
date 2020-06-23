using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.Compiler.Syntax;
using Yoakke.Compiler.Utils;

namespace Yoakke.Compiler.Semantic
{
    /// <summary>
    /// The structures that manages all of the symbols of a program.
    /// </summary>
    class SymbolTable
    {
        /// <summary>
        /// The global <see cref="Scope"/>.
        /// </summary>
        public Scope GlobalScope { get; }
        /// <summary>
        /// The current <see cref="Scope"/> (that's being worked on).
        /// </summary>
        public Scope CurrentScope { get; set; }

        public SymbolTable()
        {
            GlobalScope = new Scope(ScopeTag.None, null);
            CurrentScope = GlobalScope;
        }

        /// <summary>
        /// Defines a builtin <see cref="Type"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="Type"/>.</param>
        /// <param name="type">The <see cref="Type"/> itself to define.</param>
        public void DefineBuiltinType(string name, Type type)
        {
            var sym = new Symbol.Const(name, type);
            GlobalScope.Define(sym);
        }

        /// <summary>
        /// Defines an intrinsic function.
        /// </summary>
        /// <param name="name">The name of the intrinsic function.</param>
        /// <param name="type">The <see cref="Type"/> of the intrinsic function.</param>
        /// <param name="function">The called <see cref="Func{T, TResult}"/> to perform the action when called.</param>
        public void DefineIntrinsicFunction(string name, Type type, Func<List<Value>, Value> function)
        {
            var sym = new Symbol.Intrinsic(name, type, function);
            GlobalScope.Define(sym);
        }

        /// <summary>
        /// Pushes a new <see cref="Scope"/>, so the current scope will be the pushed one.
        /// </summary>
        /// <param name="tag">The <see cref="ScopeTag"/> for the created <see cref="Scope"/>.</param>
        public void PushScope(ScopeTag tag)
        {
            CurrentScope = new Scope(tag, CurrentScope);
        }

        /// <summary>
        /// Pops the current <see cref="Scope"/>, making the current one the parent of the current one.
        /// </summary>
        public void PopScope()
        {
            CurrentScope = Assert.NonNullValue(CurrentScope.Parent);
        }
    }

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
    /// A single, lexical scope of symbols.
    /// </summary>
    public class Scope
    {
        /// <summary>
        /// The <see cref="ScopeTag"/> for this <see cref="Scope"/>.
        /// </summary>
        public readonly ScopeTag Tag;
        /// <summary>
        /// The parent of this <see cref="Scope"/>. Null, if this is the root.
        /// </summary>
        public readonly Scope? Parent;

        /// <summary>
        /// The <see cref="Symbol"/>s defined in this <see cref="Scope"/>.
        /// </summary>
        public IEnumerable<Symbol> Symbols => symbols.Values;

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
        /// Defines a <see cref="Symbol"/> in this <see cref="Scope"/>.
        /// </summary>
        /// <param name="symbol">The <see cref="Symbol"/> to define.</param>
        public void Define(Symbol symbol)
        {
            symbols.Add(symbol.Name, symbol);
        }

        /// <summary>
        /// Searches for a <see cref="Symbol"/> in this or any parent <see cref="Scope"/>.
        /// </summary>
        /// <param name="name">The symbol-referrnig <see cref="Token"/>.</param>
        /// <returns>The referred <see cref="Symbol"/>.</returns>
        public Symbol Reference(Token name) =>
            ReferenceInternal(name.Value, name);

        /// <summary>
        /// Same as <see cref="Reference(Token)"/>, but only requires a string name.
        /// </summary>
        public Symbol Reference(string name) =>
            ReferenceInternal(name, null);

        private Symbol ReferenceInternal(string name, Token? ident)
        {
            if (symbols.TryGetValue(name, out var symbol))
            {
                return symbol;
            }
            if (Parent != null)
            {
                return Parent.ReferenceInternal(name, ident);
            }
            // Error out
            if (ident == null) throw new UndefinedSymbolError(name);
            throw new UndefinedSymbolError(ident.Value);
        }
    }
}
