using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Observers
{
    internal sealed class SimplePropertyObserver : IDisposable
    {
        private readonly INotifyPropertyChanged Source;

        private readonly string? PropertyName;

        public SimplePropertyObserver(INotifyPropertyChanged source, string? propertyName)
        {
            this.Source = source ??
                throw new ArgumentNullException(nameof(source));
            this.PropertyName = propertyName;
            this.Source.PropertyChanged += this.OnSourcePropertyChanged;
        }

        public event EventHandler? ObservingPropertyChanged;

        public void Dispose()
        {
            this.Source.PropertyChanged -= this.OnSourcePropertyChanged;
        }

        private void OnObservingPropertyChanged(EventArgs e)
        {
            this.ObservingPropertyChanged?.Invoke(this, e);
        }

        private void OnSourcePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (this.PropertyName == e.PropertyName)
            {
                this.OnObservingPropertyChanged(EventArgs.Empty);
            }
        }
    }
}
