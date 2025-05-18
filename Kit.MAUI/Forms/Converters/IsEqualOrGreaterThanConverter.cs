using System.Globalization;

namespace Kit.Forms.Converters
{
    public class IsEqualOrGreaterThanConverter : IValueConverter
    {
        public static readonly IValueConverter Instance = new IsEqualOrGreaterThanConverter();
        public IsEqualOrGreaterThanConverter()
        {

        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double intValue = System.Convert.ToDouble(value);
            double compareToValue = System.Convert.ToDouble(parameter);

            return intValue >= compareToValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}