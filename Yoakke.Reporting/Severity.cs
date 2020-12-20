using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Reporting
{
    /// <summary>
    /// A severity level for diagnostics.
    /// </summary>
    public class Severity
    {
        // Builtin severities
        
        public static readonly Severity Note = new Severity { Description = "note", Color = ConsoleColor.Green };
        public static readonly Severity Warning = new Severity { Description = "warning", Color = ConsoleColor.Yellow };
        public static readonly Severity Error = new Severity { Description = "error", Color = ConsoleColor.Red };
        public static readonly Severity InternalError = new Severity { Description = "internal compiler error", Color = ConsoleColor.Magenta };

        /// <summary>
        /// The textual representation of the severity level.
        /// </summary>
        public string Description { get; private set; }
        /// <summary>
        /// The color that the severity will be rendered as on console.
        /// </summary>
        public ConsoleColor Color { get; private set; }
    }
}
