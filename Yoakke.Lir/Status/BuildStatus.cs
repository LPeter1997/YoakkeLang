using System.Collections.Generic;
using Yoakke.Reporting;

namespace Yoakke.Lir.Status
{
    /// <summary>
    /// Interface for warnings.
    /// </summary>
    public interface IBuildWarning 
    {
        /// <summary>
        /// Retrieves the <see cref="Diagnostic"/> for this warning.
        /// </summary>
        public Diagnostic GetDiagnostic();
    }

    /// <summary>
    /// Interface for errors.
    /// </summary>
    public interface IBuildError 
    {
        /// <summary>
        /// Retrieves the <see cref="Diagnostic"/> for this error.
        /// </summary>
        public Diagnostic GetDiagnostic();
    }
}
