using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.Compiler
{
    /// <summary>
    /// Represents the base for every compile error.
    /// </summary>
    public abstract class CompileError : Exception
    {
        /// <summary>
        /// Dumps this error to the standard output.
        /// </summary>
        public abstract void Show();
    }
}
