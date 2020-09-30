using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Semantic
{
    /// <summary>
    /// A semantic step to define and reference order-dependent <see cref="Symbol"/>s.
    /// </summary>
    public class DefineSymbol : Visitor<object>
    {
        private SymbolTable symbolTable;

        /// <summary>
        /// Initializes a new <see cref="DefineSymbol"/>.
        /// </summary>
        /// <param name="symbolTable">The <see cref="SymbolTable"/> to use.</param>
        public DefineSymbol(SymbolTable symbolTable)
        {
            this.symbolTable = symbolTable;
        }

        /// <summary>
        /// Defines and resolves <see cref="Symbol"/>s for the given <see cref="Statement"/> and it's children.
        /// </summary>
        /// <param name="statement">The <see cref="Statement"/> to define and resolve inside.</param>
        public void Define(Statement statement) => Visit(statement);

        /// <summary>
        /// Defines and resolves <see cref="Symbol"/>s for the given <see cref="Expression"/> and it's children.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to define and resolve inside.</param>
        public void Define(Expression expression) => Visit(expression);

        protected override object? Visit(Statement.Var var)
        {
            base.Visit(var);

            var scope = symbolTable.ContainingScope[var];
            var symbol = new Symbol.Var(var);
            scope.Define(symbol);
            symbolTable.DefinedSymbol[var] = symbol;

            return null;
        }

        protected override object? Visit(Expression.Proc proc)
        {
            Visit(proc.Signature);
            // Declare each parameter
            foreach (var param in proc.Signature.Parameters)
            {
                if (param.Name == null) continue;

                var scope = symbolTable.ContainingScope[param];
                var symbol = new Symbol.Var(param);
                scope.Define(symbol);
                symbolTable.DefinedSymbol[param] = symbol;
            }
            Visit(proc.Body);
            return null;
        }

        protected override object? Visit(Expression.Identifier ident)
        {
            var scope = symbolTable.ContainingScope[ident];
            var symbol = scope.Reference(ident.Name);
            symbolTable.ReferredSymbol[ident] = symbol;
            return null;
        }
    }
}
