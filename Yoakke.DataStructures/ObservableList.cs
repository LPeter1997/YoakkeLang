using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.DataStructures
{
    // TODO: Doc
    public class ObservableList<T> : IObservableList<T>
    {
        private IList<T> underlying;

        public T this[int index] 
        { 
            get => underlying[index]; 
            set => underlying[index] = value; 
        }

        public int Count => underlying.Count;

        public bool IsReadOnly => underlying.IsReadOnly;

        public event IObservableList<T>.ItemAddedEventHandler? ItemAdded;
        public event IObservableList<T>.ItemRemovedEventHandler? ItemRemoved;
        public event IObservableList<T>.ListClearedEventHandler? ListCleared;

        public ObservableList(IList<T> underlying)
        {
            this.underlying = underlying;
        }

        public ObservableList()
            : this(new List<T>())
        {
        }

        public void Add(T item)
        {
            underlying.Add(item);
            ItemAdded?.Invoke(this, item);
        }

        public void Clear()
        {
            underlying.Clear();
            ListCleared?.Invoke(this);
        }

        public bool Contains(T item) => underlying.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => underlying.CopyTo(array, arrayIndex);
        public IEnumerator<T> GetEnumerator() => underlying.GetEnumerator();
        public int IndexOf(T item) => underlying.IndexOf(item);

        public void Insert(int index, T item)
        {
            underlying.Insert(index, item);
            ItemAdded?.Invoke(this, item);
        }

        public bool Remove(T item)
        {
            if (underlying.Remove(item))
            {
                ItemRemoved?.Invoke(this, item);
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            var item = underlying[index];
            underlying.RemoveAt(index);
            ItemRemoved?.Invoke(this, item);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
