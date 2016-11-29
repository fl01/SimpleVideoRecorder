using System;
using System.Globalization;
using System.Windows.Data;

namespace SimpleVideoRecorder.Client.Converters
{
    public class BooleanToInvertedBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }
}
