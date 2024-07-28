using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib
{
    public static class UpdateUtils
    {
        public static TOut To<TIn, TOut>(this TIn value, Func<TIn, TOut> action)
            => (action ?? throw new ArgumentNullException(nameof(action))).Invoke(value);

        public static void To<T>(this T value, ref T original, ref bool updated) where T : struct
        {
            // If the values already match, nothing needs to be done
            if (original.Equals(value)) return;

            // Otherwise set the "updated" flag and change the value
            updated = true;
            original = value;
        }

        public static void To<T>(this T value, ref T original, ref bool updated1, ref bool updated2) where T : struct
        {
            // If the values already match, nothing needs to be done
            if (original.Equals(value)) return;

            updated1 = true;
            updated2 = true;
            original = value;
        }

        public static void To(this string value, ref string original, ref bool updated)
        {
            // If the values already match, nothing needs to be done
            if (original == value) return;

            // Otherwise set the "updated" flag and change the value
            updated = true;
            original = value;
        }

        public static void To(this string value, ref string original, ref bool updated1, ref bool updated2)
        {
            if (original == value) return;

            updated1 = true;
            updated2 = true;
            original = value;
        }

        public static void To<T>(this T value, ref T original, Action updated) where T : struct
        {
            // If the values already match, nothing needs to be done
            if (original.Equals(value)) return;

            // Backup the original value in case it needs to be reverted
            var backup = original;

            // Set the new value
            original = value;

            // Execute the "updated" delegate
            try
            {
                updated.Invoke();
            }
            catch
            {
                // Restore the original value before passing exceptions upwards
                original = backup;
                throw;
            }
        }

        public static void To(this string value, ref string original, Action updated)
        {
            // If the values already match, nothing needs to be done
            if (original == value) return;

            // Backup the original value in case it needs to be reverted
            string backup = original;

            // Set the new value
            original = value;

            // Execute the "updated" delegate
            try
            {
                updated.Invoke();
            }
            catch
            {
                // Restore the original value before passing exceptions upwards
                original = backup;
                throw;
            }
        }

        public static void Swap<T>(ref T value1, ref T value2)
        {
            var tempValue = value1;
            value1 = value2;
            value2 = tempValue;
        }

        public static void SafeInvoke(this Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new MethodInvoker(action));
            }
            else
            {
                action();
            }
        }
    }
}
