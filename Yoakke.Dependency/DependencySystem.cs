using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        /// <summary>
        /// The current revision we are at.
        /// </summary>
        internal int CurrentRevision { get; private set; } = 0;

        // Instantiated query groups
        private Dictionary<Type, (object Proxy, Action<int> Clear)> queryGroups = new Dictionary<Type, (object Proxy, Action<int> Clear)>();
        // Query groups that have query group instantiators registeres
        private Dictionary<Type, Func<object>> queryGroupInstantiators = new Dictionary<Type, Func<object>>();
        // Runtime "call-stack" for computed values
        private Stack<DerivedDependencyValue> valueStack = new Stack<DerivedDependencyValue>();
        
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
            if (queryGroups.TryGetValue(typeof(TBase), out var queryGroup)) return (TBase)queryGroup.Proxy;
            // Check if it has an instantiation function registered
            if (queryGroupInstantiators.Remove(typeof(TBase), out var instantiate))
            {
                // Create one with the registered function
                // NOTE: Function already registers it, no need to here
                return CreateQueryGroup<TBase>(instantiate).Proxy;
            }
            throw new KeyNotFoundException("The given query group was not registered. Did you ask by it's interface type?");
        }

        /// <summary>
        /// Clears the memoized values before a certain revision.
        /// </summary>
        /// <param name="before">The revision to clear values before (inclusive).</param>
        public void Clear(int before)
        {
            foreach (var (proxy, clear) in queryGroups.Values) clear(before);
        }

        /// <summary>
        /// Clears all memoized values.
        /// </summary>
        public void Clear() => Clear(int.MaxValue);

        /// <summary>
        /// Retrieves the next revision.
        /// </summary>
        internal int GetNextRevision() => ++CurrentRevision;

        /// <summary>
        /// Does cycle-detection.
        /// Throws an exception if a cycle is present.
        /// </summary>
        internal void DetectCycle(DerivedDependencyValue value)
        {
            if (valueStack.Contains(value))
            {
                throw new InvalidOperationException("Cycle detected!");
            }
        }

        /// <summary>
        /// Registers a dependency for the current computed value.
        /// </summary>
        internal void RegisterDependency(IDependencyValue value)
        {
            if (valueStack.TryPeek(out var top) && !top.Dependencies.Contains(value)) top.Dependencies.Add(value);
        }

        /// <summary>
        /// Pushes the value onto the call-stack.
        /// </summary>
        internal void PushDependency(DerivedDependencyValue value)
        {
            RegisterDependency(value);
            valueStack.Push(value);
        }

        /// <summary>
        /// Pops the value off the top of the call-stack.
        /// </summary>
        internal void PopDependency() => valueStack.Pop();

        private (TBase Proxy, Action<int> Clear) CreateQueryGroup<TBase>(Func<object> instantiate)
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
                // Fill up the implementations properties with other query group references that are required
                InitializeQueryGroupProperties(queryGroupImpl);
                return queryGroupProxy;
            }
        }

        private (TBase Proxy, Action<int> Clear) InstantiateProxy<TBase>(Type[] argTypes, object[] args)
        {
            var type = typeof(TBase);
            var proxyClass = type.GetNestedType("Proxy");
            var proxyCtor = proxyClass.GetConstructor(argTypes);
            var proxy = (TBase)proxyCtor.Invoke(args);
            var proxyClear = proxyClass.GetMethod("Clear", new Type[] { typeof(int) });
            Action<int> proxyClearLambda = proxyClear == null
                ? before => { }
                : before => proxyClear.Invoke(proxy, new object[] { before });
            return (proxy, proxyClearLambda);
        }

        private void InitializeQueryGroupProperties<TBase>(TBase impl)
        {
            var relevantProperties = impl.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(prop => prop.GetCustomAttribute<QueryGroupAttribute>() != null);
            var proxyGetter = typeof(DependencySystem).GetMethod(nameof(Get));
            foreach (var prop in relevantProperties)
            {
                var typedProxyGetter = proxyGetter.MakeGenericMethod(prop.PropertyType);
                prop.SetValue(impl, typedProxyGetter.Invoke(this, new object[] { }));
            }
        }
    }
}
