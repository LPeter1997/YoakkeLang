﻿using System.Collections.Generic;
using Yoakke.Reporting.Info;

namespace Yoakke.Reporting
{
    /// <summary>
    /// A diagnostic that represents all user-facing information that we want to show in the console for example.
    /// </summary>
    public class Diagnostic
    {
        /// <summary>
        /// The <see cref="Severity"/> of the <see cref="Diagnostic"/>.
        /// </summary>
        public Severity? Severity { get; set; }
        /// <summary>
        /// The diagnosis identifier code, if there's any.
        /// </summary>
        public string? Code { get; set; }
        /// <summary>
        /// The summary message of the diagnosis.
        /// </summary>
        public string? Message { get; set; }
        /// <summary>
        /// The information list about this <see cref="Diagnostic"/>.
        /// </summary>
        public IList<IDiagnosticInfo> Information { get; set; } = new List<IDiagnosticInfo>();
    }
}
