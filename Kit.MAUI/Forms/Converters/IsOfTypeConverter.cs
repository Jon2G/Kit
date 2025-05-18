using System.Globalization;

namespace Kit.Forms.Converters
{
    public class IsOfTypeConverter : IValueConverter
    {
        public bool IsReversed { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return false;
            }
            if (parameter is not Type type)
            {
                return false;
            }
            if (IsReversed)
            {
                return value.GetType() != type;
            }
            return value.GetType() == type;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}