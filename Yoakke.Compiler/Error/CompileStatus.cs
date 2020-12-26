using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Compiler.Error
{
    // TODO: Doc
    public class CompileStatus
    {
        public readonly IList<ICompileError> Errors = new List<ICompileError>();

        public void Report(ICompileError error) => Errors.Add(error);
    }
}
