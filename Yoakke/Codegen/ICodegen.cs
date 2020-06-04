using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.IR;

namespace Yoakke.Backend
{
    /// <summary>
    /// The interface for every code-generation backend that compiles the IR into some other representation.
    /// </summary>
    interface ICodegen
    {
        /// <summary>
        /// Compiles the given IR <see cref="Assembly"/> to another representation.
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> to compile.</param>
        /// <returns>Another representation of the IR code.</returns>
        string Compile(Assembly assembly);
    }
}
