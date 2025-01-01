using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DesignPatterns.Memento
{
    public sealed class MementoCaretaker : IMementoCaretaker
    {
        private readonly Stack<IMemento> _mementos = new();

        public void CaptureSnapshot(IMementoable mementoable)
        {
            this._mementos.Push(mementoable.SaveToMemento());
        }

        public void Undo()
        {
            if (this._mementos.Count > 0)
            {
                var memento = this._mementos.Pop();
                memento.Originator.RestoreMemento(memento);
            }
        }

        public bool CanUndo => this._mementos.Count > 0;
    }

}
