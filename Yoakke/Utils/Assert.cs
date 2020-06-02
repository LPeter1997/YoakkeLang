using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Yoakke.Utils
{
    /// <summary>
    /// Utilities for assertions.
    /// </summary>
    static class Assert
    {
        /// <summary>
        /// Makes sure that the argument is non-null.
        /// </summary>
        /// <typeparam name="T">The type of object to check.</typeparam>
        /// <param name="t">The value to to ensure to be non-null.</param>
        public static void NonNull<T>([NotNull] T t) => 
            _ = t ?? throw new ArgumentNullException(nameof(t));
    }
}
