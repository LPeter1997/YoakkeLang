using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.Compiler.Utils
{
    /// <summary>
    /// Interface for deep-cloning values.
    /// </summary>
    /// <typeparam name="T">The type the cloning function returns.</typeparam>
    interface ICloneable<T>
    {
        /// <summary>
        /// Clones the value.
        /// </summary>
        /// <returns>The cloned value.</returns>
        T Clone();
    }
}
