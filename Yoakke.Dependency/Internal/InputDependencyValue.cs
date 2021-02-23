using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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

        public T GetValue<T>(DependencySystem system, [CallerMemberName] string memberName = "")
        {
            if (ChangedAt == -1)
            {
                throw new InvalidOperationException($"Tried to retrieve {memberName} before it was ever set!");
            }
            return (T)value;
        }

        /// <summary>
        /// Sets the stored value.
        /// </summary>
        public void SetValue(DependencySystem system, object value)
        {
            system.RegisterDependency(this);
            this.value = value;
            ChangedAt = system.GetNextRevision();
        }
    }
}
