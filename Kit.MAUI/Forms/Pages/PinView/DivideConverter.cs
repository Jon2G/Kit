using System;
using System.Globalization;
using Xamarin.Forms;

namespace Kit.Forms.Pages.PinView
{
    internal class DivideConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double val)
            {
                int number = int.Parse(parameter.ToString());
                return val / number;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}