using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Reporting.Info
{
    /// <summary>
    /// Some hint for the <see cref="Diagnostic"/>.
    /// </summary>
    public class FootnoteDiagnosticInfo : IDiagnosticInfo
    {
        /// <summary>
        /// The hint message.
        /// </summary>
        public string Message { get; set; }
    }
}
