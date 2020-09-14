using System.Collections.Generic;
using Yoakke.Compiler.IR.Passes;

namespace Yoakke.Compiler.IR
{
    /// <summary>
    /// Optimization step.
    /// </summary>
    static class Optimizer
    {
        /// <summary>
        /// Calls the given IR passes as long as they cause change in the IR.
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> to pass to the passes.</param>
        /// <param name="passes">The passes to perform.</param>
        public static void Optimize(Assembly assembly, IEnumerable<IPass> passes)
        {
            while (true)
            {
                bool changed = false;
                foreach (var pass in passes)
                {
                    bool passChanged = pass.Pass(assembly);
                    changed = changed || passChanged;
                }
                if (!changed) break;
            }
        }
    }
}
