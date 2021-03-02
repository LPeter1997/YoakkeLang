using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
        /// Retrieves the stored value asynchronously with a cancellation token to cancel the operation.
        /// </summary>
        public Task<T> GetValueAsync<T>(DependencySystem system, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the stored value asynchronously.
        /// </summary>
        public Task<T> GetValueAsync<T>(DependencySystem system) => GetValueAsync<T>(system, CancellationToken.None);

        /// <summary>
        /// Retrieves the stored value synchronously with a cancellation token to cancel the operation.
        /// </summary>
        public T GetValue<T>(DependencySystem system, CancellationToken cancellationToken)
        {
            var task = GetValueAsync<T>(system, cancellationToken);
            task.Wait(cancellationToken);
            return task.Result;
        }

        /// <summary>
        /// Retrieves the stored value synchronously.
        /// </summary>
        public T GetValue<T>(DependencySystem system) => GetValue<T>(system, CancellationToken.None);
    }
}
