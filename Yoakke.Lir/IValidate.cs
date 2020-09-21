using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir
{
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
