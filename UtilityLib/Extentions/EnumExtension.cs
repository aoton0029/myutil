using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UtilityLib.Observers;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace UtilityLib
{
    public static class EnumExtension
    {
        public static bool Has<T>(this Enum source, params T[] values)
        {
            var value = Convert.ToInt32(source, CultureInfo.InvariantCulture);

            foreach (var i in values)
            {
                var mask = Convert.ToInt32(i, CultureInfo.InvariantCulture);

                if ((value & mask) == 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static T As<T>(this object value)
        {
            return (value != null && value is T) ? (T)value : default(T);
        }

        public static bool Has<T>(this Enum source, T values)
        {
            var value = Convert.ToInt32(source, CultureInfo.InvariantCulture);
            var mask = Convert.ToInt32(values, CultureInfo.InvariantCulture);

            return (value & mask) != 0;
        }

        public static T Add<T>(this Enum source, T v)
        {
            var value = Convert.ToInt32(source, CultureInfo.InvariantCulture);
            var mask = Convert.ToInt32(v, CultureInfo.InvariantCulture);

            return Enum.ToObject(typeof(T), value | mask).As<T>();
        }

        public static T Remove<T>(this Enum source, T v)
        {
            var value = Convert.ToInt32(source, CultureInfo.InvariantCulture);
            var mask = Convert.ToInt32(v, CultureInfo.InvariantCulture);

            return Enum.ToObject(typeof(T), value & ~mask).As<T>();
        }

        public static T AsEnum<T>(this string value)
        {
            try
            {
                return Enum.Parse(typeof(T), value, true).As<T>();
            }
            catch
            {
                return default(T);
            }
        }


        public enum Status
        {
            [Description("Success")]
            Success = 1,

            [Description("Error")]
            Error = 2,

            [Description("Pending")]
            Pending = 3
        }

        public static string GetDescription(Status status)
        {
            var fieldInfo = status.GetType().GetField(status.ToString());
            var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes.Length > 0 ? attributes[0].Description : status.ToString();
        }

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

        static List<T> TrigsrcsOfBitfield<T>(int bf) where T : Enum
        {
            var r = new List<T>();
            var trigger_values = (T[])Enum.GetValues(typeof(T));
            foreach (var tv in trigger_values)
            {
                //int i = (int)tv;
                //if ((bf & (1 << i)) != 0)
                //{
                //    r.Add(tv);
                //}
            }
            return r;
        }
    }

    public class EnumListView<TEnum> : ReadOnlyObservableCollection<TEnum> where TEnum : struct, Enum
    {
        private volatile int ItemIndex;

        public EnumListView() : base(EnumListView<TEnum>.CreateItems()) { }

        private static ObservableCollection<TEnum> CreateItems()
        {
            var values = (TEnum[])Enum.GetValues(typeof(TEnum));
            return new ObservableCollection<TEnum>(values);
        }

        public int SelectedIndex
        {
            get => this.ItemIndex;
            set => this.SelectIndex(value);
        }

        public TEnum SelectedItem
        {
            get => this[this.SelectedIndex];
            set => this.SelectedIndex = this.IndexOf(value);
        }

        protected virtual void SelectIndex(int index)
        {
            if ((index < 0) || (index >= this.Count))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (Interlocked.Exchange(ref this.ItemIndex, index) != index)
            {
                this.NotifyPropertyChanged(nameof(this.SelectedIndex));
                this.NotifyPropertyChanged(nameof(this.SelectedItem));
            }
        }

        protected void NotifyPropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
    }

    [Serializable]
    [DebuggerDisplay("Value = {" + nameof(Value) + "}")]
    public class EnumVectorView<TEnum> : ObservableDataObject where TEnum : struct, Enum
    {
        private static readonly ReadOnlyDictionary<string, TEnum> EnumLookupTable = EnumVectorView<TEnum>.CreateEnumLookupTable();

        private volatile Enum EnumValue = default(TEnum);

        public EnumVectorView() { }

        public TEnum Value
        {
            get => this.GetProperty<TEnum>();
            set => this.SetProperty(value);
        }

        public bool this[TEnum enumValue]
        {
            get => this.IsSelected(enumValue);
            set => this.SelectEnum(enumValue, value);
        }

        private static ReadOnlyDictionary<string, TEnum> CreateEnumLookupTable()
        {
            var enumNames = Enum.GetNames(typeof(TEnum));
            var enumValues = (TEnum[])Enum.GetValues(typeof(TEnum));
            var lookupTable = new Dictionary<string, TEnum>(enumNames.Length);
            for (int index = 0; index < enumNames.Length; index++)
            {
                lookupTable[enumNames[index]] = enumValues[index];
            }
            return new ReadOnlyDictionary<string, TEnum>(lookupTable);
        }

        protected override void InitializeRelatedProperties()
        {
            base.InitializeRelatedProperties();
            var enumNames = Enum.GetNames(typeof(TEnum));
            var valueRelated = new string[enumNames.Length + 1];
            Array.Copy(enumNames, valueRelated, enumNames.Length);
            valueRelated[enumNames.Length] = ObservableDataObject.IndexerName;
            this.SetRelatedProperties(nameof(this.Value), valueRelated);
        }

        protected override object? GetPropertyCore(string propertyName)
        {
            return (propertyName == nameof(this.Value)) ?
                this.EnumValue : base.GetPropertyCore(propertyName);
        }

        protected override void SetPropertyCore(string propertyName, object? value)
        {
            var isEnumValue = propertyName == nameof(this.Value);
            if (isEnumValue) { this.EnumValue = (TEnum)value!; }
            else { base.SetPropertyCore(propertyName, value); }
        }

        protected override object? ExchangeProperty(string propertyName, object? value)
        {
            return (propertyName == nameof(this.Value)) ?
                Interlocked.Exchange(ref this.EnumValue, (TEnum)value!) :
                base.ExchangeProperty(propertyName, value);
        }

        protected virtual bool IsSelected(TEnum enumValue)
        {
            return this.Value.Equals(enumValue);
        }

        protected virtual void SelectEnum(TEnum enumValue, bool select)
        {
            if (select) { this.Value = enumValue; }
        }

        protected TEnum FindEnumValue(string enumName)
        {
            var lookupTable = EnumVectorView<TEnum>.EnumLookupTable;
            var hasValue = lookupTable.TryGetValue(enumName, out var enumValue);
            return hasValue ? enumValue : (TEnum)Enum.Parse(typeof(TEnum), enumName);
        }

        protected bool IsSelected([CallerMemberName] string? enumName = null)
        {
            var enumValue = this.FindEnumValue(enumName!);
            return this.IsSelected(enumValue);
        }

        protected void SelectEnum(bool select, [CallerMemberName] string? enumName = null)
        {
            var enumValue = this.FindEnumValue(enumName!);
            this.SelectEnum(enumValue, select);
        }
    }
}
