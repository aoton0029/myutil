using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Navigations.MVVC
{
    public enum HistoricalNavigationMode
    {
        UseSavedParameter,
        UseCommandParameter,
        UseSavedViewModel,
    };

    public interface ICommand
    {
        event EventHandler CanExecuteChanged;
        bool CanExecute(object parameter);
        void Execute(object parameter);
    }

    public class DelegateCommand<T> : ICommand
    {
        private readonly Action<T> _action;
        private readonly Func<T, bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<T> action, Func<T, bool> canExecute = null)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return
                (
                    _canExecute == null ||
                    (typeof(T).IsClass || parameter != null) &&
                    _canExecute((T)parameter)
                );
        }

        public virtual void Execute(object parameter)
        {
            _action((T)parameter);
        }


        public void RaiseCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }

    public class DelegateCommand : DelegateCommand<object>
    {
        public DelegateCommand(Action action, Func<bool> canExecute = null) :
            base(_ => action(), canExecute == null ? (Func<object, bool>)null : (_ => canExecute()))
        {

        }
    }

    public static class DelegateCommandExtentions
    {
        public static void RaiseCanExecuteChanged<T>(this ICommand command)
        {
            var cmd = command as DelegateCommand<T>;
            if (cmd != null)
            {
                cmd.RaiseCanExecuteChanged();
            }
        }
        public static void RaiseCanExecuteChanged(this ICommand command)
        {
            var cmd = command as DelegateCommand;
            if (cmd != null)
            {
                cmd.RaiseCanExecuteChanged();
            }
        }
    }

    public class NavigateCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        Lazy<MVVMCNavigationService> _navigationService = new Lazy<MVVMCNavigationService>(() => MVVMCNavigationService.GetInstance());

        public string ControllerID { get; set; }
        public string Action { get; set; }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (Action == null)
                _navigationService.Value.GetController(ControllerID).NavigateToInitial();
            else
                _navigationService.Value.GetController(ControllerID).Navigate(Action, parameter);
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

    public class GoBackCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        Lazy<MVVMCNavigationService> _navigationService = new Lazy<MVVMCNavigationService>(() => MVVMCNavigationService.GetInstance());

        public HistoricalNavigationMode HistoricalNavigationMode { get; set; } = HistoricalNavigationMode.UseCommandParameter;
        public string ControllerID { get; set; }
        public Dictionary<string, object> ViewBag { get; set; }

        private bool _canExecute = false;

        public GoBackCommand()
        {
            _navigationService.Value.AddGoBackCommand(this);
        }

        public void ChangeCanExecute()
        {
            var newValue = _navigationService.Value.GetController(ControllerID).CanGoBack;
            if (newValue != _canExecute)
            {
                _canExecute = newValue;
                CanExecuteChanged?.Invoke(this, null);
            }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute;
        }

        public void Execute(object parameter)
        {
            if (HistoricalNavigationMode == HistoricalNavigationMode.UseCommandParameter)
            {
                _navigationService.Value.GetController(ControllerID).GoBack(parameter, ViewBag);
            }
            else
            {
                _navigationService.Value.GetController(ControllerID).GoBack();
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

    public class GoForwardCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        Lazy<MVVMCNavigationService> _navigationService = new Lazy<MVVMCNavigationService>(() => MVVMCNavigationService.GetInstance());

        public HistoricalNavigationMode HistoricalNavigationMode { get; set; } = HistoricalNavigationMode.UseCommandParameter;
        public string ControllerID { get; set; }
        public Dictionary<string, object> ViewBag { get; set; }

        private bool _canExecute = false;

        public GoForwardCommand()
        {
            _navigationService.Value.AddGoForwardCommand(this);
        }

        public void ChangeCanExecute()
        {
            var newValue = _navigationService.Value.GetController(ControllerID).CanGoForward;
            if (newValue != _canExecute)
            {
                _canExecute = newValue;
                CanExecuteChanged?.Invoke(this, null);
            }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute;
        }

        public void Execute(object parameter)
        {
            if (HistoricalNavigationMode == HistoricalNavigationMode.UseCommandParameter)
            {
                _navigationService.Value.GetController(ControllerID).GoForward(parameter, ViewBag);
            }
            else
            {
                _navigationService.Value.GetController(ControllerID).GoForward();
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
