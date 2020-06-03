using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Yoakke.Ast;
using Yoakke.Syntax;
using Yoakke.Utils;

namespace Yoakke.Semantic
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
        public Scope CurrentScope { get; private set; }

        public SymbolTable()
        {
            GlobalScope = new Scope(null);
            CurrentScope = GlobalScope;
        }

        /// <summary>
        /// Pushes a new <see cref="Scope"/>, so the current scope will be the pushed one.
        /// </summary>
        public void PushScope()
        {
            CurrentScope = new Scope(CurrentScope);
        }

        /// <summary>
        /// Pops the current <see cref="Scope"/>, making the current one the parent of the current one.
        /// </summary>
        public void PopScope()
        {
            Assert.NonNull(CurrentScope.Parent);
            CurrentScope = CurrentScope.Parent;
        }
    }

    /// <summary>
    /// A single, lexical scope of symbols.
    /// </summary>
    class Scope
    {
        /// <summary>
        /// The parent of this <see cref="Scope"/>. Null, if this is the root.
        /// </summary>
        public Scope? Parent { get; }

        private Dictionary<string, Symbol> symbols = new Dictionary<string, Symbol>();

        /// <summary>
        /// Initializes a new <see cref="Scope"/>.
        /// </summary>
        /// <param name="parent">The parent scope of this scope.</param>
        public Scope(Scope? parent)
        {
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

    /// <summary>
    /// The base for every kind of symbol in the program.
    /// </summary>
    abstract class Symbol
    {
        /// <summary>
        /// The name of the <see cref="Symbol"/>.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The <see cref="Position"/> of the <see cref="Symbol"/>, if any. Can be null, if this is a
        /// built-in symbol.
        /// </summary>
        public Position? Position { get; }

        /// <summary>
        /// Initializes a new <see cref="Symbol"/>.
        /// </summary>
        /// <param name="name">The name of this symbol.</param>
        public Symbol(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new <see cref="Symbol"/>.
        /// </summary>
        /// <param name="token">The defining <see cref="Token"/> of this symbol.</param>
        public Symbol(Token token)
            : this(token.Value)
        {
            Position = token.Position;
        }
    }

    /// <summary>
    /// A compile-time constant <see cref="Symbol"/>.
    /// </summary>
    class ConstSymbol : Symbol
    {
        /// <summary>
        /// The corresponding <see cref="ConstDefinition"/> that has created this <see cref="ConstSymbol"/>.
        /// </summary>
        public readonly ConstDefinition? Definition;
        /// <summary>
        /// The constant <see cref="Value"/> assigned to this <see cref="ConstSymbol"/>.
        /// </summary>
        public Value? Value { get; set; }

        public ConstSymbol(string name, Value value)
            : base(name)
        {
            Value = value;
        }

        public ConstSymbol(ConstDefinition definition) 
            : base(definition.Name)
        {
            Definition = definition;
        }
    }

    /// <summary>
    /// A user-defined variable <see cref="Symbol"/>. 
    /// </summary>
    class VariableSymbol : Symbol
    {
        public VariableSymbol(Token name)
            : base(name)
        {
        }
    }
}
