using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic.Types;
using Yoakke.Dependency;

namespace Yoakke.Compiler.Services
{
    /// <summary>
    /// Service for typing.
    /// </summary>
    [QueryGroup]
    public partial interface ITypeService
    {
        /// <summary>
        /// Retrieves the type of a given expression.
        /// </summary>
        /// <param name="expression">The expression to get the type of.</param>
        /// <returns>The type of the expression.</returns>
        public Type TypeOf(Syntax.Ast.Expression expression);

        /// <summary>
        /// Checks if the given AST node is type-safe.
        /// </summary>
        /// <param name="node">The node to type-check.</param>
        /// <returns>True, if the node successfully passes type-checking.</returns>
        public bool IsTypeSafe(Syntax.Ast.Node node);

        // TODO: This is temporarily public, because the dependency codegen can't properly implement non-public members
        public Type ToSemanticType(Lir.Values.Value value);
        public Lir.Types.Type ToLirType(Type type);
    }
}
