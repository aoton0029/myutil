using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSample.Sample2
{
    class EventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();

        public void Subscribe<TEventArgs>(EventHandler<TEventArgs> handler)
            where TEventArgs : EventArgs
        {
            var type = typeof(TEventArgs);
            if (!_handlers.ContainsKey(type))
                _handlers[type] = new List<Delegate>();
            _handlers[type].Add(handler);
        }

        public void Unsubscribe<TEventArgs>(EventHandler<TEventArgs> handler)
            where TEventArgs : EventArgs
        {
            var type = typeof(TEventArgs);
            if (_handlers.TryGetValue(type, out var list))
            {
                list.Remove(handler);
                if (list.Count == 0) _handlers.Remove(type);
            }
        }

        public void Publish<TEventArgs>(object sender, TEventArgs args)
            where TEventArgs : EventArgs
        {
            if (_handlers.TryGetValue(typeof(TEventArgs), out var list))
            {
                foreach (var handler in list.ToArray())
                    ((EventHandler<TEventArgs>)handler)?.Invoke(sender, args);
            }
        }
    }

}
