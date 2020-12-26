using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Yoakke.DataStructures
{
    /// <summary>
    /// A wrapper type for <see cref="IDictionary{TKey, TValue}"/>s to have value semantics.
    /// </summary>
    public class ValueDictionary<TKey, TValue> : IValueDictionary<TKey, TValue> where TKey : notnull
    {
        private IDictionary<TKey, TValue> underlying;

        public TValue this[TKey key] { get => underlying[key]; set => underlying[key] = value; }
        public ICollection<TKey> Keys => underlying.Keys;
        public ICollection<TValue> Values => underlying.Values;
        public int Count => underlying.Count;
        public bool IsReadOnly => underlying.IsReadOnly;

        public ValueDictionary()
            : this(new Dictionary<TKey, TValue>())
        {
        }

        public ValueDictionary(IDictionary<TKey, TValue> underlying)
        {
            this.underlying = underlying;
        }

        public void Add(TKey key, TValue value) => underlying.Add(key, value);
        public void Add(KeyValuePair<TKey, TValue> item) => underlying.Add(item);
        public void Clear() => underlying.Clear();
        public bool Contains(KeyValuePair<TKey, TValue> item) => underlying.Contains(item);
        public bool ContainsKey(TKey key) => underlying.ContainsKey(key);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) =>
            underlying.CopyTo(array, arrayIndex);
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => underlying.GetEnumerator();
        public bool Remove(TKey key) => underlying.Remove(key);
        public bool Remove(KeyValuePair<TKey, TValue> item) => underlying.Remove(item);
        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => 
            underlying.TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => underlying.GetEnumerator();

        public override bool Equals(object? obj) =>
            obj is IValueDictionary<TKey, TValue> other && Equals(other);

        public bool Equals(IDictionary<TKey, TValue>? other)
        {
            if (other is null) return false;
            if (Count != other.Count) return false;
            foreach (var (key, value) in underlying)
            {
                if (!other.TryGetValue(key, out var otherValue)) return false;
                if (value == null && otherValue == null) continue;
                if (value == null) return false;
                if (!value.Equals(otherValue)) return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            var h = new HashCode();
            // Order by key to make hash code stable
            foreach (var e in this.OrderBy(kv => kv.Key)) h.Add(e);
            return h.ToHashCode();
        }
    }
}
