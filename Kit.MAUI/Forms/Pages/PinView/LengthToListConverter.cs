using System.Globalization;

namespace Kit.Forms.Pages.PinView
{
    internal class LengthToListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int length)
            {
                return Enumerable.Range(1, length);
            }
            return new List<object>();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}