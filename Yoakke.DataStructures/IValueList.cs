using System;
using System.Collections.Generic;

namespace Yoakke.DataStructures
{
    /// <summary>
    /// An <see cref="IList{T}"/> wrapper interface for value semantics.
    /// </summary>
    public interface IValueList<T> : IList<T>, IEquatable<IValueList<T>>
    {
    }
}
