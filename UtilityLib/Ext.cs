using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib
{
    public static class Ext
    {
        /// <summary>
        /// Clones the model.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classObject">The class object.</param>
        /// <returns>T.</returns>
        public static T CloneModel<T>(this T classObject) where T : class
        {
            T result;
            if (classObject == null)
            {
                result = default(T);
            }
            else
            {
                object obj = Activator.CreateInstance(typeof(T));
                PropertyInfo[] properties = typeof(T).GetProperties();
                PropertyInfo[] array = properties;
                for (int i = 0; i < array.Length; i++)
                {
                    PropertyInfo propertyInfo = array[i];
                    if (propertyInfo.CanWrite)
                        propertyInfo.SetValue(obj, propertyInfo.GetValue(classObject, null), null);
                }
                result = (obj as T);
            }
            return result;
        }

    }
}
