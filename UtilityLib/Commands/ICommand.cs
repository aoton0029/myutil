using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace UtilityLib.Commands
{
    public interface ICommand
    {
        event EventHandler? CanExecuteChanged;

        bool CanExecute(object? parameter);

        void Execute(object? parameter);
    }
}
