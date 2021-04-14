using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Error;
using Yoakke.Dependency;
using Yoakke.Lir.Runtime;
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

        [QueryGroup]
        public ICompilationService Compilation { get; set; }
#pragma warning restore CS8618

        public Value Evaluate(Expression expression)
        {
            var (uncheckedAsm, proc) = Compilation.GenerateEvaluation(expression);
            var asm = uncheckedAsm.Check();
            // TODO: Errors?
            var vm = new VirtualMachine(asm);
            var result = vm.Execute(proc, new Value[] { });
            return result;
        }

        public Semantic.Types.Type EvaluateType(Expression expression)
        {
            var value = Evaluation.Evaluate(expression);
            // TODO: Invoke type translator
            throw new NotImplementedException();
        }
    }
}
