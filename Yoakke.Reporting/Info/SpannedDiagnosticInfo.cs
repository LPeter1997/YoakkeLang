using Yoakke.Text;

namespace Yoakke.Reporting.Info
{
    /// <summary>
    /// Information for a <see cref="Diagnostic"/> that has a <see cref="Span"/> associated with it.
    /// </summary>
    public class SpannedDiagnosticInfo : IDiagnosticInfo
    {
        /// <summary>
        /// The <see cref="Span"/> of the diagnosis place.
        /// </summary>
        public Span Span { get; set; }
        /// <summary>
        /// The message appended to the position.
        /// </summary>
        public string? Message { get; set; }
        /// <summary>
        /// The <see cref="Severity"/> of this information.
        /// </summary>
        public Severity? Severity { get; set; }
    }
}
