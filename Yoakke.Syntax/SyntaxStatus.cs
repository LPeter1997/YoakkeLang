using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
