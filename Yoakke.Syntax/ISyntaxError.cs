namespace Yoakke.Syntax
{
    /// <summary>
    /// Interface for syntactic errors.
    /// </summary>
    public interface ISyntaxError
    {
        /// <summary>
        /// Retrieves the error message for this syntax error.
        /// </summary>
        public string GetErrorMessage();
    }
}
