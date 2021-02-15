using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Dependency
{
    /// <summary>
    /// Attribute to mark a query inside a query group as an input query.
    /// Must be used for the code-generator to work.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class InputQueryAttribute : Attribute
    {
    }
}
