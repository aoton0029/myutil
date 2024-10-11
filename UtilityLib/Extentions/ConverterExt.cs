//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace UtilityLib.Extentions
//{
//    [ValueConversion(typeof(IConvertible), typeof(IConvertible),
//                     ParameterType = typeof(IConvertible))]
//    public sealed class ExponentiationConverter : IValueConverter
//    {
//        public ExponentiationConverter() { }

//        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//        {
//            try
//            {
//                var number = Convertible.ToDouble(value ?? 0.0, culture);
//                var @base = Convertible.ToDouble(parameter ?? Math.E, culture);
//                var result = Math.Pow(@base, number);
//                return Convertible.ChangeType(result, targetType, culture);
//            }
//            catch (Exception) { return DependencyProperty.UnsetValue; }
//        }

//        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//        {
//            try
//            {
//                var number = Convertible.ToDouble(value ?? 0.0, culture);
//                var @base = Convertible.ToDouble(parameter ?? Math.E, culture);
//                var result = Math.Log(number, @base);
//                return Convertible.ChangeType(result, targetType, culture);
//            }
//            catch (Exception) { return DependencyProperty.UnsetValue; }
//        }
//    }

//    /// <summary>
//    /// 表示 <see cref="IConvertible"/> 数字到其对应的对数的转换器。
//    /// </summary>
//    [ValueConversion(typeof(IConvertible), typeof(IConvertible),
//                     ParameterType = typeof(IConvertible))]
//    public sealed class LogarithmConverter : IValueConverter
//    {
//        public LogarithmConverter() { }

//        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//        {
//            try
//            {
//                var number = Convertible.ToDouble(value ?? 0.0, culture);
//                var @base = Convertible.ToDouble(parameter ?? Math.E, culture);
//                var result = Math.Log(number, @base);
//                return Convertible.ChangeType(result, targetType, culture);
//            }
//            catch (Exception) { return DependencyProperty.UnsetValue; }
//        }

//        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//        {
//            try
//            {
//                var number = Convertible.ToDouble(value ?? 0.0, culture);
//                var @base = Convertible.ToDouble(parameter ?? Math.E, culture);
//                var result = Math.Pow(@base, number);
//                return Convertible.ChangeType(result, targetType, culture);
//            }
//            catch (Exception) { return DependencyProperty.UnsetValue; }
//        }
//    }
//}
