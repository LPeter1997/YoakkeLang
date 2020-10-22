using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Compile;
using Yoakke.Compiler.Compile.Intrinsics;
using Yoakke.Lir.Values;
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
        public readonly Scope GlobalScope = new Scope(ScopeKind.None, null);
        /// <summary>
        /// The current <see cref="Scope"/> we are working in.
        /// </summary>
        public Scope CurrentScope { get; private set; }

        private readonly Dictionary<Node, Scope> containingScope = new Dictionary<Node, Scope>();
        private readonly Dictionary<Node, Symbol> definedSymbol = new Dictionary<Node, Symbol>();
        private readonly Dictionary<Node, Symbol> referredSymbol = new Dictionary<Node, Symbol>();

        /// <summary>
        /// Initializes a new <see cref="SymbolTable"/>.
        /// </summary>
        public SymbolTable()
        {
            CurrentScope = GlobalScope;
            DefineBuiltinPrimitives();
            DefineBuiltinIntrinsics();
        }

        private void DefineBuiltinPrimitives()
        {
            void DefineBuiltinType(Type type)
            {
                Debug.Assert(type is Type.Prim);
                this.DefineBuiltinType(type.ToString(), type);
            }

            DefineBuiltinType(Type.Type_);

            DefineBuiltinType(Type.I8);
            DefineBuiltinType(Type.I16);
            DefineBuiltinType(Type.I32);
            DefineBuiltinType(Type.I64);
            DefineBuiltinType(Type.U8);
            DefineBuiltinType(Type.U16);
            DefineBuiltinType(Type.U32);
            DefineBuiltinType(Type.U64);
            DefineBuiltinType(Type.Bool);
        }

        private void DefineBuiltinIntrinsics()
        {
            DefineBuiltin("@extern", new ExternIntrinsic());
        }

        /// <summary>
        /// Pushes a new <see cref="Scope"/> to be the current <see cref="Scope"/>.
        /// </summary>
        /// <param name="scopeTag">The <see cref="ScopeKind"/> of the new <see cref="Scope"/>.</param>
        public void PushScope(ScopeKind scopeKind)
        {
            CurrentScope = new Scope(scopeKind, CurrentScope);
        }

        /// <summary>
        /// Pops the current <see cref="Scope"/>, making it's parent the current one.
        /// </summary>
        public void PopScope()
        {
            Debug.Assert(CurrentScope.Parent != null);
            CurrentScope = CurrentScope.Parent;
        }

        /// <summary>
        /// Assigns the current <see cref="Scope"/> for the given <see cref="Node"/>.
        /// </summary>
        /// <param name="node">The <see cref="Node"/> to assign the current <see cref="Scope"/> for.</param>
        public void AssignCurrentScope(Node node)
        {
            containingScope[node] = CurrentScope;
        }

        // TODO: Doc
        public void DefineBuiltinType(string name, Type type) =>
            DefineBuiltin(name, Type.Type_, new Value.User(type));

        // TODO: Doc
        public void DefineBuiltin(string name, Type type, Value value) =>
            GlobalScope.Define(new Symbol.Const(name, type, value));

        // TODO: Doc
        public void DefineBuiltin(string name, IIntrinsic intr) =>
            GlobalScope.Define(new Symbol.Intrinsic(name, intr));

        /// <summary>
        /// Defines a <see cref="Symbol"/> for the given <see cref="Node"/>.
        /// </summary>
        /// <param name="node">The <see cref="Node"/> to define the <see cref="Symbol"/> for.</param>
        /// <param name="symbol">The <see cref="Symbol"/> to define.</param>
        public void DefineSymbol(Node node, Symbol symbol)
        {
            var scope = ContainingScope(node);
            scope.Define(symbol);
            definedSymbol.Add(node, symbol);
        }

        /// <summary>
        /// Refers a <see cref="Symbol"/> for the given <see cref="Node"/>.
        /// </summary>
        /// <param name="node">The <see cref="Node"/> to refer the <see cref="Symbol"/> for.</param>
        /// <param name="name">The name of the <see cref="Symbol"/> to refer to.</param>
        /// <returns>The referred <see cref="Symbol"/>.</returns>
        public Symbol ReferSymbol(Node node, string name)
        {
            var scope = ContainingScope(node);
            var symbol = scope.Reference(name);
            referredSymbol.Add(node, symbol);
            return symbol;
        }

        // TODO: Doc
        public Scope ContainingScope(Node node) => containingScope[node];

        // TODO: Doc
        public Symbol DefinedSymbol(Node node) => definedSymbol[node];

        // TODO: Doc
        public Symbol ReferredSymbol(Node node) => referredSymbol[node];

        // TODO: Doc
        public bool IsGlobal(Node node)
        {
            var scope = ContainingScope(node);
            var ancestor = scope.AncestorWithKind(ScopeKind.Proc, ScopeKind.Struct);
            return ancestor == null || ancestor.Kind == ScopeKind.Struct;
        }
    }
}
