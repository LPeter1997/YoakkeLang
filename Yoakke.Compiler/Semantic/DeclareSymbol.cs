using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Semantic
{
    /// <summary>
    /// A semantic step to declare order-independent <see cref="Symbol"/>s.
    /// </summary>
    public class DeclareSymbol : Visitor<object>
    {
        private SymbolTable symbolTable;

        /// <summary>
        /// Initializes a new <see cref="DeclareSymbol"/>.
        /// </summary>
        /// <param name="symbolTable">The <see cref="SymbolTable"/> to use.</param>
        public DeclareSymbol(SymbolTable symbolTable)
        {
            this.symbolTable = symbolTable;
        }

        /// <summary>
        /// Declares order-independent <see cref="Symbol"/>s for the given <see cref="Statement"/> ans it's children.
        /// </summary>
        /// <param name="statement">The <see cref="Statement"/> to declare inside.</param>
        public void Declare(Statement statement) => Visit(statement);

        /// <summary>
        /// Declares order-independent <see cref="Symbol"/>s for the given <see cref="Expression"/> ans it's children.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to declare inside.</param>
        public void Declare(Expression expression) => Visit(expression);

        protected override object? Visit(Declaration.Const cons)
        {
            base.Visit(cons);

            var scope = symbolTable.ContainingScope[cons];
            var symbol = new Symbol.Const(cons);
            scope.Define(symbol);
            symbolTable.DefinedSymbol[cons] = symbol;

            return null;
        }
    }
}
