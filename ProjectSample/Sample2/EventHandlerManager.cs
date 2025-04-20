using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSample.Sample2
{
    class EventHandlerManager : IDisposable
    {
        private readonly EventBus _eventBus;

        // 通常のイベント購読
        private readonly List<(Type, Delegate)> _subscriptions = new();

        // PropertyChanged, CollectionChangedの購読マップ
        private readonly Dictionary<INotifyPropertyChanged, PropertyChangedEventHandler> _propertyObservers = new();
        private readonly Dictionary<INotifyCollectionChanged, NotifyCollectionChangedEventHandler> _collectionObservers = new();

        public EventHandlerManager(EventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void Subscribe<TEventArgs>(EventHandler<TEventArgs> handler)
            where TEventArgs : EventArgs
        {
            _eventBus.Subscribe(handler);
            _subscriptions.Add((typeof(TEventArgs), handler));
        }

        public void ObservePropertyChanged(INotifyPropertyChanged target)
        {
            if (_propertyObservers.ContainsKey(target)) return;

            PropertyChangedEventHandler handler = (s, e) =>
            {
                _eventBus.Publish(s!, new PropertyChangedEventArgs(e.PropertyName));
            };

            target.PropertyChanged += handler;
            _propertyObservers[target] = handler;
        }

        public void ObserveCollectionChanged(INotifyCollectionChanged collection)
        {
            if (_collectionObservers.ContainsKey(collection)) return;

            NotifyCollectionChangedEventHandler handler = (s, e) =>
            {
                _eventBus.Publish(s!, e);
            };

            collection.CollectionChanged += handler;
            _collectionObservers[collection] = handler;
        }

        public void UnsubscribeAll()
        {
            foreach (var (type, handler) in _subscriptions)
            {
                var unsubscribe = typeof(EventBus).GetMethod(nameof(EventBus.Unsubscribe))?
                    .MakeGenericMethod(type);
                unsubscribe?.Invoke(_eventBus, new object[] { handler });
            }
            _subscriptions.Clear();

            foreach (var (target, handler) in _propertyObservers)
            {
                target.PropertyChanged -= handler;
            }
            _propertyObservers.Clear();

            foreach (var (collection, handler) in _collectionObservers)
            {
                collection.CollectionChanged -= handler;
            }
            _collectionObservers.Clear();
        }

        public void Dispose() => UnsubscribeAll();
    }

}
