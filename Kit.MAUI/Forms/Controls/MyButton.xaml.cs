using CommunityToolkit.Maui.Core;
using System.ComponentModel;
using System.Windows.Input;

namespace Kit.Forms.Controls
{
    [Preserve(AllMembers = true)]
    [XamlCompilation(XamlCompilationOptions.Skip)]
    public partial class MyButton : ContentView
    {
        public new static readonly BindableProperty MarginProperty = BindableProperty.Create(
            propertyName: nameof(Margin), returnType: typeof(Thickness), declaringType: typeof(MyButton), defaultValue: new Thickness(0));
        public new Thickness Margin
        {
            get { return (Thickness)GetValue(MarginProperty); }
            set
            {
                SetValue(MarginProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty StackMarginProperty = BindableProperty.Create(
            propertyName: nameof(StackMargin), returnType: typeof(Thickness), declaringType: typeof(MyButton), defaultValue: new Thickness(0));
        public Thickness StackMargin
        {
            get { return (Thickness)GetValue(StackMarginProperty); }
            set
            {
                SetValue(StackMarginProperty, value);
                OnPropertyChanged();
            }
        }
        public static readonly BindableProperty StackVerticalOptionsProperty = BindableProperty.Create(
            propertyName: nameof(StackVerticalOptions), returnType: typeof(LayoutOptions), declaringType: typeof(MyButton), defaultValue: LayoutOptions.FillAndExpand);
        public LayoutOptions StackVerticalOptions
        {
            get { return (LayoutOptions)GetValue(StackVerticalOptionsProperty); }
            set
            {
                SetValue(StackVerticalOptionsProperty, value);
                OnPropertyChanged();
            }
        }
        public static readonly BindableProperty StackHorizontalOptionsProperty = BindableProperty.Create(
    propertyName: nameof(StackHorizontalOptionsProperty), returnType: typeof(LayoutOptions), declaringType: typeof(MyButton), defaultValue: LayoutOptions.FillAndExpand);
        public LayoutOptions StackHorizontalOptions
        {
            get { return (LayoutOptions)GetValue(StackHorizontalOptionsProperty); }
            set
            {
                SetValue(StackHorizontalOptionsProperty, value);
                OnPropertyChanged();
            }
        }


        public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(
            propertyName: nameof(CornerRadius), returnType: typeof(float), declaringType: typeof(MyButton), defaultValue: 0f);
        public float CornerRadius
        {
            get { return (float)GetValue(CornerRadiusProperty); }
            set
            {
                SetValue(CornerRadiusProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(
            propertyName: nameof(BorderColor), returnType: typeof(Color), declaringType: typeof(MyButton), defaultValue: Colors.Black);
        public Color BorderColor
        {
            get { return (Color)GetValue(BorderColorProperty); }
            set
            {
                SetValue(BorderColorProperty, value);
                OnPropertyChanged();
            }
        }

        public new static readonly BindableProperty PaddingProperty = BindableProperty.Create(
            propertyName: nameof(Padding), returnType: typeof(Thickness), declaringType: typeof(MyButton), defaultValue: new Thickness(0));
        public new Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set
            {
                SetValue(PaddingProperty, value);
                OnPropertyChanged();
            }
        }

        public new static readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(
            propertyName: nameof(BackgroundColor), returnType: typeof(Color), declaringType: typeof(MyButton), defaultValue: Colors.White);
        public new Color BackgroundColor
        {
            get { return (Color)GetValue(BackgroundColorProperty); }
            set
            {
                SetValue(BackgroundColorProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty NativeAnimationColorProperty = BindableProperty.Create(
            propertyName: nameof(NativeAnimationColor), returnType: typeof(Color), declaringType: typeof(MyButton), defaultValue: Color.FromHex("#00A0FF"));
        public Color NativeAnimationColor
        {
            get { return (Color)GetValue(NativeAnimationColorProperty); }
            set
            {
                SetValue(NativeAnimationColorProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty PressedBackgroundColorProperty = BindableProperty.Create(
            propertyName: nameof(PressedBackgroundColor), returnType: typeof(Color), declaringType: typeof(MyButton), defaultValue: Color.FromHex("#00A0FF"));
        public Color PressedBackgroundColor
        {
            get { return (Color)GetValue(PressedBackgroundColorProperty); }
            set
            {
                SetValue(PressedBackgroundColorProperty, value);
                OnPropertyChanged();
            }
        }



        public static readonly BindableProperty ImgSourceProperty = BindableProperty.Create(
            propertyName: nameof(ImgSource), returnType: typeof(ImageSource), declaringType: typeof(MyButton), defaultValue: null);

        [TypeConverter(typeof(Converters.MyImageSourceConverter))]
        public ImageSource ImgSource
        {
            get { return (ImageSource)GetValue(ImgSourceProperty); }
            set
            {
                this.Img.IsVisible = !(value is null);
                SetValue(ImgSourceProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty ImgHeightRequestProperty = BindableProperty.Create(
            propertyName: nameof(ImgHeightRequest), returnType: typeof(double), declaringType: typeof(MyButton), defaultValue: 30d);
        public double ImgHeightRequest
        {
            get { return (double)GetValue(ImgHeightRequestProperty); }
            set
            {
                SetValue(ImgHeightRequestProperty, value);
                OnPropertyChanged();
            }
        }


        public static readonly BindableProperty OrientationProperty = BindableProperty.Create(
            propertyName: nameof(Orientation), returnType: typeof(StackOrientation), declaringType: typeof(MyButton), defaultValue: StackOrientation.Horizontal);
        public StackOrientation Orientation
        {
            get { return (StackOrientation)GetValue(OrientationProperty); }
            set
            {
                SetValue(OrientationProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
            propertyName: nameof(FontSize), returnType: typeof(double), declaringType: typeof(MyButton), defaultValue: Device.GetNamedSize(NamedSize.Large, typeof(Label)));

        [TypeConverter(typeof(FontSizeConverter))]
        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set
            {
                SetValue(FontSizeProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
            propertyName: nameof(TextColor), returnType: typeof(Color), declaringType: typeof(MyButton), defaultValue: Colors.Black);
        public Color TextColor
        {
            get { return (Color)GetValue(TextColorProperty); }
            set
            {
                SetValue(TextColorProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty FontAttributesProperty = BindableProperty.Create(
            propertyName: nameof(FontAttributes), returnType: typeof(FontAttributes), declaringType: typeof(MyButton), defaultValue: FontAttributes.None);

        public FontAttributes FontAttributes
        {
            get { return (FontAttributes)GetValue(FontAttributesProperty); }
            set
            {
                SetValue(FontAttributesProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty TextProperty = BindableProperty.Create(
            propertyName: nameof(Text), returnType: typeof(string), declaringType: typeof(MyButton), defaultValue: string.Empty);
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set
            {
                this.Lbl.IsVisible = !(string.IsNullOrEmpty(value));
                if (!this.Lbl.IsVisible)
                {
                    this.Img.VerticalOptions =
                    this.Img.HorizontalOptions = LayoutOptions.Center;
                }
                SetValue(TextProperty, value);
                OnPropertyChanged();
            }
        }


        public event EventHandler Touched;

        public static readonly BindableProperty TouchedCommandProperty = BindableProperty.Create(
            propertyName: nameof(Text), returnType: typeof(ICommand), declaringType: typeof(MyButton), defaultValue: null);
        public ICommand TouchedCommand
        {
            get { return (ICommand)GetValue(TouchedCommandProperty); }
            set
            {
                SetValue(TouchedCommandProperty, value);
                OnPropertyChanged();
            }
        }

        public MyButton()
        {
            InitializeComponent();
            this.Frame.BindingContext = this;
        }
        private void TouchEffect_OnCompleted(object sender, TouchGestureCompletedEventArgs e)
        {
            Touched?.Invoke(this, e);
            TouchedCommand?.Execute(this);
        }
    }
}