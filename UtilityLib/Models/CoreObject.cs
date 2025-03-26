using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class CoreObject : INotifyPropertyChanged
    {
        public Guid Id { get; } = Guid.NewGuid();

        public bool IsDirty { get; private set; }
        public bool IsNew { get; private set; } = true;

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly Stack<Action> _undoStack = new();
        private readonly Stack<Action> _redoStack = new();

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            IsDirty = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return;

            var oldValue = field;
            field = value;
            RaisePropertyChanged(propertyName);

            // Undo/Redo 登録
            _undoStack.Push(() => {
                field = oldValue;
                RaisePropertyChanged(propertyName);
            });
            _redoStack.Clear(); // redoは上書き
        }

        public void AcceptChanges()
        {
            IsDirty = false;
            IsNew = false;
            _undoStack.Clear();
            _redoStack.Clear();
        }

        public void Undo()
        {
            if (_undoStack.Count > 0)
            {
                var undo = _undoStack.Pop();
                _redoStack.Push(undo); // 現在状態をredo用に
                undo.Invoke();
            }
        }

        public void Redo()
        {
            if (_redoStack.Count > 0)
            {
                var redo = _redoStack.Pop();
                _undoStack.Push(redo);
                redo.Invoke();
            }
        }
    }

}
