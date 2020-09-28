using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
