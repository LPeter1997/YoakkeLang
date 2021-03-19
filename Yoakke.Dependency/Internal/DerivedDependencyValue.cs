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
    /// A value that came from a computation.
    /// </summary>
    public class DerivedDependencyValue : IDependencyValue
    {
        // Delegates for different computation types

        public delegate Task<object> ComputeValueAsyncCtDelegate(DependencySystem system, CancellationToken cancellationToken);
        public delegate Task<object> ComputeValueAsyncDelegate(DependencySystem system);
        public delegate object ComputeValueCtDelegate(DependencySystem system, CancellationToken cancellationToken);
        public delegate object ComputeValueDelegate(DependencySystem system);

        public static ComputeValueAsyncCtDelegate ToAsyncCtDelegate(ComputeValueAsyncCtDelegate f) => f;
        public static ComputeValueAsyncCtDelegate ToAsyncCtDelegate(ComputeValueAsyncDelegate f) => (sys, ct) => f(sys);
        public static ComputeValueAsyncCtDelegate ToAsyncCtDelegate(ComputeValueCtDelegate f) => (sys, ct) => Task.FromResult(f(sys, ct));
        public static ComputeValueAsyncCtDelegate ToAsyncCtDelegate(ComputeValueDelegate f) => (sys, ct) => Task.FromResult(f(sys));

        public static ComputeValueAsyncCtDelegate ToAsyncCtDelegate<T>(Func<DependencySystem, Task<T>> f) =>
            ToAsyncCtDelegate(new ComputeValueAsyncDelegate(sys => f(sys).ContinueWith(t => (object)t.Result)));
        public static ComputeValueAsyncCtDelegate ToAsyncCtDelegate<T>(Func<DependencySystem, CancellationToken, Task<T>> f) =>
            new ComputeValueAsyncCtDelegate((sys, ct) => f(sys, ct).ContinueWith(t => (object)t.Result));

        internal IList<IDependencyValue> Dependencies { get; private set; } = new List<IDependencyValue>();

        // Things for computation
        private ComputeValueAsyncCtDelegate recompute;
        private EventProxy[] eventProxies;
        // Storage
        private object cachedValue;
        private HashSet<(object Sender, object Args)>[] cachedEvents;
        // Temporaries for re-cacheing events
        private HashSet<(object Sender, object Args)>[] tempCachedEvents;
        // Temporaries for event handlers
        private Action[] eventHandlerUnsubscribers;

        public Revision ChangedAt { get; private set; } = Revision.Invalid;
        public Revision VerifiedAt { get; private set; } = Revision.Invalid;

        public DerivedDependencyValue(EventProxy[] eventProxies, ComputeValueAsyncCtDelegate recompute)
        {
            this.recompute = recompute;
            this.eventProxies = eventProxies;
            // Instantiate the temporary cache arrays
            this.cachedEvents = new HashSet<(object Sender, object Args)>[this.eventProxies.Length];
            this.tempCachedEvents = new HashSet<(object Sender, object Args)>[this.eventProxies.Length];
            this.eventHandlerUnsubscribers = new Action[this.eventProxies.Length];
            // The caches need to be filled
            for (int i = 0; i < this.eventProxies.Length; ++i)
            {
                this.cachedEvents[i] = new HashSet<(object Sender, object Args)>();
                this.tempCachedEvents[i] = new HashSet<(object Sender, object Args)>();
            }
        }

        public void Clear(Revision before)
        {
            if (VerifiedAt <= before)
            {
                cachedValue = null;
                ChangedAt = Revision.Invalid;
                VerifiedAt = Revision.Invalid;
                Dependencies.Clear();
            }
        }

        public async Task<T> GetValueAsync<T>(DependencySystem system, CancellationToken cancellationToken)
        {
            if (!system.AllowMemo)
            {
                // We disabled memoization, recompute
                // Recomputation takes care of emitting events
                // Since there is no memoization, we don't even record events
                return (T)(await recompute(system, cancellationToken));
            }
            else if (ChangedAt != Revision.Invalid)
            {
                // Value is already memoized
                if (VerifiedAt == system.CurrentRevision)
                {
                    // We can just clone and return
                    // Since there is no recomputation, we need to re-emit events manually
                    ResendEvents();
                    return GetValueCloned<T>();
                }
                // The system has a later revision, let's see if this value is reusable
                // NOTE: We need to get the value to the dependency to update it's statistics
                // and possibly participate in early-termination optimization
                var tasks = Dependencies.Select(dep => dep.GetValueAsync<object>(system, cancellationToken)).ToArray();
                // We need to wait for all tasks to finish
                Task.WaitAll(tasks, cancellationToken);
                // In case a cancellation is requested, we shouldn't continue from here
                if (cancellationToken.IsCancellationRequested) return default;
                // Now check wether dependencies have been updated since this one
                if (Dependencies.All(dep => dep.ChangedAt <= VerifiedAt))
                {
                    // All dependencies came from earlier revisions, this one is still fine
                    // Update the verification, still just clone the memoized value
                    VerifiedAt = system.CurrentRevision;
                    // Since there is no recomputation, we need to re-emit events manually
                    ResendEvents();
                    return GetValueCloned<T>();
                }
                // We need to do a recomputation
                // This will emit relevant events, no need to explicitly
                // But we need to capture them to compare them to the ones cached last time
                // We collect events into the temporary cache
                SubscribeToEvents(tempCachedEvents);
                var newValue = await recompute(system, cancellationToken);
                // We need to unsubscribe to avoid leaking
                UnsubscribeFromEvents();
                // In case a cancellation is requested, we shouldn't continue from here
                if (cancellationToken.IsCancellationRequested) return (T)newValue;
                // Check if we can do an early termination because of the old and new result maching
                if (newValue.Equals(cachedValue))
                {
                    // The new value is exactly same as the old one
                    // We still need to check if the cached events are the same
                    if (AreEventCachesEqual())
                    {
                        // Even the emitted events are the same, we don't need to do anything
                        // We will update verification again, as we don't want the dependee values to update unnecessarily
                        VerifiedAt = system.CurrentRevision;
                        // NOTE: The newValue is already a clone here, we can just return that
                        return (T)newValue;
                    }
                    // TODO: If newValue was Equal and only the event caches differed,
                    // we could actually return newValue without overwriting the original one,
                    // saving a clone
                }
                // The new value is different
                cachedValue = newValue;
                // Also use the new events
                SwapEventCachesAndClearTemporaries();
            }
            else
            {
                // Value not memoized yet
                // We check for a cycle first
                system.DetectCycle(this);
                // We push this value onto the computation or "call"-stack
                system.PushDependency(this);
                // NOTE: We ned a try-finally because there is a chance that the first recomputation tries to access an unset value
                // but popping off the dependency from the system is crucial
                try
                {
                    // This is the first time we are dealing with events, record them to the primary cache
                    SubscribeToEvents(cachedEvents);
                    // Now we do the recomputation
                    // This will emit relevant events, no need to explicitly
                    var newValue = await recompute(system, cancellationToken);
                    // We need to unsubscribe to avoid leaking
                    UnsubscribeFromEvents();
                    // In case a cancellation is requested, we shouldn't continue from here
                    if (cancellationToken.IsCancellationRequested) return (T)newValue;
                    // No cancellation, we can store the value
                    cachedValue = newValue;
                    // NOTE: Events are automatically stored in the primary cache
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

        private bool AreEventCachesEqual() =>
            cachedEvents.Zip(tempCachedEvents).All(sets => sets.First.SetEquals(sets.Second));

        private void SubscribeToEvents(HashSet<(object Sender, object Args)>[] cache)
        {
            for (int i = 0; i < eventProxies.Length; ++i)
            {
                // Create a handler that caches the event
                int j = i;
                EventHandler<object> handler = (sender, args) => cache[j].Add((sender, args));
                // Register it
                var unsubscribeAction = eventProxies[i].Subscribe(handler);
                // Store the unsubscriber
                eventHandlerUnsubscribers[i] = unsubscribeAction;
            }
        }

        private void UnsubscribeFromEvents()
        {
            foreach (var unsubscriber in eventHandlerUnsubscribers) unsubscriber();
        }

        private void ResendEvents()
        {
            for (int i = 0; i < eventProxies.Length; ++i)
            {
                eventProxies[i].Send(cachedEvents[i]);
            }
        }

        private void SwapEventCachesAndClearTemporaries()
        {
            // Swap instances in the arrays
            for (int i = 0; i < eventProxies.Length; ++i)
            {
                var cache1 = cachedEvents[i];
                cachedEvents[i] = tempCachedEvents[i];
                tempCachedEvents[i] = cache1;
                // We also clear the temporaries here
                cache1.Clear();
            }
        }

        private T GetValueCloned<T>()
        {
            if (cachedValue is ICloneable cloneable) return (T)cloneable.Clone();
            else return (T)cachedValue;
        }
    }
}
