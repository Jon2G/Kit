using System.Globalization;

namespace Kit.Forms.Converters
{
    public class IfNullOrEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null || string.IsNullOrEmpty(value.ToString()))
            {
                return parameter;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
