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
    /// A value that came from setting an input.
    /// </summary>
    public class InputDependencyValue : IDependencyValue
    {
        private object value;

        public int ChangedAt { get; private set; } = -1;
        public int VerifiedAt => ChangedAt;

        public Task<T> GetValueAsync<T>(DependencySystem system, CancellationToken cancellationToken)
        {
            system.RegisterDependency(this);
            if (ChangedAt == -1)
            {
                throw new InvalidOperationException($"Tried to retrieve input value before it was ever set!");
            }
            return Task.FromResult((T)value);
        }

        /// <summary>
        /// Sets the stored value.
        /// </summary>
        public void SetValue(DependencySystem system, object value)
        {
            this.value = value;
            ChangedAt = system.GetNextRevision();
        }
    }
}
