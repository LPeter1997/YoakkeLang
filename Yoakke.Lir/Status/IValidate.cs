namespace Yoakke.Lir.Status
{
    /// <summary>
    /// An interface for everything that needs static validation.
    /// </summary>
    public interface IValidate
    {
        /// <summary>
        /// Validates the object.
        /// </summary>
        /// <param name="status">The <see cref="BuildStatus"/> to report to.</param>
        public void Validate(BuildStatus status);
    }

    /// <summary>
    /// An error during validation.
    /// </summary>
    public class ValidationError : IBuildError
    {
        /// <summary>
        /// The object that failed validation.
        /// </summary>
        public readonly IValidate Subject;
        /// <summary>
        /// The resoning for the failure.
        /// </summary>
        public readonly string Message;

        public ValidationError(IValidate subject, string message)
        {
            Subject = subject;
            Message = message;
        }

        public string GetErrorMessage() => 
            $"Validation error: {Message}\nWhile validating:\n{Subject}";
    }
}
