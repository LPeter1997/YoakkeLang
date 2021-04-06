using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic.Types;
using Yoakke.Dependency;
using Yoakke.Lir.Values;

namespace Yoakke.Compiler.Services
{
    /// <summary>
    /// Service for compile-time evaluation.
    /// </summary>
    [QueryGroup]
    public partial interface IEvaluationService
    {
        /// <summary>
        /// Compile-time evaluates the given expression.
        /// </summary>
        /// <param name="expression">The AST expression to evaluate.</param>
        /// <returns>The evaluated value.</returns>
        public Value Evaluate(Syntax.Ast.Expression expression);

        /// <summary>
        /// Compile-time evaluates the given expression to a type.
        /// </summary>
        /// <param name="expression">The AST expression to evaluate.</param>
        /// <returns>The evaluated value as a type.</returns>
        public Type EvaluateType(Syntax.Ast.Expression expression);
    }
}
