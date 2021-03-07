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

        public Revision ChangedAt { get; private set; } = Revision.Invalid;
        public Revision VerifiedAt => ChangedAt;

        public void Clear(Revision before)
        {
            if (ChangedAt <= before)
            {
                value = null;
                ChangedAt = Revision.Invalid;
            }
        }

        public Task<T> GetValueAsync<T>(DependencySystem system, CancellationToken cancellationToken)
        {
            system.RegisterDependency(this);
            if (ChangedAt == Revision.Invalid)
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
