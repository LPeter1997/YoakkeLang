using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir.Values;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Compile
{
    /// <summary>
    /// The dependency system that incrementally compiles the result.
    /// </summary>
    public class DependencySystem : IDependencySystem
    {
        public Value Evaluate(Expression expression)
        {
            throw new NotImplementedException();
        }

        public Semantic.Type EvaluateToType(Expression expression)
        {
            throw new NotImplementedException();
        }
    }
}
