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
        public void Subscribe(EventHandler eventHandler)
        {
            // TODO
            throw new NotImplementedException();
        }

        public void Unsubscribe(EventHandler eventHandler)
        {
            // TODO
            throw new NotImplementedException();
        }

        public void Send(IEnumerable<(object Sender, object Args)> events)
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}
