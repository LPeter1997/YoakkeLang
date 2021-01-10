using Yoakke.Reporting;

namespace Yoakke.Lir.Status
{
    /// <summary>
    /// A type to signal an accidental empty build.
    /// </summary>
    public class EmptyBuild : IBuildWarning
    {
        public Diagnostic GetDiagnostic() => new Diagnostic
        {
            Severity = Severity.Warning,
            Message = "The built assembly contains nothing!",
        };
    }
}
