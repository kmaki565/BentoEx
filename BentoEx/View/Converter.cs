using BentoEx.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BentoEx.View
{
    public class OrderStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (Enum.Equals(value, Bento.OrderStatus.ordered))
                    return "注文済み";
                else if (Enum.Equals(value, Bento.OrderStatus.expired))
                    return "受付終了";
                else
                    return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        // No need to implement converting back on a one-way binding 
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    // https://stackoverflow.com/questions/34337755/convert-enum-to-string-inside-textblock-text
    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string EnumString;
            try
            {
                EnumString = Enum.GetName((value.GetType()), value);
                return EnumString;
            }
            catch
            {
                return string.Empty;
            }
        }

        // No need to implement converting back on a one-way binding 
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToValueConverter<T> : IValueConverter
    {
        public T FalseValue { get; set; }
        public T TrueValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return FalseValue;
            else
                return (bool)value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value != null ? value.Equals(TrueValue) : false;
        }
    }
    public class BoolToStringConverter : BoolToValueConverter<String> { }
}
