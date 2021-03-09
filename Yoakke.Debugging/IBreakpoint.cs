using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Debugging
{
    /// <summary>
    /// Interface for a breakpoint.
    /// </summary>
    public interface IBreakpoint
    {
        /// <summary>
        /// The address of this breakpoint.
        /// </summary>
        public ulong Address { get; }

        /// <summary>
        /// True, if this breakpoint is set.
        /// </summary>
        public bool IsSet { get; set; }
    }
}
