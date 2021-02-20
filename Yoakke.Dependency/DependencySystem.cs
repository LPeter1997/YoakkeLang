﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Dependency.Internal;

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
        // The revision we are at
        private int revisionCounter = 0;

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
            // NOTE: We don't use the other functions so we avoid the attribute checking, as these ones require
            // the InputQuery one
            queryGroupInstantiators.Add(typeof(TBase), () => default);
            return this;
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
                // NOTE: Function already registers it, no need to here
                return CreateQueryGroup<TBase>(instantiate);
            }
            throw new KeyNotFoundException("The given query group was not registered. Did you ask by it's interface type?");
        }

        /// <summary>
        /// Retrieves the next revision.
        /// </summary>
        internal int GetNextRevision() => revisionCounter++;

        private TBase CreateQueryGroup<TBase>(Func<object> instantiate)
        {
            if (typeof(TBase).IsAssignableTo(typeof(IInputQueryGroup)))
            {
                // This is an input query, only depends on the system
                var queryGroupProxy = InstantiateProxy<TBase>(
                    new Type[] { typeof(DependencySystem) },
                    new object[] { this });
                // Register the proxy
                queryGroups.Add(typeof(TBase), queryGroupProxy);
                return queryGroupProxy;
            }
            else
            {
                // Instantiate using the function
                var queryGroupImpl = (TBase)instantiate();
                // Instantiate the proxy
                var queryGroupProxy = InstantiateProxy<TBase>(
                    new Type[] { typeof(DependencySystem), typeof(TBase) },
                    new object[] { this, queryGroupImpl });
                // Register the proxy
                queryGroups.Add(typeof(TBase), queryGroupProxy);
                // TODO: Fill up dependent query properties of queryGroupImpl here?
                return queryGroupProxy;
            }
        }

        private TBase InstantiateProxy<TBase>(Type[] argTypes, object[] args)
        {
            var type = typeof(TBase);
            var proxyClass = type.GetNestedType("Proxy");
            var proxyCtor = proxyClass.GetConstructor(argTypes);
            return (TBase)proxyCtor.Invoke(args);
        }
    }
}
