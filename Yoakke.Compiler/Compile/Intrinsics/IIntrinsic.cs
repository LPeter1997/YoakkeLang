using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir.Types;
using Yoakke.Lir.Values;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Compile.Intrinsics
{
    /// <summary>
    /// Represents some compiler intrinsic value.
    /// </summary>
    public interface IIntrinsic
    {
        /// <summary>
        /// Evaluates the intrinsic to a <see cref="Value"/>.
        /// </summary>
        /// <param name="system">The <see cref="IDependencySystem"/>.</param>
        /// <param name="args">The arguments passed in for the intrinsic.</param>
        /// <returns>The <see cref="Value"/> the intrinsic evaluates to using the given arguments.</returns>
        public Value Evaluate(IDependencySystem system, IList<Expression> args);
    }
}
