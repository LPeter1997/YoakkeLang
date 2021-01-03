using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Reporting;
using Yoakke.Syntax.Error;

namespace Yoakke.Compiler.Error
{
    // TODO: Doc
    public class SyntaxError : ICompileError
    {
        public readonly ISyntaxError Error;

        public SyntaxError(ISyntaxError error)
        {
            Error = error;
        }

        public Diagnostic GetDiagnostic() => Error.GetDiagnostic();
    }
}
