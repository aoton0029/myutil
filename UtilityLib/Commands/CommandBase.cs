using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Commands
{
    public abstract class CommandBase : ICommand
    {
        private readonly SynchronizationContext? InitialSyncContext;

        protected CommandBase()
        {
            this.InitialSyncContext = SynchronizationContext.Current;
        }

        public event EventHandler? CanExecuteChanged;

        public abstract void Execute(object? parameter);

        public virtual bool CanExecute(object? parameter) => true;

        protected void NotifyCanExecuteChanged()
        {
            this.OnCanExecuteChanged(EventArgs.Empty);
        }

        protected virtual void OnCanExecuteChanged(EventArgs e)
        {
            if (this.InitialSyncContext is null) { this.OnCanExecuteChanged((object)e); }
            else { this.InitialSyncContext.Post(this.OnCanExecuteChanged, (object)e); }
        }

        private void OnCanExecuteChanged(object? e)
        {
            this.CanExecuteChanged?.Invoke(this, (EventArgs)e!);
        }
    }
}
