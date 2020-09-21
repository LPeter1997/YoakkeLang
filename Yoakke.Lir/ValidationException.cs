using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir
{
    /// <summary>
    /// An <see cref="Exception"/> related to <see cref="UncheckedAssembly"/> validation.
    /// </summary>
    public class ValidationException : Exception
    {
        /// <summary>
        /// The object that failed validation.
        /// </summary>
        public IValidate Subject { get; }

        public ValidationException(IValidate subject)
        {
            Subject = subject;
        }

        public ValidationException(IValidate subject, string message)
            : base(message)
        {
            Subject = subject;
        }

        public ValidationException(IValidate subject, string message, Exception innerException)
            : base(message, innerException)
        {
            Subject = subject;
        }
    }
}
