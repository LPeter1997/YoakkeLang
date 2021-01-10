using System;
using Yoakke.Reporting;

namespace Yoakke.Lir.Status
{
    /// <summary>
    /// An interface for everything that needs static validation.
    /// </summary>
    public interface IValidate
    {
        public void Validate(ValidationContext context);
    }

    /// <summary>
    /// An error during validation.
    /// </summary>
    public class ValidationError : IBuildError
    {
        /// <summary>
        /// The <see cref="ValidationContext"/> during validation.
        /// </summary>
        public readonly ValidationContext Context;
        /// <summary>
        /// The object that failed validation.
        /// </summary>
        public readonly IValidate Subject;
        /// <summary>
        /// The resoning for the failure.
        /// </summary>
        public readonly string Message;

        public ValidationError(ValidationContext context, IValidate subject, string message)
        {
            Context = context;
            Subject = subject;
            Message = message;
        }

        // TODO: Show IR code?
        public Diagnostic GetDiagnostic() => new Diagnostic
        {
            Severity = Severity.Error,
            Message = Message,
        };
    }
}
