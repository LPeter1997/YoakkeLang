using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Debugging.Events
{
    /// <summary>
    /// The event arguments for when a process was started.
    /// </summary>
    public class ProcessStartedEventArgs : EventArgs
    {
        /// <summary>
        /// The process that was started.
        /// </summary>
        public IProcess Process { get; set; }
    }
}
