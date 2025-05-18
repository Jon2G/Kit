using System.ComponentModel;
using System.IO;
using FFImageLoading.Maui;

namespace Kit.Forms.Controls
{
    public partial class MyImage : CachedImage
    {
        public static readonly BindableProperty MySourceProperty = BindableProperty.Create(
            propertyName: nameof(MySource), returnType: typeof(ImageSource), declaringType: typeof(MyImage), defaultValue: null,
          propertyChanged: ImgChanged);

        public static readonly BindableProperty CrossImageProperty = BindableProperty.Create(
            propertyName: nameof(CrossImage), returnType: typeof(Kit.Controls.CrossImage.CrossImage), declaringType: typeof(MyImage), defaultValue: null,
            propertyChanged: CrossImageChanged);

        private static void CrossImageChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is MyImage img)
            {
                if (newValue != null && newValue != oldValue)
                {
                    var imgSource = (newValue as CrossImage.CrossImage);
                    if (imgSource != null)
                        img.Source = ImageSource.FromStream(() => new MemoryStream(imgSource.ToArray()));

                }
            }
        }

        private static void ImgChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is MyImage img)
            {
                if (newValue != oldValue)
                {
                    img.Source = newValue as ImageSource;
                }
            }
        }

        [TypeConverter(typeof(Converters.MyImageSourceConverter))]
        public ImageSource MySource
        {
            get { return (ImageSource)GetValue(MySourceProperty); }
            set
            {
                SetValue(MySourceProperty, value);
                OnPropertyChanged();
            }
        }
        public Kit.Controls.CrossImage.CrossImage CrossImage
        {
            get { return (Kit.Controls.CrossImage.CrossImage)GetValue(CrossImageProperty); }
            set
            {
                SetValue(CrossImageProperty, value);
                OnPropertyChanged();
            }
        }


        public MyImage()
        {
            this.CacheType = FFImageLoading.Cache.CacheType.Disk;
            this.FadeAnimationForCachedImages = true;
        }
        public object Tag { get; set; }
    }
}
