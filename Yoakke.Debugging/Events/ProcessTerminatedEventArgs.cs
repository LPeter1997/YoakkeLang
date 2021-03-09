using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Debugging.Events
{
    /// <summary>
    /// The event arguments for the process termination event.
    /// </summary>
    public class ProcessTerminatedEventArgs : EventArgs
    {
        /// <summary>
        /// The process that was terminated.
        /// </summary>
        public IProcess Process { get; set; }
        /// <summary>
        /// The exit code.
        /// </summary>
        public int ExitCode { get; set; }
    }
}
