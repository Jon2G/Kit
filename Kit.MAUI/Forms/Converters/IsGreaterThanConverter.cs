using System.Globalization;

namespace Kit.Forms.Converters
{
    public class IsGreaterThanConverter : IValueConverter
    {
        public IsGreaterThanConverter()
        {

        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double intValue = System.Convert.ToDouble(value);
            double compareToValue = System.Convert.ToDouble(parameter);

            return intValue > compareToValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}