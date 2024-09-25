using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Commands
{
    public class DelegateCommand : CommandBase
    {
        private readonly Action<object?> ExecuteDelegate;

        private readonly Predicate<object?> CanExecuteDelegate;

        public DelegateCommand(Action<object?> executeDelegate,
            Predicate<object?>? canExecuteDelegate = null)
        {
            this.ExecuteDelegate = executeDelegate ??
                throw new ArgumentNullException(nameof(executeDelegate));
            this.CanExecuteDelegate = canExecuteDelegate ?? base.CanExecute;
        }

        public override void Execute(object? parameter)
        {
            this.ExecuteDelegate.Invoke(parameter);
        }

        public override bool CanExecute(object? parameter)
        {
            return this.CanExecuteDelegate.Invoke(parameter);
        }

        public new void NotifyCanExecuteChanged()
        {
            base.NotifyCanExecuteChanged();
        }

        public DelegateCommand ObserveCanExecute(
            INotifyPropertyChanged source, string? propertyName)
        {
            if (source is null) { throw new ArgumentNullException(nameof(source)); }
            var observer = new Observers.SimplePropertyObserver(source, propertyName);
            observer.ObservingPropertyChanged += this.OnObservingCanExecuteChanged;
            return this;
        }

        private void OnObservingCanExecuteChanged(object? sender, EventArgs e)
        {
            this.NotifyCanExecuteChanged();
        }
    }
}
