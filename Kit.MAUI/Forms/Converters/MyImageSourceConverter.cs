using System.ComponentModel;
using System.Globalization;

namespace Kit.Forms.Converters
{
    [TypeConverter(typeof(ImageSource))]
    public class MyImageSourceConverter : TypeConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string sValue)
                if (string.IsNullOrEmpty(sValue))
                {
                    return null;
                }
                else
                {
                    if (Device.RuntimePlatform == Device.UWP)
                    {
                        return ImageSource.FromFile($"Resources/{value}");
                    }
                    return ImageSource.FromFile(sValue);
                }
            throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(ImageSource)));
        }
    }
}
