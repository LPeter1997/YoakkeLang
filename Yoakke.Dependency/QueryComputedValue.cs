using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Dependency
{
    /// <summary>
    /// For internal use only.
    /// Data associated with a query-computed value.
    /// </summary>
    public class QueryComputedValue
    {
        /// <summary>
        /// The computed value.
        /// </summary>
        public object Value { get; private set; }
        /// <summary>
        /// The revision when the <see cref="Value"/> changed.
        /// </summary>
        public int ChangedAt { get; private set; } = -1;
        /// <summary>
        /// The revision when we know that the old <see cref="Value"/> is reusable.
        /// </summary>
        public int VerifiedAt { get; private set; } = -1;
        /// <summary>
        /// Dependencies of this query result.
        /// </summary>
        public ISet<QueryComputedValue> Dependencies { get; } = new HashSet<QueryComputedValue>();
        /// <summary>
        /// True, if the <see cref="Value"/> has been set at least once.
        /// </summary>
        public bool IsSet => ChangedAt != -1;
    }
}
