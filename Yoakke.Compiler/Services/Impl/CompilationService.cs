using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Services.Impl
{
    internal class CompilationService : ICompilationService
    {
        public (UncheckedAssembly, Proc) GenerateEvaluation(Expression expression)
        {
            throw new NotImplementedException();
        }

        public UncheckedAssembly Compile(Declaration.File file)
        {
            throw new NotImplementedException();
        }
    }
}
