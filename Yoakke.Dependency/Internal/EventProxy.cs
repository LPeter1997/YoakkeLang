using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Dependency.Internal
{
    /// <summary>
    /// Proxy for a C# event.
    /// </summary>
    internal class EventProxy
    {
        private readonly Func<EventHandler<object>, Action> subscriber;
        private readonly Action<IEnumerable<(object Sender, object Args)>> sender;

        public EventProxy(
            Func<EventHandler<object>, Action> subscriber, 
            Action<IEnumerable<(object Sender, object Args)>> sender)
        {
            this.subscriber = subscriber;
            this.sender = sender;
        }

        /// <summary>
        /// Subscribes to the event that this one is a proxy to.
        /// </summary>
        /// <param name="eventHandler">The event handler to subascribe.</param>
        /// <returns>The action to run to unregister the handler.</returns>
        public Action Subscribe(EventHandler<object> eventHandler) => subscriber(eventHandler);

        public void Send(IEnumerable<(object Sender, object Args)> events) => sender(events);
    }
}
