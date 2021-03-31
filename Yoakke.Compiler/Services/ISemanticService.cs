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
    /// Service definition for semantic operations.
    /// </summary>
    [QueryGroup]
    public partial interface ISemanticService
    {
        /// <summary>
        /// Type-checks the given AST node.
        /// </summary>
        /// <param name="node">The AST node to check.</param>
        /// <returns>True, if the AST node was type-safe.</returns>
        public bool TypeCheck(Syntax.Ast.Node node);

        /// <summary>
        /// Evaluates the type of an expression AST node.
        /// </summary>
        /// <param name="expr">The expression to evaluate.</param>
        /// <returns>The type of the given expression.</returns>
        public Semantic.Types.Type TypeOf(Syntax.Ast.Expression expr);

        /// <summary>
        /// Evaluates the given expression compile-time.
        /// </summary>
        /// <param name="expression">The expression AST to evaluate.</param>
        /// <returns>The evaluated value object.</returns>
        public Value EvaluateValue(Syntax.Ast.Expression expression);

        /// <summary>
        /// Evaluates the given expression compile-time as type.
        /// </summary>
        /// <param name="expression">The expression AST to evaluate.</param>
        /// <returns>The evaluated type object.</returns>
        public Type EvaluateType(Syntax.Ast.Expression expression);
    }
}
