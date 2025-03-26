using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Models
{
    public class CoreProperty<T>
    {
        private T _value;
        private readonly Func<T, bool> _validator;
        private readonly Action<T> _onChanged;

        public CoreProperty(T initialValue = default, Action<T> onChanged = null, Func<T, bool> validator = null)
        {
            _value = initialValue;
            _onChanged = onChanged;
            _validator = validator;
        }

        public T Value
        {
            get => _value;
            set
            {
                if (Equals(_value, value)) return;
                if (_validator != null && !_validator(value)) return;

                _value = value;
                _onChanged?.Invoke(value);
            }
        }
    }

}
