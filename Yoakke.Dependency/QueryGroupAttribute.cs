using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Dependency
{
    /// <summary>
    /// Attribute to mark an interface as a related group of queries.
    /// Must be used for the code-generator to work.
    /// Can be used on query group properties to inject other query groups for easier access.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Property)]
    public class QueryGroupAttribute : Attribute
    {
    }
}
