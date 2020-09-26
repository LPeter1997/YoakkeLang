using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Status
{
    /// <summary>
    /// A type to signal an accidental empty build.
    /// </summary>
    public class EmptyBuild : IBuildWarning
    {
        public string GetWarningMessage() => "Empty build!";
    }
}
