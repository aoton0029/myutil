using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchAppSample
{
    public class EventBus
    {
        private readonly Dictionary<Type, List<WeakReference>> _eventHandlers = new();

        // EventHandler登録
        public void Subscribe<TEventArgs>(EventHandler<TEventArgs> handler) where TEventArgs : EventArgs
        {
            var type = typeof(TEventArgs);
            if (!_eventHandlers.TryGetValue(type, out var handlers))
            {
                handlers = new List<WeakReference>();
                _eventHandlers[type] = handlers;
            }
            handlers.Add(new WeakReference(handler));
        }

        // EventHandler解除（生きている参照のみ解除対象）
        public void Unsubscribe<TEventArgs>(EventHandler<TEventArgs> handler) where TEventArgs : EventArgs
        {
            var type = typeof(TEventArgs);
            if (_eventHandlers.TryGetValue(type, out var handlers))
            {
                handlers.RemoveAll(wr =>
                {
                    if (wr.Target is EventHandler<TEventArgs> existingHandler)
                    {
                        return existingHandler == handler;
                    }
                    return !wr.IsAlive;
                });

                if (handlers.Count == 0)
                {
                    _eventHandlers.Remove(type);
                }
            }
        }

        // イベント発行
        public void Publish<TEventArgs>(object sender, TEventArgs args) where TEventArgs : EventArgs
        {
            var type = typeof(TEventArgs);
            if (_eventHandlers.TryGetValue(type, out var handlers))
            {
                var deadHandlers = new List<WeakReference>();

                foreach (var weakRef in handlers)
                {
                    if (weakRef.Target is EventHandler<TEventArgs> handler)
                    {
                        handler(sender, args);
                    }
                    else if (!weakRef.IsAlive)
                    {
                        deadHandlers.Add(weakRef);
                    }
                }

                // 死んだハンドラは後で掃除
                foreach (var dead in deadHandlers)
                {
                    handlers.Remove(dead);
                }

                if (handlers.Count == 0)
                {
                    _eventHandlers.Remove(type);
                }
            }
        }
    }
}
