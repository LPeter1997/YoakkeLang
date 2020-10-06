using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic;
using Yoakke.Lir;
using Yoakke.Lir.Status;
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
        /// The <see cref="SymbolTable"/> of the system.
        /// </summary>
        public SymbolTable SymbolTable { get; }

        /// <summary>
        /// Compiles a given file into an <see cref="Assembly"/>.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="status">The <see cref="BuildStatus"/> to report to.</param>
        /// <returns>The compiled and verified <see cref="Assembly"/>, if there were no errors.</returns>
        public Assembly? Compile(Declaration.File file, BuildStatus status);

        /// <summary>
        /// Typechecks the given <see cref="Statement"/>, meaning it validates correct type usage.
        /// </summary>
        /// <param name="statement">The <see cref="Statement"/> to type-check.</param>
        public void TypeCheck(Statement statement);

        /// <summary>
        /// Retrieves the semantic <see cref="Type"/> of an <see cref="Expression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to get the <see cref="Type"/> of.</param>
        /// <returns>The <see cref="Expression"/>s <see cref="Type"/>.</returns>
        public Type TypeOf(Expression expression);

        /// <summary>
        /// Evaluates the given <see cref="Expression"/> as a <see cref="Value"/>.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to evaluate.</param>
        /// <returns>The evaluated <see cref="Value"/>.</returns>
        public Value Evaluate(Expression expression);

        /// <summary>
        /// Evaluates the given <see cref="Declaration.Const"/>, if it's not evaluated yet.
        /// </summary>
        /// <param name="constDecl">The <see cref="Declaration.Const"/> to evaluate, if not evaluated yet.</param>
        /// <returns>The evaluated <see cref="Value"/> of the constant declaration.</returns>
        public Value EvaluateConst(Declaration.Const constDecl);

        /// <summary>
        /// Evaluates the given <see cref="Symbol.Const"/>, if it's not evaluated yet.
        /// </summary>
        /// <param name="constSym">The <see cref="Symbol.Const"/> to evaluate, if not evaluated yet.</param>
        /// <returns>The evaluated <see cref="Value"/> of the constant symbol.</returns>
        public Value EvaluateConst(Symbol.Const constSym);

        /// <summary>
        /// Evaluates the given <see cref="Expression"/> as a <see cref="Type"/>.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to evaluate.</param>
        /// <returns>The evaluated value as a <see cref="Type"/>.</returns>
        public Type EvaluateType(Expression expression);

        /// <summary>
        /// Translates a semantic <see cref="Type"/> to a <see cref="Lir.Types.Type"/>.
        /// </summary>
        /// <param name="type">The semantic <see cref="Type"/> to translate.</param>
        /// <returns>The translated <see cref="Lir.Types.Type"/>.</returns>
        public Lir.Types.Type TranslateToLirType(Type type);
    }
}
