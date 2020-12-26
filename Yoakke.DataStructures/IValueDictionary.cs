using System;
using System.Collections.Generic;

namespace Yoakke.DataStructures
{
    /// <summary>
    /// An <see cref="IDictionary{TKey, TValue}"/> wrapper interface for value semantics.
    /// </summary>
    public interface IValueDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IEquatable<IDictionary<TKey, TValue>>
    {
    }
}
