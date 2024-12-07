using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Commands
{
    public abstract class CommandBase : ICommand
    {

        protected CommandBase()
        {

        }

        public event EventHandler? CanExecuteChanged;

        public abstract void Execute(object? parameter);

        public virtual bool CanExecute(object? parameter) => true;

        protected void NotifyCanExecuteChanged()
        {
            this.OnCanExecuteChanged(EventArgs.Empty);
        }

        private void OnCanExecuteChanged(object? e)
        {
            this.CanExecuteChanged?.Invoke(this, (EventArgs)e!);
        }
    }
}
