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
    public class ResolveSymbol : Visitor<object>
    {
        private SymbolTable symbolTable;

        /// <summary>
        /// Initializes a new <see cref="ResolveSymbol"/>.
        /// </summary>
        /// <param name="symbolTable">The <see cref="SymbolTable"/> to use.</param>
        public ResolveSymbol(SymbolTable symbolTable)
        {
            this.symbolTable = symbolTable;
        }

        /// <summary>
        /// Defines and resolves <see cref="Symbol"/>s for the given <see cref="Statement"/> and it's children.
        /// </summary>
        /// <param name="statement">The <see cref="Statement"/> to define and resolve inside.</param>
        public void Resolve(Statement statement) => Visit(statement);

        /// <summary>
        /// Defines and resolves <see cref="Symbol"/>s for the given <see cref="Expression"/> and it's children.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to define and resolve inside.</param>
        public void Resolve(Expression expression) => Visit(expression);

        protected override object? Visit(Statement.Var var)
        {
            base.Visit(var);
            symbolTable.DefineSymbol(var, new Symbol.Var(var));
            return null;
        }

        protected override object? Visit(Expression.Proc proc)
        {
            Visit(proc.Signature);
            // Declare each parameter
            foreach (var param in proc.Signature.Parameters)
            {
                if (param.Name == null) continue;

                symbolTable.DefineSymbol(param, new Symbol.Var(param));
            }
            Visit(proc.Body);
            return null;
        }

        protected override object? Visit(Expression.Identifier ident)
        {
            symbolTable.ReferSymbol(ident, ident.Name);
            return null;
        }
    }
}
