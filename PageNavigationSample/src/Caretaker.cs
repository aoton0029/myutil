using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample
{
    public class Caretaker<T>
    {
        private readonly Stack<Memento<T>> _history = new();

        public void Save(T state)
        {
            _history.Push(new Memento<T>(state));
        }

        public T? Undo()
        {
            if (_history.Count > 0)
            {
                return _history.Pop().State;
            }
            return default;
        }

        public bool CanUndo => _history.Count > 0;
    }
}
