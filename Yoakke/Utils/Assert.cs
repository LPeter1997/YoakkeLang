using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Yoakke.Utils
{
    static class Assert
    {
        public static void NonNull<T>([NotNull] T t) => 
            _ = t ?? throw new ArgumentNullException(nameof(t));
    }
}
