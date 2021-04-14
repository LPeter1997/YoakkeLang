using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Dependency;
using Yoakke.Lir.Values;

namespace Yoakke.Compiler.Services
{
    /// <summary>
    /// Service definition for operations that require compilation or CTFE.
    /// </summary>
    [QueryGroup]
    public partial interface ICompilationService
    {
        /// <summary>
        /// Generates the LIR evaluation procedure for the given expression.
        /// </summary>
        /// <param name="expression">The expression to generate the evaluation procedure for.</param>
        /// <returns>The pair of unchecked LIR assembly and the evaluation procedure to invoke 
        /// to evaluate the expression.</returns>
        public (Lir.UncheckedAssembly, Lir.Proc) GenerateEvaluation(Syntax.Ast.Expression expression);

        /// <summary>
        /// Compiles the AST to an unchecked assembly.
        /// </summary>
        /// <param name="file">The top-level AST to compile.</param>
        /// <returns>The compiled unchecked LIR assembly.</returns>
        public Lir.UncheckedAssembly Compile(Syntax.Ast.Declaration.File file);
    }
}
