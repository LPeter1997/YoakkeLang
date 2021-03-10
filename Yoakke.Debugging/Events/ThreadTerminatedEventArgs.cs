using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Debugging.Events
{
    /// <summary>
    /// Event arguments for when a thread is terminated in a process.
    /// </summary>
    public class ThreadTerminatedEventArgs
    {
        /// <summary>
        /// The thread that was terminated.
        /// </summary>
        public IThread Thread { get; set; }
        /// <summary>
        /// The exit code of the terminated thread.
        /// </summary>
        public int ExitCode { get; set; }
    }
}
