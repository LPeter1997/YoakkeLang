using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Dependency;
using Yoakke.Text;

namespace Yoakke.Compiler.Services
{
    /// <summary>
    /// Service definition for compiler inputs.
    /// </summary>
    [InputQueryGroup]
    public partial interface IInputService
    {
        /// <summary>
        /// Retrieves the source text for a given file path.
        /// </summary>
        /// <param name="path">The file path to retrieve source text for.</param>
        /// <returns>The source text associated with the given path.</returns>
        public SourceText SourceText(string path);
    }
}
