using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Status
{
    // TODO: Doc
    public class ValidationContext
    {
        public delegate void ValidationErrorEventHandler(ValidationContext sender, ValidationError error);

        public event ValidationErrorEventHandler? ValidationError;

        // TODO: We need to keep some SourceFile representation thingy here of the assembly?

        public readonly UncheckedAssembly Assembly;

        public ValidationContext(UncheckedAssembly assembly)
        {
            Assembly = assembly;
        }

        internal void Report(ValidationError error) => ValidationError?.Invoke(this, error);
    }
}
