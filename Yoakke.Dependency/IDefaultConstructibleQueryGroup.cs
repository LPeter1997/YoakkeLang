using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Dependency
{
    /// <summary>
    /// For internal use only.
    /// Marks a query group as default constructible. This is the case for empty query groups and ones with only
    /// input elements.
    /// </summary>
    public interface IDefaultConstructibleQueryGroup
    {
    }
}
