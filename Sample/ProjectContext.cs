using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    public class ProjectContext
    {
        public Project CurrentProject { get; private set; }

        private readonly Stack<IProjectCommand> _undoStack = new();
        private readonly Stack<IProjectCommand> _redoStack = new();

        public ProjectContext(Project project)
        {
            CurrentProject = project;
        }

        public void ExecuteCommand(IProjectCommand command)
        {
            command.Execute(CurrentProject);
            _undoStack.Push(command);
            _redoStack.Clear(); // Redoスタックは操作のたびに無効化
        }

        public void Undo()
        {
            if (_undoStack.TryPop(out var command))
            {
                command.Undo(CurrentProject);
                _redoStack.Push(command);
            }
        }

        public void Redo()
        {
            if (_redoStack.TryPop(out var command))
            {
                command.Execute(CurrentProject);
                _undoStack.Push(command);
            }
        }
    }

}
