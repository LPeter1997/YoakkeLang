using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Dependency.Internal
{
    /// <summary>
    /// A computed value's interface.
    /// </summary>
    public interface IDependencyValue
    {
        /// <summary>
        /// When last changed.
        /// </summary>
        public int ChangedAt { get; }
        /// <summary>
        /// When last verified that it can be re-used.
        /// </summary>
        public int VerifiedAt { get; }
        /// <summary>
        /// True, if this dependency does not require updating.
        /// </summary>
        public bool IsUpToDate { get; }
        /// <summary>
        /// Retrieves the stored value.
        /// </summary>
        public T GetValue<T>(DependencySystem system, [CallerMemberName] string memberName = "");
    }
}
