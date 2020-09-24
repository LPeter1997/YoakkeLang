using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir
{
    // TODO: We could consider writing errors into an array and present that
    // That way we don't have to fix exceptions one-by-one

    /// <summary>
    /// An interface for everything that needs static validation.
    /// </summary>
    public interface IValidate
    {
        /// <summary>
        /// Validates the object.
        /// </summary>
        public void Validate();
    }
}
