using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.DataStructures
{
    /// <summary>
    /// An <see cref="IList{T}"/> wrapper interface for value semantics.
    /// </summary>
    public interface IValueList<T> : IList<T>, IEquatable<IValueList<T>>
    {
    }
}
