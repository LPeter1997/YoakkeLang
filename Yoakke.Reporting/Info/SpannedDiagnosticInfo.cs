﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Text;

namespace Yoakke.Reporting.Info
{
    /// <summary>
    /// Information for a <see cref="Diagnostic"/> that has a <see cref="Span"/> associated with it.
    /// </summary>
    public class SpannedDiagnosticInfo
    {
        /// <summary>
        /// The <see cref="Span"/> of the diagnosis place.
        /// </summary>
        public Span Span { get; set; }
        /// <summary>
        /// The message appended to the position.
        /// </summary>
        public string? Message { get; set; }
    }
}
