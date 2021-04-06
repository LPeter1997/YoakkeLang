using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir.Values;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Services.Impl
{
    internal class EvaluationService : IEvaluationService
    {
        public Value Evaluate(Expression expression)
        {
            throw new NotImplementedException();
        }

        public Semantic.Types.Type EvaluateType(Expression expression)
        {
            throw new NotImplementedException();
        }
    }
}
