using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.Compiler
{
    /// <summary>
    /// The types of output the compiler can produce.
    /// </summary>
    enum OutputType
    {
        /// <summary>
        /// Produce IR code.
        /// </summary>
        IR,
        /// <summary>
        /// Produce object code.
        /// </summary>
        Obj,
        /// <summary>
        /// Produce an executable.
        /// </summary>
        Exe,
    }
}
