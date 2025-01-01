using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample
{

    public class SharedStateManager<T> : IMediator
    {
        private readonly Caretaker<T> _caretaker = new();
        private T _currentState;

        public SharedStateManager(T initialState)
        {
            _currentState = DeepCopy(initialState);
        }

        private readonly Dictionary<string, List<Action<object>>> _eventHandlers = new();

        public void Register(string eventCode, Action<object> handler)
        {
            if (!_eventHandlers.ContainsKey(eventCode))
            {
                _eventHandlers[eventCode] = new List<Action<object>>();
            }
            _eventHandlers[eventCode].Add(handler);
        }

        public void Unregister(string eventCode, Action<object> handler)
        {
            if (_eventHandlers.ContainsKey(eventCode))
            {
                _eventHandlers[eventCode].Remove(handler);
                if (_eventHandlers[eventCode].Count == 0)
                {
                    _eventHandlers.Remove(eventCode);
                }
            }
        }

        public void Notify(string eventCode, object data)
        {
            if (_eventHandlers.ContainsKey(eventCode))
            {
                foreach (var handler in _eventHandlers[eventCode])
                {
                    handler.Invoke(data);
                }
            }
        }

        public void UpdateState(T newState)
        {
            _caretaker.Save(DeepCopy(_currentState));
            _currentState = DeepCopy(newState);
            Notify("StateUpdated", _currentState);
        }

        public T? Undo()
        {
            if (_caretaker.CanUndo)
            {
                _currentState = _caretaker.Undo();
                Notify("StateUpdated", _currentState);
                return _currentState;
            }
            return default;
        }

        public T CurrentState => _currentState;

        private T DeepCopy(T obj)
        {
            if (obj == null) return default;

            var json = System.Text.Json.JsonSerializer.Serialize(obj);
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }
    }

}
