using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib
{
    public static class EnumExtension
    {
        /// <summary>
        /// EnumのDescription属性を取得します。
        /// </summary>
        public static string GetDescription(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return attribute == null ? value.ToString() : attribute.Description;
        }

        /// <summary>
        /// 文字列からEnumを取得します。
        /// </summary>
        public static T ToEnum<T>(this string value, T defaultValue) where T : struct
        {
            if (Enum.TryParse(value, true, out T result))
            {
                return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// Enumの値が定義されているかどうかを確認します。
        /// </summary>
        public static bool IsDefined<T>(this T value) where T : struct, Enum
        {
            return Enum.IsDefined(typeof(T), value);
        }

        /// <summary>
        /// Enumのすべての値を取得します。
        /// </summary>
        public static T[] GetAllValues<T>() where T : struct, Enum
        {
            return (T[])Enum.GetValues(typeof(T));
        }

        /// <summary>
        /// Enumの値をデータソースとしてリスト化し、ComboBoxやDataGridViewなどのコントロールにバインドするためのデータソースを作成します。
        /// </summary>
        public static List<KeyValuePair<string, T>> ToDataSource<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
                       .Cast<T>()
                       .Select(e => new KeyValuePair<string, T>(e.GetDescription(), e))
                       .ToList();
        }

        /// <summary>
        /// ComboBoxにEnumをデータソースとして設定します。
        /// </summary>
        public static void SetEnumDataSource<T>(this ComboBox comboBox) where T : Enum
        {
            comboBox.DataSource = ToDataSource<T>();
            comboBox.DisplayMember = "Key";
            comboBox.ValueMember = "Value";
        }

        /// <summary>
        /// DataGridViewにEnumをデータソースとして設定します。
        /// </summary>
        public static void SetEnumDataSource<T>(this DataGridViewComboBoxColumn column) where T : Enum
        {
            column.DataSource = ToDataSource<T>();
            column.DisplayMember = "Key";
            column.ValueMember = "Value";
        }
    }
}
