using System.ComponentModel;
namespace Kit.Forms.Controls
{
    public partial class MyImageButton : ImageButton
    {
        public static readonly BindableProperty ImgSourceProperty = BindableProperty.Create(
            propertyName: nameof(MySource), returnType: typeof(ImageSource), declaringType: typeof(MyImage), defaultValue: null);

        [TypeConverter(typeof(Converters.MyImageSourceConverter))]
        public ImageSource MySource
        {
            get { return (ImageSource)GetValue(ImgSourceProperty); }
            set
            {
                SetValue(ImgSourceProperty, value);
                OnPropertyChanged();
            }
        }

        public MyImageButton()
        {

        }
        public object Tag { get; set; }
    }
}
