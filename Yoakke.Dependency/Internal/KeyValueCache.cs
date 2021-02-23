using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Dependency.Internal
{
    /// <summary>
    /// A cache type for associating dependency keys with dependency values.
    /// </summary>
    public class KeyValueCache
    {
        private Dictionary<object, IDependencyValue> values = new Dictionary<object, IDependencyValue>();

        /// <summary>
        /// Gets the dependency value for an input query.
        /// </summary>
        public InputDependencyValue GetInput(object key)
        {
            if (values.TryGetValue(key, out var value)) return (InputDependencyValue)value;
            var newValue = new InputDependencyValue();
            values.Add(key, newValue);
            return newValue;
        }

        /// <summary>
        /// Gets the dependency value for a derived query.
        /// </summary>
        public DerivedDependencyValue GetDerived(object key, Func<DependencySystem, object> recompute)
        {
            if (values.TryGetValue(key, out var value)) return (DerivedDependencyValue)value;
            var newValue = new DerivedDependencyValue(recompute);
            values.Add(key, newValue);
            return newValue;
        }
    }
}
