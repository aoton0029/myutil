using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UtilityLib.Core
{
    public static class ValidationHelper
    {
        public static bool IsNumeric(string input) =>
            !string.IsNullOrEmpty(input) && input.All(char.IsDigit);

        public static bool IsAlphanumeric(string input) =>
            !string.IsNullOrEmpty(input) && input.All(c => char.IsLetterOrDigit(c));

        public static bool IsValidEmail(string email) =>
            !string.IsNullOrEmpty(email) &&
            Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        public static bool IsValidPhoneNumber(string phoneNumber) =>
            !string.IsNullOrEmpty(phoneNumber) &&
            Regex.IsMatch(phoneNumber, @"^\d{2,4}-\d{2,4}-\d{4}$");

        public static bool IsValidPostalCode(string postalCode) =>
            !string.IsNullOrEmpty(postalCode) &&
            Regex.IsMatch(postalCode, @"^\d{3}-\d{4}$");

        public static bool IsInRange<T>(T value, T min, T max) where T : IComparable<T> =>
            value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;

        public static bool HasMinLength(string input, int minLength) =>
            !string.IsNullOrEmpty(input) && input.Length >= minLength;

        public static bool HasMaxLength(string input, int maxLength) =>
            input == null || input.Length <= maxLength;
    }
}
