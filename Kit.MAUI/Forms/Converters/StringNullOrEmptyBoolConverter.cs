namespace Kit.Forms.Converters
{
    public class StringNullOrEmptyBoolConverter : IValueConverter
    {
        public StringNullOrEmptyBoolConverter()
        {
            
        }
        public bool IsReversed { get; set; }

        /// <summary>Returns false if string is null or empty
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var s = value as string;
            bool result = string.IsNullOrEmpty(s?.Trim());
            if (IsReversed)
            {
                result = !result;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}