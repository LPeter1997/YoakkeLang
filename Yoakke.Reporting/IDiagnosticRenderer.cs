using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Reporting
{
    /// <summary>
    /// Anything that can visualize a <see cref="Diagnostic"/> to the user in some way.
    /// </summary>
    public interface IDiagnosticRenderer
    {
        /// <summary>
        /// Renders the given <see cref="Diagnostic"/> to the user.
        /// </summary>
        /// <param name="diagnostic">The <see cref="Diagnostic"/> to render.</param>
        public void Render(Diagnostic diagnostic);
    }
}
