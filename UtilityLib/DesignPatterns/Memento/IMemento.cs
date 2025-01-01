using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DesignPatterns.Memento
{

    public interface IMemento
    {
        IMementoable Originator { get; }
    }

    public interface IMementoable
    {
        IMemento SaveToMemento();

        void RestoreMemento(IMemento memento);
    }

    public interface IMementoCaretaker
    {
        bool CanUndo { get; }

        void CaptureSnapshot(IMementoable mementoable);

        void Undo();
    }
}
