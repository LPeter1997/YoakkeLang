using Yoakke.Reporting;

namespace Yoakke.Syntax.Error
{
    /// <summary>
    /// Interface for syntactic errors.
    /// </summary>
    public interface ISyntaxError
    {
        /// <summary>
        /// Creates a <see cref="Diagnostic"/> for this error.
        /// </summary>
        /// <returns>The <see cref="Diagnostic"/> representing this syntax error.</returns>
        public Diagnostic GetDiagnostic();
    }
}
