using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Yoakke.Compiler.IR
{
    /// <summary>
    /// An interface that every IR pass must implement.
    /// </summary>
    interface IPass
    {
        /// <summary>
        /// Passes through the IR code.
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> of the IR to pass through.</param>
        /// <returns>True, if modifications were performed during the pass.</returns>
        bool Pass(Assembly assembly);
    }
}
