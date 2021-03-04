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
        public IDependencyValue GetInput(object key)
        {
            if (values.TryGetValue(key, out var value)) return (InputDependencyValue)value;
            var newValue = new InputDependencyValue();
            values.Add(key, newValue);
            return newValue;
        }

        /// <summary>
        /// Sets the dependency value for an input query.
        /// </summary>
        public void SetInput(DependencySystem system, object key, object value)
        {
            var dependencyValue = GetInput(key);
            ((InputDependencyValue)dependencyValue).SetValue(system, value);
        }

        /// <summary>
        /// Gets the dependency value for a derived query asynchronously and with a cancellation token.
        /// </summary>
        public IDependencyValue GetDerived(object key, DerivedDependencyValue.ComputeValueAsyncCtDelegate recompute)
        {
            if (values.TryGetValue(key, out var value)) return (DerivedDependencyValue)value;
            var newValue = new DerivedDependencyValue(recompute);
            values.Add(key, newValue);
            return newValue;
        }

        /// <summary>
        /// Gets the dependency value for a derived query asynchronously.
        /// </summary>
        public IDependencyValue GetDerived(object key, DerivedDependencyValue.ComputeValueAsyncDelegate recompute) =>
            GetDerived(key, new DerivedDependencyValue.ComputeValueAsyncCtDelegate((sys, ct) => recompute(sys)));

        /// <summary>
        /// Gets the dependency value with a cancellation token.
        /// </summary>
        public IDependencyValue GetDerived(object key, DerivedDependencyValue.ComputeValueCtDelegate recompute) =>
            GetDerived(key, new DerivedDependencyValue.ComputeValueAsyncCtDelegate((sys, ct) => Task.FromResult(recompute(sys, ct))));

        /// <summary>
        /// Gets the dependency value with a cancellation token.
        /// </summary>
        public IDependencyValue GetDerived(object key, DerivedDependencyValue.ComputeValueDelegate recompute) =>
            GetDerived(key, new DerivedDependencyValue.ComputeValueAsyncCtDelegate((sys, ct) => Task.FromResult(recompute(sys))));
    }
}
