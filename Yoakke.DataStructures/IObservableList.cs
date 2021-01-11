using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.DataStructures
{
    // TODO: Doc
    public interface IObservableList<T> : IList<T>
    {
        public delegate void ItemAddedEventHandler(IObservableList<T> sender, T item);
        public delegate void ItemRemovedEventHandler(IObservableList<T> sender, T item);
        public delegate void ListClearedEventHandler(IObservableList<T> sender);

        public event ItemAddedEventHandler? ItemAdded;
        public event ItemRemovedEventHandler? ItemRemoved;
        public event ListClearedEventHandler? ListCleared;
    }
}
