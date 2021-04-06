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
    }
}
