using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Error;
using Yoakke.Compiler.Services;
using Yoakke.Compiler.Symbols;
using Yoakke.Compiler.Symbols.Impl;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Internal
{
    internal static class SymbolResolution
    {
        // Define and assign scope for AST nodes
        private class DefineScope : Visitor<object>
        {
            private SymbolTable symbolTable;
            private Action<ICompileError> onError;

            public DefineScope(SymbolTable symbolTable, Action<ICompileError> onError)
            {
                this.symbolTable = symbolTable;
                this.onError = onError;
            }

            public void Define(Node node) => Visit(node);

            // Attach current scope
            // TODO: Simplify with custom visitor that could call a common function?

            protected override object? Visit(Declaration declaration)
            {
                symbolTable.AssociateScope(declaration);
                return base.Visit(declaration);
            }

            protected override object? Visit(Statement statement)
            {
                // TODO: A better visitor could also avoid duplicates
                if (!(statement is Declaration)) symbolTable.AssociateScope(statement);
                return base.Visit(statement);
            }

            protected override object? Visit(Expression expression)
            {
                symbolTable.AssociateScope(expression);
                return base.Visit(expression);
            }

            protected override object? Visit(Expression.ProcSignature.Parameter param)
            {
                symbolTable.AssociateScope(param);
                return base.Visit(param);
            }

            protected override object? Visit(Expression.StructType.Field field)
            {
                symbolTable.AssociateScope(field);
                return base.Visit(field);
            }

            protected override object? Visit(Expression.StructValue.Field field)
            {
                symbolTable.AssociateScope(field);
                return base.Visit(field);
            }

            // Define scopes

            protected override object? Visit(Expression.StructType sty)
            {
                symbolTable.PushScope(parent => new Scope.Struct(parent));
                base.Visit(sty);
                symbolTable.PopScope();
                return null;
            }

            protected override object? Visit(Expression.Proc proc)
            {
                symbolTable.PushScope(parent => new Scope.Local(parent));
                base.Visit(proc);
                symbolTable.PopScope();
                return null;
            }

            protected override object? Visit(Expression.Block block)
            {
                symbolTable.PushScope(parent => new Scope.Local(parent));
                base.Visit(block);
                symbolTable.PopScope();
                return null;
            }
        }

        // Declare order-independent things
        private class DeclareSymbol : Visitor<object>
        {
            private IEvaluationService evalService;
            private ITypeService typeService;
            private SymbolTable symbolTable;
            private Action<ICompileError> onError;

            public DeclareSymbol(IEvaluationService evalService, ITypeService typeService, SymbolTable symbolTable, Action<ICompileError> onError)
            {
                this.evalService = evalService;
                this.typeService = typeService;
                this.symbolTable = symbolTable;
                this.onError = onError;
        }

            public void Declare(Node node) => Visit(node);

            protected override object? Visit(Declaration.Const cons)
            {
                base.Visit(cons);
                var symbol = new Symbol.Const(evalService, typeService, symbolTable, cons);
                var scope = (Scope)symbol.ContainingScope;
                if (!scope.TryDefine(symbol))
                {
                    var alreadyDefined = scope.Symbols[symbol.Name];
                    // TODO
                    throw new NotImplementedException("Could not define symbol, probably redefinition");
                }
                else
                {
                    symbolTable.AssociateSymbol(cons, symbol);
                }
                return null;
            }

            protected override object? Visit(Expression.ProcSignature.Parameter param)
            {
                base.Visit(param);
                var symbol = new Symbol.LocalVar(evalService, symbolTable, param);
                var scope = (Scope)symbol.ContainingScope;
                if (!scope.TryDefine(symbol))
                {
                    var alreadyDefined = scope.Symbols[symbol.Name];
                    // TODO
                    throw new NotImplementedException("Could not define symbol, probably redefinition");
                }
                else
                {
                    symbolTable.AssociateSymbol(param, symbol);
                }
                return null;
            }
        }

        // Resolves symbol references, defines symvols for order-dependent things
        private class ResolveSymbol : Visitor<object>
        {
            private IEvaluationService evalService;
            private ITypeService typeService;
            private SymbolTable symbolTable;
            private Action<ICompileError> onError;

            public ResolveSymbol(IEvaluationService evalService, ITypeService typeService, SymbolTable symbolTable, Action<ICompileError> onError)
            {
                this.evalService = evalService;
                this.typeService = typeService;
                this.symbolTable = symbolTable;
                this.onError = onError;
            }

            public void Resolve(Node node) => Visit(node);

            protected override object? Visit(Statement.Var var)
            {
                base.Visit(var);
                var scope = (Scope)symbolTable.ContainingScope(var);
                var isLocal = scope is Scope.Local;
                Symbol symbol = isLocal
                    ? new Symbol.LocalVar(evalService, typeService, symbolTable, var)
                    : new Symbol.GlobalVar(evalService, typeService, symbolTable, var);
                if (!scope.TryDefine(symbol))
                {
                    var alreadyDefined = scope.Symbols[symbol.Name];
                    // TODO
                    throw new NotImplementedException("Could not define symbol, probably redefinition");
                }
                else
                {
                    symbolTable.AssociateSymbol(var, symbol);
                }
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
                var scope = symbolTable.ContainingScope(ident);
                var symbol = scope.Reference(ident.Name);
                if (symbol == null)
                {
                    if (ident.ParseTreeNode is Syntax.ParseTree.Expression.Literal lit)
                    {
                        onError(new UndefinedSymbolError(lit.Token));
                    }
                    else
                    {
                        onError(new UndefinedSymbolError(ident.Name));
                    }
                }
                else
                {
                    symbolTable.AssociateSymbol(ident, symbol);
                }
                return null;
            }
        }

        public static ISymbolTable Resolve(
            IEvaluationService evalService, 
            ITypeService typeService, 
            Node root,
            Action<ICompileError> onError)
        {
            var symbolTable = new SymbolTable();
            // We just do the steps in order
            new DefineScope(symbolTable, onError).Define(root);
            new DeclareSymbol(evalService, typeService, symbolTable, onError).Declare(root);
            new ResolveSymbol(evalService, typeService, symbolTable, onError).Resolve(root);
            return symbolTable;
        }
    }
}
