using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Reporting
{
    /// <summary>
    /// An <see cref="IDiagnosticRenderer"/> that prints the <see cref="Diagnostic"/> to console.
    /// </summary>
    public class ConsoleDiagnosticRenderer : IDiagnosticRenderer
    {
        public void Render(Diagnostic diagnostic)
        {
            RenderDiagnosticHead(diagnostic);
        }

        private void RenderDiagnosticHead(Diagnostic diagnostic)
        {
            if (diagnostic.Severity == null && diagnostic.Message == null) return;

            if (diagnostic.Severity != null)
            {
                Console.ForegroundColor = diagnostic.Severity.Color;
                Console.Write(diagnostic.Severity.Description);
                if (diagnostic.Code != null)
                {
                    Console.Write('[');
                    Console.Write(diagnostic.Code);
                    Console.Write(']');
                }
                Console.ResetColor();
                if (diagnostic.Message != null) Console.Write(": ");
            }

            if (diagnostic.Message != null) Console.Write(diagnostic.Message);

            Console.WriteLine();
        }
    }
}
