using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Debugging.Events
{
    /// <summary>
    /// Arguments for when a process logged something in the debugger.
    /// </summary>
    public class DebugOutputEventArgs : EventArgs
    {
        /// <summary>
        /// The process that logged the string.
        /// </summary>
        public IProcess Process { get; set; }
        /// <summary>
        /// The logged message.
        /// </summary>
        public string Message { get; set; }
    }
}
