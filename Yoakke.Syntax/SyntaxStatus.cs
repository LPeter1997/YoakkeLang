using System.Collections.Generic;
using Yoakke.Syntax.Error;

namespace Yoakke.Syntax
{
    /// <summary>
    /// Status of syntax analysis, like errors.
    /// </summary>
    public class SyntaxStatus
    {
        /// <summary>
        /// The reported errors.
        /// </summary>
        public readonly IList<ISyntaxError> Errors = new List<ISyntaxError>();

        public void Report(ISyntaxError error) => Errors.Add(error);
    }
}
