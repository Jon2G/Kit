
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kit.Forms.Extensions;
using System.Windows.Input;

using AsyncAwaitBestPractices.MVVM;
using AsyncAwaitBestPractices;
using Kit.MAUI.Forms.Pages;
using ZXing.Net.Maui;
using BarcodeFormat = ZXing.Net.Maui.BarcodeFormat;

namespace Kit.Forms.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Lector : ContentView
    {
        public static readonly BindableProperty ColorProperty = BindableProperty.Create(
            propertyName: nameof(Color),
            returnType: typeof(Color), declaringType: typeof(Lector), defaultValue: Colors.Black);

        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set
            {
                SetValue(ColorProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
            propertyName: nameof(FontSize),
            returnType: typeof(double), declaringType: typeof(Lector), defaultValue: 14d);

        [TypeConverter(typeof(FontSizeConverter))]
        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set
            {
                SetValue(FontSizeProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty OnCodeReadCommandProperty =
            BindableProperty.Create(
              propertyName: nameof(OnCodeReadCommand),
              returnType: typeof(AsyncCommand<string>),
              declaringType: typeof(Lector),
              //defaultValue: new Xamarin.Forms.Command<string>(
              //    (x) => Log.Logger.Debug("Código leido:{0}", x)),
              defaultBindingMode: BindingMode.TwoWay,
              propertyChanged: OnCodeReadCommandPropertyChanged);

        private static void OnCodeReadCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is Lector lector)
            {
                lector.OnCodeReadCommand = newValue as AsyncCommand<string>;
            }
        }

        public AsyncCommand<string> OnCodeReadCommand
        {
            get => (AsyncCommand<string>)this.GetValue(Lector.OnCodeReadCommandProperty);
            set
            {
                SetValue(Lector.OnCodeReadCommandProperty, value);
                OnPropertyChanged();
            }
        }

        private string _Barcode;

        public string Barcode
        {
            get => _Barcode;
            set
            {
                _Barcode = value;
                OnPropertyChanged();
            }
        }

        private ICommand _OpenCameraCommand;

        public ICommand OpenCameraCommand
        {
            get => _OpenCameraCommand;
            private set
            {
                _OpenCameraCommand = value;
                OnPropertyChanged();
            }
        }

        public BarcodeFormat BarcodeFormats { get; set; }

        public static BarcodeFormat AllFormats
        {
            get
            {
                return

                    BarcodeFormat.Aztec | BarcodeFormat.Codabar | BarcodeFormat.Code39 |
                    BarcodeFormat.Code39 | BarcodeFormat.Code128 | BarcodeFormat.DataMatrix |
                    BarcodeFormat.Ean8 | BarcodeFormat.Ean13 | BarcodeFormat.Itf |
                    BarcodeFormat.MaxiCode | BarcodeFormat.Pdf417 | BarcodeFormat.QrCode |
                    BarcodeFormat.Rss14 | BarcodeFormat.RssExpanded | BarcodeFormat.UpcA |
                    BarcodeFormat.UpcE | BarcodeFormat.UpcEanExtension |
                    BarcodeFormat.Msi | BarcodeFormat.Plessey | BarcodeFormat.Imb;


            }
        }

        private ZXingScannerPage Page;
        private INavigation INavigation;
        private bool IsShell;
        private ICommand _CloseCommand;
        private ICommand CloseCommand => _CloseCommand ??= new Command(Close);

        public Lector() : this(AllFormats)
        {
        }

        public Lector(BarcodeFormat? BarcodeFormats)
        {
            InitializeComponent();
            Init(BarcodeFormats);
        }

        public void Init(BarcodeFormat? BarcodeFormats)
        {
            this.OpenCameraCommand = new Kit.Extensions.Command(OpenCamera);
            this.BarcodeFormats = BarcodeFormats ?? BarcodeFormat.QrCode | BarcodeFormat.Code128 | BarcodeFormat.Ean13;
        }

        private void Close()
        {
            if (IsShell)
            {
                INavigation.PopAsync();
            }
            else
            {
                INavigation.PopModalAsync();
            }
        }

        private void OnDisappearing(object sender, EventArgs e)
        {
            Page.OnScanResult -= OnScanResult;
            Page.Disappearing -= OnDisappearing;
            this.Page.ToolbarItems?.Clear();
            this.Page = null;
        }



        private ZXingScannerPage BuildPage()
        {
            this.Page = new ZXingScannerPage(new()
            {
                Formats = this.BarcodeFormats
            })
            {
                Title = "Leector de codigos de barras",
            };
            this.Page.ToolbarItems.Add(new ToolbarItem()
            {
                Text = "Cerrar",
                Command = CloseCommand
            });
            Page.OnScanResult += OnScanResult;
            Page.Disappearing += OnDisappearing;
            return this.Page;
        }

        private void OpenCamera()
        {
            if (this.Page is not null)
            {
                return;
            }
            this.IsShell = (Shell.Current is not null);
            this.INavigation = IsShell ? Shell.Current.Navigation : Microsoft.Maui.Controls.Application.Current.MainPage.Navigation;
            BuildPage();
            Device.BeginInvokeOnMainThread(() =>
            {
                if (this.IsShell)
                {
                    this.INavigation.PushAsync(Page);
                }
                else
                {
                    this.INavigation.PushModalAsync(new NavigationPage(Page) { BarTextColor = Colors.White, BarBackgroundColor = Colors.CadetBlue }, true);
                }
            });
        }
        private void OnScanResult(object sender, BarcodeDetectionEventArgs e)
        {

            this.Page.IsScanning = false;
            Device.BeginInvokeOnMainThread(() =>
            {
                var result = e.Results.FirstOrDefault()?.Value;
                CloseCommand.Execute(this);
                if (string.IsNullOrEmpty(result))
                {
                    Barcode = null;
                }
                else
                {
                    Barcode = result;
                }
                OnCodeReadCommand?.ExecuteAsync(Barcode).SafeFireAndForget();
            });
        }

        public async void Abrir()
        {
            if (this.BarcodeFormats is null || !this.BarcodeFormats.Any())
            {
                throw new WarningException("Please call Init before attemping to open this Reader");
            }
            await Permisos.EnsurePermission<Permissions.Camera>("Por favor permita el acceso");
            this.OpenCameraCommand.Execute(null);
        }
    }
}