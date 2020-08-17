using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Backend
{
    /// <summary>
    /// Interface for every IR backend, that compiles the IR to some other representation.
    /// </summary>
    public interface IBackend
    {
        /// <summary>
        /// Compiles the given <see cref="Assembly"/> to the backend's representation.
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> to compile.</param>
        /// <returns>The backend's code representation.</returns>
        public string Compile(Assembly assembly);
    }
}
