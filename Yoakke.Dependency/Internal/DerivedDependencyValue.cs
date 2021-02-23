using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Dependency.Internal
{
    /// <summary>
    /// A value that came from a computation.
    /// </summary>
    public class DerivedDependencyValue : IDependencyValue
    {
        internal IList<IDependencyValue> Dependencies { get; private set; } = new List<IDependencyValue>();

        private Func<DependencySystem, object> recompute;
        private object cachedValue;

        public int ChangedAt { get; private set; } = -1;
        public int VerifiedAt { get; private set; } = -1;

        public DerivedDependencyValue(Func<DependencySystem, object> recompute)
        {
            this.recompute = recompute;
        }

        public T GetValue<T>(DependencySystem system, [CallerMemberName] string memberName = "")
        {
            system.DetectCycle(this);
            if (ChangedAt != -1)
            {
                // Value is already memoized
                if (VerifiedAt == system.CurrentRevision)
                {
                    // We can just clone and return
                    return GetValueCloned<T>();
                }
                // The system has a later revision, let's see if this value is reusable
                if (Dependencies.All(dep => dep.ChangedAt <= VerifiedAt))
                {
                    // All dependencies came from earlier revisions, this one is still fine
                    // Update the verification, still just clone the memoized value
                    VerifiedAt = system.CurrentRevision;
                    return GetValueCloned<T>();
                }
                // We need to do a recomputation
                var newValue = recompute(system);
                if (newValue.Equals(cachedValue))
                {
                    // The new value is exactly same as the old one
                    // We will update verification again, as we don't want the dependee values to update unnecessarily
                    VerifiedAt = system.CurrentRevision;
                    // NOTE: The newValue is already a clone here, we can just return that
                    return (T)newValue;
                }
                // The new value is different
                cachedValue = newValue;
            }
            else
            {
                // Value not memoized yet
                // We push this value onto the computation or "call"-stack
                system.PushDependency(this);
                try
                {
                    // Now we do the recomputation
                    cachedValue = recompute(system);
                }
                finally
                {
                    // We are done with the computation, pop off
                    system.PopDependency();
                }
            }
            // We update both change and verification, as because of the recomputation we are definitely verified now
            // And we also have to note to the dependee values that we changed
            VerifiedAt = system.CurrentRevision;
            ChangedAt = system.CurrentRevision;
            return GetValueCloned<T>();
        }

        private T GetValueCloned<T>()
        {
            if (cachedValue is ICloneable cloneable) return (T)cloneable.Clone();
            else return (T)cachedValue;
        }
    }
}
