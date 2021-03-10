using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Debugging.Events
{
    /// <summary>
    /// Event arguments for when a thread is started in a process.
    /// </summary>
    public class ThreadStartedEventArgs : EventArgs
    {
        /// <summary>
        /// The thread that was started.
        /// </summary>
        public IThread Thread { get; set; }
    }
}
