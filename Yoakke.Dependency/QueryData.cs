using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Dependency
{
    internal class QueryData
    {
        /// <summary>
        /// The computed value.
        /// </summary>
        public object Value;
        /// <summary>
        /// The revision when the <see cref="Value"/> changed.
        /// </summary>
        public Revision ChangedAt;
        /// <summary>
        /// The revision when we know that the old <see cref="Value"/> is reusable.
        /// </summary>
        public Revision VerifiedAt;
        /// <summary>
        /// Dependencies of this query result.
        /// </summary>
        public IList<QueryData> Dependencies;
    }
}
