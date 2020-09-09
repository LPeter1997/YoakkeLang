using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Backend.Backends.X86Family
{
    /// <summary>
    /// An interface for abstracting away intel and AT&T syntax.
    /// </summary>
    public interface IX86Syntax
    {
        /// <summary>
        /// Returns the intel representation.
        /// </summary>
        /// <returns>The string of the intel representation.</returns>
        public string ToIntelSyntax();
    }
}
