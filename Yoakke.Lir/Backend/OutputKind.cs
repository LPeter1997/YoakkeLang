using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Backend
{
    /// <summary>
    /// The output that should be produced.
    /// </summary>
    public enum OutputKind
    {
        Object,
        Executable,
        StaticLibrary,
        DynamicLibrary,
    }
}
