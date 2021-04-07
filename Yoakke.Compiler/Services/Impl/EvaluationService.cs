using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Error;
using Yoakke.Dependency;
using Yoakke.Lir.Values;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Services.Impl
{
    internal class EvaluationService : IEvaluationService
    {
#pragma warning disable CS8618, CS0067
        public event EventHandler<ICompileError> OnError;
#pragma warning restore CS8618, CS0067

#pragma warning disable CS8618
        [QueryGroup]
        public IEvaluationService Evaluation { get; set; }
#pragma warning restore CS8618

        public Value Evaluate(Expression expression)
        {
            throw new NotImplementedException();
        }

        public Semantic.Types.Type EvaluateType(Expression expression)
        {
            var value = Evaluation.Evaluate(expression);
            // TODO: Invoke type translator
            throw new NotImplementedException();
        }
    }
}
