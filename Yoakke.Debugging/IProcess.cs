using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Debugging
{
    /// <summary>
    /// Represents a debuggable process.
    /// </summary>
    public interface IProcess
    {
        /// <summary>
        /// The first thread that starts when the process is started.
        /// </summary>
        public IThread MainThread { get; }
    }
}
