using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Reporting;

namespace Yoakke.Compiler.Error
{
    /// <summary>
    /// Interface for compile errors.
    /// </summary>
    public interface ICompileError
    {
        /// <summary>
        /// Creates a <see cref="Diagnostic"/> for this error.
        /// </summary>
        /// <returns>The <see cref="Diagnostic"/> representing this compile error.</returns>
        public Diagnostic GetDiagnostic();
    }
}
