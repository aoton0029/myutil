using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib
{
    internal class CollectionExt
    {
        public class MonitoredCollection<T> : Collection<T>
        {
            private bool _dontRaiseEvents;
            public event Action? Changed;
            public event Action<T>? Added;
            public event Action<T>? Removing;
            public event Action<T>? Removed;

            private void OnChanged()
            {
                if (!_dontRaiseEvents) Changed?.Invoke();
            }

            private void OnAdded(T item)
            {
                if (!_dontRaiseEvents) Added?.Invoke(item);
            }

            private void OnRemoving(T item) => Removing?.Invoke(item);

            private void OnRemoved(T item)
            {
                if (!_dontRaiseEvents) Removed?.Invoke(item);
            }


            public MonitoredCollection() 
            { 
                
            }

            protected override void InsertItem(int index, T item)
            {
                base.InsertItem(index, item);
                OnAdded(item);
                OnChanged();
            }

            protected override void SetItem(int index, T item)
            {
                var oldItem = Items[index];

                OnRemoving(oldItem);
                base.SetItem(index, item);
                OnRemoved(oldItem);
                OnAdded(item);
                OnChanged();
            }

            protected override void RemoveItem(int index)
            {
                var oldItem = Items[index];

                OnRemoving(oldItem);
                base.RemoveItem(index);
                OnRemoved(oldItem);
            }

            protected override void ClearItems()
            {
                foreach (var item in this) OnRemoving(item);

                var oldItems = new List<T>(this);

                base.ClearItems();

                foreach (var item in oldItems) OnRemoved(item);

                OnChanged();
            }

            public void AddMany(IEnumerable<T> collection)
            {
                var added = new LinkedList<T>();

                _dontRaiseEvents = true;
                foreach (var item in collection.Where(item => !Contains(item)))
                {
                    Add(item);
                    added.AddLast(item);
                }
                _dontRaiseEvents = false;

                foreach (var item in added) OnAdded(item);
                OnChanged();
            }

            public void SetMany(IEnumerable<T> enumeration)
            {
                var copy = new List<T>(this);

                var removed = new LinkedList<T>();

                _dontRaiseEvents = true;
                foreach (var item in copy.Where(item => !enumeration.Contains(item)))
                {
                    Remove(item);
                    removed.AddLast(item);
                }
                _dontRaiseEvents = false;

                foreach (var item in removed) Removed?.Invoke(item);

                AddMany(enumeration);
            }
        }

    }
}
