using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample
{
    public abstract class ObservableObject : INotifyPropertyChanged, INotifyPropertyChanging
    {
        /// <inheritdoc cref="INotifyPropertyChanged.PropertyChanged"/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <inheritdoc cref="INotifyPropertyChanging.PropertyChanging"/>
        public event PropertyChangingEventHandler? PropertyChanging;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            ArgumentNullException.ThrowIfNull(e);

            PropertyChanged?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanging(PropertyChangingEventArgs e)
        {
            ArgumentNullException.ThrowIfNull(e);
            PropertyChanging?.Invoke(this, e);
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected void OnPropertyChanging([CallerMemberName] string? propertyName = null)
        {
            OnPropertyChanging(new PropertyChangingEventArgs(propertyName));
        }

        protected bool SetProperty<T>([NotNullIfNotNull(nameof(newValue))] ref T field, T newValue, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            OnPropertyChanging(propertyName);

            field = newValue;

            OnPropertyChanged(propertyName);

            return true;
        }

        protected bool SetProperty<T>([NotNullIfNotNull(nameof(newValue))] ref T field, T newValue, IEqualityComparer<T> comparer, [CallerMemberName] string? propertyName = null)
        {
            ArgumentNullException.ThrowIfNull(comparer);

            if (comparer.Equals(field, newValue))
            {
                return false;
            }

            OnPropertyChanging(propertyName);

            field = newValue;

            OnPropertyChanged(propertyName);

            return true;
        }

        protected bool SetProperty<T>(T oldValue, T newValue, Action<T> callback, [CallerMemberName] string? propertyName = null)
        {
            ArgumentNullException.ThrowIfNull(callback);

            // We avoid calling the overload again to ensure the comparison is inlined
            if (EqualityComparer<T>.Default.Equals(oldValue, newValue))
            {
                return false;
            }

            OnPropertyChanging(propertyName);

            callback(newValue);

            OnPropertyChanged(propertyName);

            return true;
        }

        protected bool SetProperty<T>(T oldValue, T newValue, IEqualityComparer<T> comparer, Action<T> callback, [CallerMemberName] string? propertyName = null)
        {
            ArgumentNullException.ThrowIfNull(comparer);
            ArgumentNullException.ThrowIfNull(callback);

            if (comparer.Equals(oldValue, newValue))
            {
                return false;
            }

            OnPropertyChanging(propertyName);

            callback(newValue);

            OnPropertyChanged(propertyName);

            return true;
        }
    }
}
