using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Models
{
    public interface IValidator
    {
        bool Validate(object value, ValidationContext context);
        string ErrorMessage { get; }
    }

    public class ValidationContext
    {
        public string PropertyName { get; set; }
        public Dictionary<string, object> Items { get; } = new();
    }

    public class RangeValidator<T> : IValidator where T : IComparable<T>
    {
        public T Min { get; }
        public T Max { get; }
        public string ErrorMessage { get; private set; }

        public RangeValidator(T min, T max)
        {
            Min = min;
            Max = max;
        }

        public bool Validate(object value, ValidationContext context)
        {
            if (value is not T typedValue)
            {
                ErrorMessage = $"{context.PropertyName} is not of type {typeof(T).Name}.";
                return false;
            }

            if (typedValue.CompareTo(Min) < 0 || typedValue.CompareTo(Max) > 0)
            {
                ErrorMessage = $"{context.PropertyName} must be between {Min} and {Max}.";
                return false;
            }

            return true;
        }
    }

    public class MultipleValidator : IValidator
    {
        private readonly List<IValidator> _validators = new();
        public string ErrorMessage { get; private set; }

        public void AddValidator(IValidator validator)
        {
            _validators.Add(validator);
        }

        public bool Validate(object value, ValidationContext context)
        {
            foreach (var validator in _validators)
            {
                if (!validator.Validate(value, context))
                {
                    ErrorMessage = validator.ErrorMessage;
                    return false;
                }
            }
            return true;
        }
    }




}
