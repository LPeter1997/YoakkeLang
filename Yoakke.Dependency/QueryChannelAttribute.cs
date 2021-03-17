using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Dependency
{
    /// <summary>
    /// Specifies a channel for the given query method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    public class QueryChannelAttribute : Attribute
    {
        /// <summary>
        /// The name of the channel.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Initializes a new <see cref="QueryChannelAttribute"/>.
        /// </summary>
        /// <param name="name">The name of the channel.</param>
        public QueryChannelAttribute(string name)
        {
            Name = name;
        }
    }
}
