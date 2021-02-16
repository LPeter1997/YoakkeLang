using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Dependency
{
    /// <summary>
    /// Attribute to mark a query group as an input query group.
    /// Must be used for the code-generator to work.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class InputQueryGroupAttribute : Attribute
    {
    }
}
