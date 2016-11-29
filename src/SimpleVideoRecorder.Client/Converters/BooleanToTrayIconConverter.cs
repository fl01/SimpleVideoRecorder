using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SimpleVideoRecorder.Client.Converters
{
    public class BooleanToTrayIconConverter : IValueConverter
    {
        private static BitmapImage recordingIcon = new BitmapImage(new Uri("pack://application:,,,/SimpleVideoRecorder.Client;component/Assets/Recording.ico")) { CacheOption = BitmapCacheOption.OnLoad };
        private static BitmapImage idleIcon = new BitmapImage(new Uri("pack://application:,,,/SimpleVideoRecorder.Client;component/Assets/Idle.ico")) { CacheOption = BitmapCacheOption.OnLoad };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return (bool)value ? recordingIcon : idleIcon;
            }

            throw new InvalidOperationException("Value is not a bool");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
