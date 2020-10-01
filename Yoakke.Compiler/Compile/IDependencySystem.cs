using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic;
using Yoakke.Lir.Values;
using Yoakke.Syntax;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Compile
{
    /// <summary>
    /// This is the system that resolves parts of the program during compilation.
    /// Since we are not fully typed while compiling, we need partial results which
    /// the implementors of this interface will provide.
    /// </summary>
    public interface IDependencySystem
    {
        /// <summary>
        /// Retrieves the <see cref="Symbol"/> that the given <see cref="Node"/> defined.
        /// </summary>
        /// <param name="node">The <see cref="Node"/> to get the <see cref="Symbol"/> for.</param>
        /// <returns>The <see cref="Symbol"/> the <see cref="Node"/> defined.</returns>
        public Symbol DefinedSymbolFor(Node node);

        /// <summary>
        /// Retrieves the <see cref="Symbol"/> that the given <see cref="Node"/> referred to.
        /// </summary>
        /// <param name="node">The <see cref="Node"/> to get the <see cref="Symbol"/> for.</param>
        /// <returns>The <see cref="Symbol"/> the <see cref="Node"/> referred to.</returns>
        public Symbol ReferredSymbolFor(Node node);

        /// <summary>
        /// Retrieves the semantic <see cref="Type"/> of an <see cref="Expression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to get the <see cref="Type"/> of.</param>
        /// <returns>The <see cref="Expression"/>s <see cref="Type"/>.</returns>
        public Type TypeOf(Expression expression);

        /// <summary>
        /// Typechecks the given <see cref="Statement"/>, meaning it validates correct type usage.
        /// </summary>
        /// <param name="statement">The <see cref="Statement"/> to type-check.</param>
        public void TypeCheck(Statement statement);

        /// <summary>
        /// Evaluates the given <see cref="Expression"/> as a <see cref="Value"/>.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to evaluate.</param>
        /// <returns>The evaluated <see cref="Value"/>.</returns>
        public Value Evaluate(Expression expression);

        /// <summary>
        /// Evaluates the given <see cref="Expression"/> as a <see cref="Type"/>.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to evaluate.</param>
        /// <returns>The evaluated value as a <see cref="Type"/>.</returns>
        public Type EvaluateToType(Expression expression);

        /// <summary>
        /// Translates a semantic <see cref="Type"/> to a <see cref="Lir.Types.Type"/>.
        /// </summary>
        /// <param name="type">The semantic <see cref="Type"/> to translate.</param>
        /// <returns>The translated <see cref="Lir.Types.Type"/>.</returns>
        public Lir.Types.Type TranslateToLirType(Type type);
    }
}
