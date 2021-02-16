using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Dependency
{
    /// <summary>
    /// A system that keeps track of query groups, dependencies and revisions.
    /// </summary>
    public class DependencySystem
    {
        // Instantiated query groups
        private Dictionary<Type, object> queryGroups = new Dictionary<Type, object>();
        // Query groups that have query group instantiators registeres
        private Dictionary<Type, Func<object>> queryGroupInstantiators = new Dictionary<Type, Func<object>>();

        /// <summary>
        /// Registers a query group to be managed by this <see cref="DependencySystem"/>.
        /// </summary>
        /// <typeparam name="TBase">The query groups interface type.</typeparam>
        /// <param name="instantiate">The instantiation function.</param>
        /// <returns>The <see cref="DependencySystem"/> itself.</returns>
        public DependencySystem Register<TBase>(Func<TBase> instantiate)
        {
            if (!Attribute.IsDefined(typeof(TBase), typeof(QueryGroupAttribute)))
            {
                throw new InvalidOperationException("All query groups must be tagged with the QueryGroup attribute!");
            }
            queryGroupInstantiators.Add(typeof(TBase), () => instantiate());
            return this;
        }

        /// <summary>
        /// Registers a default constructible query group to be managed by this <see cref="DependencySystem"/>.
        /// </summary>
        /// <typeparam name="TBase">The query groups interface type.</typeparam>
        /// <typeparam name="TDerived">The query groups implementation type.</typeparam>
        /// <returns>The <see cref="DependencySystem"/> itself.</returns>
        public DependencySystem Register<TBase, TDerived>() where TDerived : TBase, new() =>
            Register<TBase>(() => new TDerived());

        /// <summary>
        /// Registers an input query group to be managed by this <see cref="DependencySystem"/>.
        /// </summary>
        /// <typeparam name="TBase">The query groups interface type.</typeparam>
        /// <returns>The <see cref="DependencySystem"/> itself.</returns>
        public DependencySystem Register<TBase>() where TBase : IInputQueryGroup
        {
            // TODO: Somehow lead this back to the other case
            // We probably need reflection of some sort to read out a nested implementor we can instantiate
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves a registered query group.
        /// </summary>
        /// <typeparam name="TBase">The interface type that was used to register the query group.</typeparam>
        /// <returns>The registered query group.</returns>
        public TBase Get<TBase>()
        {
            // Check if it's already instantiated, if so, return it
            if (queryGroups.TryGetValue(typeof(TBase), out var queryGroup)) return (TBase)queryGroup;
            // Check if it has an instantiation function registered
            if (queryGroupInstantiators.Remove(typeof(TBase), out var instantiate))
            {
                // Create one with the registered function
                // NOTE: Function already registers it
                var newQueryGroup = CreateQueryGroup<TBase>(instantiate);
                // Return it
                return newQueryGroup;
            }
            throw new KeyNotFoundException("The given query group was not registered. Did you ask by it's interface type?");
        }

        private TBase CreateQueryGroup<TBase>(Func<object> instantiate)
        {
            // Instantiate using the function
            var queryGroup = (TBase)instantiate();
            // First we add it, so we can resolve for circular dependencies
            queryGroups.Add(typeof(TBase), queryGroup);
            // TODO: Fill in properties, if needed
            return queryGroup;
        }
    }
}
