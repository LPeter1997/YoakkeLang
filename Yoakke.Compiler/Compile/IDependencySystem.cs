using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Compile
{
    /// <summary>
    /// The system that resolves dependencies between compile-time expressions and evaluation.
    /// </summary>
    public interface IDependencySystem
    {
        /// <summary>
        /// Compiles the given <see cref="Declaration.File"/> to an <see cref="Assembly"/>.
        /// </summary>
        /// <param name="file">The file node to compile.</param>
        /// <returns>The compiled <see cref="Assembly"/>.</returns>
        public Assembly Compile(Declaration.File file);

        /// <summary>
        /// Asks the dependency system to evaluate the given <see cref="Expression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to evaluate.</param>
        /// <returns>The evaluated <see cref="Lir.Values.Value"/>.</returns>
        public Lir.Values.Value Evaluate(Expression expression);

        /// <summary>
        /// Asks the dependency system to evaluate the given <see cref="Expression"/> as a type.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to evaluate.</param>
        /// <returns>The evaluated <see cref="Semantic.Type"/>.</returns>
        public Semantic.Type EvaluateToType(Expression expression);
    }
}
