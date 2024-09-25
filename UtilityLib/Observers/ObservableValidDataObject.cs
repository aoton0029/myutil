using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Observers
{
    [Serializable]
    public abstract class ObservableDataObject : INotifyPropertyChanged
    {
        protected const string IndexerName = "Item[]";

        private static readonly object Missing = new object();

        private readonly ConcurrentDictionary<string, object?> DataStorage;

        private readonly ConcurrentDictionary<string, string[]> RelatedProperties;

        protected ObservableDataObject()
        {
            this.DataStorage = new ConcurrentDictionary<string, object?>();
            this.RelatedProperties = new ConcurrentDictionary<string, string[]>();
            this.InitializeRelatedProperties();
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void InitializeRelatedProperties() { }

        protected bool IsEntityName([NotNullWhen(false)] string? propertyName)
        {
            return string.IsNullOrEmpty(propertyName);
        }

        [return: MaybeNull]
        protected T GetProperty<T>([CallerMemberName] string? propertyName = null)
        {
            if (this.IsEntityName(propertyName)) { return (T)(object)this; }
            var value = this.GetPropertyCore(propertyName);
            var hasValue = value != ObservableDataObject.Missing;
            return hasValue ? (T?)value : default(T);
        }

        protected virtual void SetProperty<T>(
            [AllowNull] T value, [CallerMemberName] string? propertyName = null)
        {
            if (this.IsEntityName(propertyName)) { throw new InvalidOperationException(); }
            var property = this.ExchangeProperty(propertyName, value);
            var propertyChanged = !RuntimeHelpers.Equals(property, value);
            if (propertyChanged) { this.RelatedNotifyPropertyChanged(propertyName); }
        }

        protected virtual object? GetPropertyCore(string propertyName)
        {
            var hasValue = this.DataStorage.TryGetValue(propertyName, out var value);
            return hasValue ? value : ObservableDataObject.Missing;
        }

        protected virtual void SetPropertyCore(string propertyName, object? value)
        {
            this.DataStorage[propertyName] = value;
        }

        protected virtual object? ExchangeProperty(string propertyName, object? value)
        {
            var property = this.GetPropertyCore(propertyName);
            this.SetPropertyCore(propertyName, value);
            return property;
        }

        protected string[] GetRelatedProperties([CallerMemberName] string? propertyName = null)
        {
            if (this.IsEntityName(propertyName)) { return Array.Empty<string>(); }
            this.RelatedProperties.TryGetValue(propertyName, out var relatedProperties);
            return relatedProperties ?? Array.Empty<string>();
        }

        protected void SetRelatedProperties(string propertyName, params string[] relatedProperties)
        {
            if (this.IsEntityName(propertyName)) { throw new InvalidOperationException(); }
            var hasRelated = !(relatedProperties is null) && (relatedProperties.Length != 0);
            if (hasRelated) { this.RelatedProperties[propertyName] = relatedProperties!; }
            else { this.RelatedProperties.TryRemove(propertyName, out relatedProperties!); }
        }

        protected void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected void RelatedNotifyPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.NotifyPropertyChanged(propertyName);
            if (this.IsEntityName(propertyName)) { return; }
            var relatedProperties = this.GetRelatedProperties(propertyName);
            Array.ForEach(relatedProperties, this.NotifyPropertyChanged);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}
