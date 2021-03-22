using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Yoakke.DataStructures
{
    /// <summary>
    /// A wrapper type for <see cref="IList{T}"/>s to have value semantics.
    /// </summary>
    public class ValueList<T> : IValueList<T>
    {
        private IList<T> underlying;

        public T this[int index] { get => underlying[index]; set => underlying[index] = value; }
        public int Count => underlying.Count;
        public bool IsReadOnly => underlying.IsReadOnly;

        public ValueList()
            : this(new List<T>())
        {
        }

        public ValueList(IList<T> underlying)
        {
            this.underlying = underlying;
        }

        public void Add(T item) => underlying.Add(item);
        public void Clear() => underlying.Clear();
        public bool Contains(T item) => underlying.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => underlying.CopyTo(array, arrayIndex);
        public IEnumerator<T> GetEnumerator() => underlying.GetEnumerator();
        public int IndexOf(T item) => underlying.IndexOf(item);
        public void Insert(int index, T item) => underlying.Insert(index, item);
        public bool Remove(T item) => underlying.Remove(item);
        public void RemoveAt(int index) => underlying.RemoveAt(index);
        IEnumerator IEnumerable.GetEnumerator() => (underlying as IEnumerable).GetEnumerator();

        // TODO: Optimize with Count comparison?
        public bool Equals(IValueList<T>? other) => other != null && this.SequenceEqual(other);

        public override bool Equals(object? obj) => obj is IValueList<T> o && Equals(o);

        // TODO: We could sample elements above a certain limit to speed up hashing
        public override int GetHashCode()
        {
            var h = new HashCode();
            foreach (var e in this) h.Add(e);
            return h.ToHashCode();
        }
    }
}
