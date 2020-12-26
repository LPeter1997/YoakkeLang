using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Semantic
{
    /// <summary>
    /// Module for doing symbol resultion in the AST.
    /// </summary>
    public static class SymbolResolution
    {
        // Define and assign scope for AST nodes
        private class DefineScope : Visitor<object>
        {
            private SymbolTable symbolTable;

            public DefineScope(SymbolTable symbolTable)
            {
                this.symbolTable = symbolTable;
            }

            public void Define(Node node) => Visit(node);

            // Attach current scope

            protected override object? Visit(Declaration declaration)
            {
                symbolTable.AssignCurrentScope(declaration);
                return base.Visit(declaration);
            }

            protected override object? Visit(Statement statement)
            {
                symbolTable.AssignCurrentScope(statement);
                return base.Visit(statement);
            }

            protected override object? Visit(Expression expression)
            {
                symbolTable.AssignCurrentScope(expression);
                return base.Visit(expression);
            }

            protected override object? Visit(Expression.ProcSignature.Parameter param)
            {
                symbolTable.AssignCurrentScope(param);
                return base.Visit(param);
            }

            protected override object? Visit(Expression.StructType.Field field)
            {
                symbolTable.AssignCurrentScope(field);
                return base.Visit(field);
            }

            protected override object? Visit(Expression.StructValue.Field field)
            {
                symbolTable.AssignCurrentScope(field);
                return base.Visit(field);
            }

            // Define scopes

            protected override object? Visit(Expression.StructType sty)
            {
                symbolTable.PushScope(ScopeKind.Struct);
                base.Visit(sty);
                symbolTable.PopScope();
                return null;
            }

            protected override object? Visit(Expression.Proc proc)
            {
                symbolTable.PushScope(ScopeKind.Proc);
                base.Visit(proc);
                symbolTable.PopScope();
                return null;
            }

            protected override object? Visit(Expression.Block block)
            {
                symbolTable.PushScope(ScopeKind.None);
                base.Visit(block);
                symbolTable.PopScope();
                return null;
            }
        }

        // Declare order-independent things
        private class DeclareSymbol : Visitor<object>
        {
            private SymbolTable symbolTable;

            public DeclareSymbol(SymbolTable symbolTable)
            {
                this.symbolTable = symbolTable;
            }

            public void Declare(Node node) => Visit(node);

            protected override object? Visit(Declaration.Const cons)
            {
                base.Visit(cons);
                symbolTable.DefineSymbol(cons, new Symbol.Const(cons));
                return null;
            }

            protected override object? Visit(Expression.ProcSignature.Parameter param)
            {
                base.Visit(param);
                symbolTable.DefineSymbol(param, new Symbol.Var(param));
                return null;
            }
        }

        // Resolves symbol references, defines symvols for order-dependent things
        private class ResolveSymbol : Visitor<object>
        {
            private SymbolTable symbolTable;

            public ResolveSymbol(SymbolTable symbolTable)
            {
                this.symbolTable = symbolTable;
            }

            public void Resolve(Node node) => Visit(node);

            protected override object? Visit(Statement.Var var)
            {
                base.Visit(var);
                symbolTable.DefineSymbol(var, new Symbol.Var(var, symbolTable.IsGlobal(var)));
                return null;
            }

            protected override object? Visit(Expression.Proc proc)
            {
                Visit(proc.Signature);
                // NOTE: Parameters are already declared
                Visit(proc.Body);
                return null;
            }

            protected override object? Visit(Expression.Identifier ident)
            {
                symbolTable.ReferSymbol(ident, ident.Name);
                return null;
            }
        }

        /// <summary>
        /// Does symbol resolution for the given AST <see cref="Node"/>.
        /// </summary>
        /// <param name="symbolTable">The <see cref="SymbolTable"/> to use for resolution.</param>
        /// <param name="node">The <see cref="Node"/> to perform the resolution in.</param>
        public static void Resolve(SymbolTable symbolTable, Node node)
        {
            // We just do the steps in order
            new DefineScope(symbolTable).Define(node);
            new DeclareSymbol(symbolTable).Declare(node);
            new ResolveSymbol(symbolTable).Resolve(node);
        }
    }
}
