using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Dependency.Internal
{
    /// <summary>
    /// Proxy for a C# event.
    /// </summary>
    public class EventProxy
    {
        private readonly Func<EventHandler<object>, Action> subscriber;
        private readonly object target;
        private readonly FieldInfo delegateFieldInfo;

        public EventProxy(
            object target,
            string name,
            Func<EventHandler<object>, Action> subscriber)
        {
            this.subscriber = subscriber;
            this.target = target;
            this.delegateFieldInfo = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Subscribes to the event that this one is a proxy to.
        /// </summary>
        /// <param name="eventHandler">The event handler to subascribe.</param>
        /// <returns>The action to run to unregister the handler.</returns>
        public Action Subscribe(EventHandler<object> eventHandler) => subscriber(eventHandler);

        public void Send(DependencySystem system, IEnumerable<(object Sender, object Args)> events)
        {
            var delegates = delegateFieldInfo.GetValue(target) as MulticastDelegate;
            if (delegates == null) return;
            foreach (var (sender, args) in events)
            {
                if (system.CacheEvent((sender, args)))
                {
                    foreach (var handler in delegates.GetInvocationList())
                    {
                        handler.Method.Invoke(handler.Target, new object[] { sender, args });
                    }
                }
            }
        }
    }
}
