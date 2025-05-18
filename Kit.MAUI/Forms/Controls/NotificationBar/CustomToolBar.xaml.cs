
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Kit.Forms.Controls.NotificationBar
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CustomToolBar : ContentView
    {
        public static readonly BindableProperty LogoTextProperty = BindableProperty.Create(
            propertyName: nameof(LogoText), returnType: typeof(string), declaringType: typeof(CustomToolBar), defaultValue: null);
        public string LogoText
        {
            get { return (string)GetValue(LogoTextProperty); }
            set
            {
                SetValue(LogoTextProperty, value);
                OnPropertyChanged();
            }
        }


        public static readonly BindableProperty LogoProperty = BindableProperty.Create(
            propertyName: nameof(Logo), returnType: typeof(ImageSource), declaringType: typeof(CustomToolBar), defaultValue: null);

        [TypeConverter(typeof(Converters.MyImageSourceConverter))]
        public ImageSource Logo
        {
            get { return (ImageSource)GetValue(LogoProperty); }
            set
            {
                SetValue(LogoProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty IsLogoVisibleProperty = BindableProperty.Create(
            propertyName: nameof(IsLogoVisible), returnType: typeof(bool), declaringType: typeof(CustomToolBar), defaultValue: false);
        public bool IsLogoVisible
        {
            get { return (bool)GetValue(IsLogoVisibleProperty); }
            set
            {
                SetValue(IsLogoVisibleProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty IsProgressVisibleProperty = BindableProperty.Create(
            propertyName: nameof(IsProgressVisible), returnType: typeof(bool), declaringType: typeof(CustomToolBar), defaultValue: false);
        public bool IsProgressVisible
        {
            get { return (bool)GetValue(IsProgressVisibleProperty); }
            set
            {
                SetValue(IsProgressVisibleProperty, value);
                OnPropertyChanged();
            }
        }
        public static readonly BindableProperty IsSandwichVisibleProperty = BindableProperty.Create(
            propertyName: nameof(IsSandwichVisible), returnType: typeof(bool), declaringType: typeof(CustomToolBar), defaultValue: false);
        public bool IsSandwichVisible
        {
            get { return (bool)GetValue(IsSandwichVisibleProperty); }
            set
            {
                SetValue(IsSandwichVisibleProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty ProgressSourceProperty = BindableProperty.Create(
            propertyName: nameof(ProgressSource), returnType: typeof(float), declaringType: typeof(CustomToolBar), defaultValue: null);
        public float ProgressSource
        {
            get { return (float)GetValue(ProgressSourceProperty); }
            set
            {
                SetValue(ProgressSourceProperty, value);
                OnPropertyChanged();
            }
        }

        private void ToogleMenu(object sender, EventArgs e)
        {
            if (Application.Current.MainPage is FlyoutPage master)
            {
                master.IsPresented = !master.IsPresented;
            }
        }
        public CustomToolBar()
        {
            InitializeComponent();
        }

        private void WarningOffLine(VisualElement sender, EventArgs args)
        {
            Acr.UserDialogs.UserDialogs.Instance.Alert("No fue posible conectarse al servidor,esto puede impedir o dificultar la operación.", "Alerta", "Ok");
        }

        private void Click(object sender, EventArgs e)
        {
            if (sender is Grid grid && grid.BindingContext is Notificacion notificacion)
            {
                notificacion.Command.Execute(sender);
            }
        }

    }
}