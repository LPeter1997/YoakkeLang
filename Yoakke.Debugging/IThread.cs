using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Debugging
{
    /// <summary>
    /// Represents a debuggable thread.
    /// </summary>
    public interface IThread
    {
        /// <summary>
        /// The process this thread belongs to.
        /// </summary>
        public IProcess Process { get; }
    }
}
