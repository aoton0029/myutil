using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSample.Sample2
{
    class WaveformStep : INotifyPropertyChanged, INotifyPropertyChanging, INotifyDataErrorInfo
    {
        public decimal Frequency { get; set; }

        public decimal Amplitude { get; set; }

        public decimal Phase { get; set; }

        public decimal StepTime { get; set; }

        private Dictionary<string, List<string>> _errors = new();

        public bool HasErrors => _errors.Any();

        public event PropertyChangedEventHandler? PropertyChanged;
        public event PropertyChangingEventHandler? PropertyChanging;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public IEnumerable GetErrors(string? propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) return Enumerable.Empty<string>();
            return _errors.TryGetValue(propertyName, out var list) ? list : Enumerable.Empty<string>();
        }

        public void AddError(string propertyName, string error)
        {
            if (!_errors.ContainsKey(propertyName)) _errors[propertyName] = new List<string>();
            if (!_errors[propertyName].Contains(error))
            {
                _errors[propertyName].Add(error);
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        public void ClearErrors(string propertyName)
        {
            if (_errors.Remove(propertyName))
            {
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }
    }
}
